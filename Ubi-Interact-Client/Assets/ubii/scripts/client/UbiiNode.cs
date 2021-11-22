using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using System.Collections.Generic;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.UtilityFunctions.Parser;
using Ubii.Devices;
using System.Linq;

public class UbiiNode : MonoBehaviour, IUbiiNode
{
    public delegate void InitializedEventHandler();
    public static event InitializedEventHandler OnInitialized;

    public delegate void ConnectEventHandler();
    public static event ConnectEventHandler OnConnected;

    [Header("Network configuration")]
    [Tooltip("Host ip the client connects to. Default is localhost.")]
    public string ip = "localhost";
    [Tooltip("Port for the client connection to the server. Default is 8101.")]
    public int port = 8101;
    [Tooltip("Name for the client connection to the server. Default is Unity3D Client.")]
    public string clientName = "Unity3D Client Node";
    [Tooltip("Which method to use for topic data connection.")]
    public UbiiNetworkClient.TOPICDATA_CONNECTION_MODE topicDataConnectionMode = UbiiNetworkClient.TOPICDATA_CONNECTION_MODE.ZEROMQ;

    [Tooltip("Automatically connect on start.")]
    public bool autoConnect = true;
    [Tooltip("Ubi-Interact node is used exclusively for processing modules.")]
    public bool isDedicatedProcessingNode = false;

    [Tooltip("Sets the delay for publishing records")]
    [Range(1, 5000)]
    public int publishDelay = 25;
    private Ubii.Clients.Client clientNodeSpecification;
    private UbiiNetworkClient networkClient;

    private ProcessingModuleDatabase _processingModuleDatabase = new ProcessingModuleDatabase();
    public ProcessingModuleDatabase processingModuleDatabase { get { return _processingModuleDatabase; } }
    private ProcessingModuleManager processingModuleManager;
    public ProcessingModuleManager ProcessingModuleManager { get { return processingModuleManager; } }
    private TopicDataProxy topicDataProxy;
    private TopicDataBuffer topicData = new TopicDataBuffer();

    private Dictionary<string, Device> registeredDevices = new Dictionary<string, Device>();

    #region unity

    private void Start()
    {
        if (autoConnect)
        {
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

        }
    }

    private async void OnDisable()
    {
        await DeregisterAllDevices();
        if (networkClient != null)
        {
            networkClient.ShutDown();
        }
        Debug.Log("Shutting down UbiiClient");
    }

    #endregion

    #region initialization

    public async Task Initialize()
    {
        clientNodeSpecification = new Ubii.Clients.Client
        {
            Name = clientName,
            IsDedicatedProcessingNode = isDedicatedProcessingNode
        };

        List<Ubii.Processing.ProcessingModule> pmDatabaseList = processingModuleDatabase.GetAllSpecifications();
        //Debug.Log("Node init PM list: " + pmDatabaseList.Count);
        foreach (Ubii.Processing.ProcessingModule pm in pmDatabaseList)
        {
            clientNodeSpecification.ProcessingModules.Add(pm);
        }

        bool success = await InitNetworkConnection();
        networkClient.SetPublishDelay(publishDelay);
        if (success)
        {
            processingModuleManager = new ProcessingModuleManager(this.GetID(), null, this.processingModuleDatabase, this.topicDataProxy);
            await SubscribeSessionInfo();
            OnInitialized?.Invoke();
            Debug.Log("Node client specs: " + clientNodeSpecification);
        }
        else
        {
            Debug.LogError("UbiiNode.Initialize() - failed to establish network connection to master node");
        }
    }

    public async Task<bool> InitNetworkConnection()
    {
        networkClient = new UbiiNetworkClient(ip, port, this.topicDataConnectionMode);
        clientNodeSpecification = await networkClient.Initialize(clientNodeSpecification);
        if (clientNodeSpecification == null)
        {
            return false;
        }

        OnConnected?.Invoke();
        this.topicDataProxy = new TopicDataProxy(topicData, networkClient);
        return true;
    }

    #endregion

    public string GetID()
    {
        return clientNodeSpecification.Id;
    }

    public bool IsConnected()
    {
        return networkClient.IsConnected();
    }

    public Task WaitForConnection()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;
        return Task.Run(() =>
        {
            int maxRetries = 100;
            int currentTry = 1;
            while (networkClient == null && currentTry <= maxRetries)
            {
                currentTry++;
                Thread.Sleep(100);
            }

            while (!IsConnected() && currentTry <= maxRetries)
            {
                currentTry++;
                Thread.Sleep(100);
            }

            if (currentTry > maxRetries)
            {
                cts.Cancel();
            }
        }, token);
    }

    /// <summary>
    /// Generates a timestamp
    /// </summary>
    /// <returns></returns>
    public Timestamp GenerateTimeStamp()
    {
        // TODO: Should be the same as in nodeJS implementation
        return new Timestamp
        {
            Seconds = DateTime.Now.Second,
            Nanos = (int)DateTime.Now.Ticks
        };
    }

    public Task<ServiceReply> CallService(ServiceRequest request)
    {
        return networkClient.CallService(request);
    }

    public void Publish(TopicDataRecord record)
    {
        topicDataProxy.Publish(record);
    }

    public void Publish(TopicDataRecordList recordList)
    {
        if (recordList == null || recordList.Elements == null) return;
        
        foreach (TopicDataRecord record in recordList.Elements)
        {
            this.Publish(record);
        }
    }

    public void Publish(TopicDataRecord[] recordsArray)
    {
        foreach (TopicDataRecord record in recordsArray)
        {
            this.Publish(record);
        }
    }

    public void PublishImmediately(TopicDataRecord record)
    {
        topicDataProxy.PublishImmediately(record);
    }

    public void SetPublishDelay(int millisecs)
    {
        networkClient.SetPublishDelay(millisecs);
    }

    public Task<SubscriptionToken> SubscribeTopic(string topic, Action<TopicDataRecord> callback)
    {
        return topicDataProxy.SubscribeTopic(topic, callback);
    }

    public Task<SubscriptionToken> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        return topicDataProxy.SubscribeRegex(regex, callback);
    }

    public Task<bool> Unsubscribe(SubscriptionToken token)
    {
        return this.topicDataProxy.Unsubscribe(token);
    }

    public async Task<ServiceReply> RegisterDevice(Ubii.Devices.Device ubiiDevice)
    {
        ServiceReply deviceRegReply = await networkClient.RegisterDevice(ubiiDevice);

        if (deviceRegReply.Error == null)
            registeredDevices.Add(deviceRegReply.Device.Id, deviceRegReply.Device);

        return deviceRegReply;
    }

    public async Task<ServiceReply> DeregisterDevice(Ubii.Devices.Device ubiiDevice)
    {
        var deviceDeregReply = await networkClient.DeregisterDevice(ubiiDevice);
        if (!registeredDevices.Remove(ubiiDevice.Id))
            Debug.LogError("Device " + ubiiDevice.Name + " could not be removed from local list.");
        else
            Debug.Log("Deregistering " + ubiiDevice + " successful!");
        return deviceDeregReply;
    }

    private async Task DeregisterAllDevices()
    {
        foreach (Device device in registeredDevices.Values.ToList())
        {
            await DeregisterDevice(device);
        }
    }

    /// <summary>
    /// Subscribes session start/stop
    /// </summary>
    /// <returns></returns>
    private async Task SubscribeSessionInfo()
    {
        await SubscribeTopic(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.START_SESSION, OnStartSession);
        await SubscribeTopic(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.STOP_SESSION, OnStopSession);
    }

    /// <summary>
    /// Callback for start session subscription
    /// </summary>
    /// <param name="record"></param>
    public async void OnStartSession(TopicDataRecord record)
    {
        List<ProcessingModule> localPMs = new List<ProcessingModule>();
        foreach (Ubii.Processing.ProcessingModule pm in record.Session.ProcessingModules)
        {
            if (pm.NodeId == this.GetID())
            {
                //Debug.Log("UbiiNode.OnStartSession() - applicable pm: " + pm);
                ProcessingModule newModule = this.processingModuleManager.CreateModule(pm);
                //Debug.Log("UbiiNode.OnStartSession() - created instance: " + newModule.ToString());
                if (newModule != null) localPMs.Add(newModule);
            }
        }

        Google.Protobuf.Collections.RepeatedField<Ubii.Processing.ProcessingModule> elements = new Google.Protobuf.Collections.RepeatedField<Ubii.Processing.ProcessingModule>();
        foreach (ProcessingModule pm in localPMs)
        {
            elements.Add(pm.ToProtobuf());
        }
        ServiceRequest pmRuntimeAddRequest = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.PM_RUNTIME_ADD,
            ProcessingModuleList = new Ubii.Processing.ProcessingModuleList
            {
                Elements = { elements }
            }
        };
        //Debug.Log(nameof(OnStartSession) + " - runtime add request: " + pmRuntimeAddRequest);

        ServiceReply reply = await CallService(pmRuntimeAddRequest);
        //Debug.Log("start session runtime add PMs reply: " + reply);
        if (reply.Success != null)
        {
            try
            {
                bool success = await this.processingModuleManager.ApplyIOMappings(record.Session.IoMappings, record.Session.Id);
                foreach (var pm in localPMs)
                {
                    this.processingModuleManager.StartModule(new Ubii.Processing.ProcessingModule { Id = pm.Id });
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        else
        {
            //TODO: delete modules 
        }
    }

    /// <summary>
    /// Callback for stop session subscription
    /// </summary>
    /// <param name="msgSession"></param>
    public void OnStopSession(TopicDataRecord msgSession)
    {
        foreach (ProcessingModule pm in this.processingModuleManager.processingModules.Values)
        {
            if (pm.SessionId == msgSession.Session.Id)
            {
                this.processingModuleManager.StopModule(new Ubii.Processing.ProcessingModule { Id = pm.Id });
                this.processingModuleManager.RemoveModule(pm);
            }
        }

        /*Google.Protobuf.Collections.RepeatedField<Ubii.Processing.ProcessingModule> elements = new Google.Protobuf.Collections.RepeatedField<Ubii.Processing.ProcessingModule>();
        foreach (ProcessingModule pm in localPMs)
        {
            elements.Add(pm.ToProtobuf());
        }
        ServiceRequest pmRuntimeAddRequest = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.PM_RUNTIME_REMOVE,
            ProcessingModuleList = new Ubii.Processing.ProcessingModuleList
            {
                Elements = { elements }
            }
        };
        //Debug.Log(nameof(OnStartSession) + " - runtime add request: " + pmRuntimeAddRequest);

        ServiceReply reply = await CallService(pmRuntimeAddRequest);
        //Debug.Log("start session runtime add PMs reply: " + reply);
        if (reply.Success != null)
        {
            try
            {
                bool success = await this.processingModuleManager.ApplyIOMappings(record.Session.IoMappings, record.Session.Id);
                foreach (var pm in localPMs)
                {
                    this.processingModuleManager.StartModule(new Ubii.Processing.ProcessingModule { Id = pm.Id });
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        else
        {
            //TODO: delete modules 
        }*/
    }
}

