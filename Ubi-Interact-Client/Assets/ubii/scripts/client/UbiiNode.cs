using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Ubii.Services;
using Ubii.TopicData;
using Ubii.Devices;

public class UbiiNode : MonoBehaviour, IUbiiNode
{

    const int CONNECTION_RETRY_INCREMENT_SECONDS = 5;
    const int CONNECTION_RETRY_MAX_DELAY_SECONDS = 30;

    public delegate void InitializedEventHandler();
    public static event InitializedEventHandler OnInitialized;

    public delegate void ConnectEventHandler();
    public static event ConnectEventHandler OnConnected;

    [Tooltip("Name for the client connection to the server. Default is Unity3D Client.")]
    public string clientName = "Unity3D Client Node";

    [Header("Network configuration")]

    [Tooltip("Which method to use for service connection.")]
    public UbiiNetworkClient.SERVICE_CONNECTION_MODE serviceConnectionMode = UbiiNetworkClient.SERVICE_CONNECTION_MODE.HTTPS;
    [Tooltip("Which method to use for topic data connection.")]
    public UbiiNetworkClient.TOPICDATA_CONNECTION_MODE topicDataConnectionMode = UbiiNetworkClient.TOPICDATA_CONNECTION_MODE.HTTPS;

    [Tooltip("Automatically connect on start.")]
    public bool autoConnect = true;
    [Tooltip("Ubi-Interact node is used exclusively for processing modules.")]
    public bool isDedicatedProcessingNode = false;

    [Tooltip("Sets the delay for publishing records")]
    [Range(1, 5000)]
    public int msPublishInterval = 25;

    [Tooltip("Address for Master Node service connection. IMPORTANT: UWP can only connect via the binary endpoint (NOT json).")]
    public string serviceAddress = UbiiNetworkClient.DEFAULT_LOCALHOST_ADDRESS_SERVICE_HTTP;
    [Tooltip("Address for Master Node topic data connection.")]
    public string topicDataAddress = UbiiNetworkClient.DEFAULT_LOCALHOST_ADDRESS_TOPICDATA_WS;

    private Ubii.Clients.Client clientNodeSpecification;
    private UbiiNetworkClient networkClient;

    private ProcessingModuleDatabase _processingModuleDatabase = new ProcessingModuleDatabase();
    public ProcessingModuleDatabase processingModuleDatabase { get { return _processingModuleDatabase; } }
    private ProcessingModuleManager processingModuleManager;
    public ProcessingModuleManager ProcessingModuleManager { get { return processingModuleManager; } }
    private TopicDataProxy topicDataProxy;
    private TopicDataBuffer topicData = new TopicDataBuffer();

    private Dictionary<string, Device> registeredDevices = new Dictionary<string, Device>();
    private CancellationTokenSource ctsInitConnection = null;

    public string Id
    {
        get { return clientNodeSpecification.Id; }
    }

    public string Name
    {
        get { return clientName; }
    }

    #region unity

    private async void Start()
    {
        if (autoConnect)
        {
            try
            {
                await Initialize(serviceConnectionMode, serviceAddress, topicDataConnectionMode, topicDataAddress);
            }
            catch (Exception e)
            {
                Debug.LogError("UBII UbiiNode.Initialize(): " + e.ToString());
            }
        }
    }

    private async void OnDisable()
    {
        ctsInitConnection?.Cancel();
        topicDataProxy?.StopPublishing();

        await DeregisterAllDevices();
        if (networkClient != null)
        {
            await networkClient.ShutDown();
        }
        Debug.Log("UBII - Shutting down UbiiClient");
    }

    #endregion

    #region connection

    public async Task Initialize(
        UbiiNetworkClient.SERVICE_CONNECTION_MODE serviceConnectionMode = UbiiNetworkClient.DEFAULT_SERVICE_CONNECTION_MODE,
        string serviceAddress = UbiiNetworkClient.DEFAULT_LOCALHOST_ADDRESS_SERVICE_HTTP,
        UbiiNetworkClient.TOPICDATA_CONNECTION_MODE topicDataConnectionMode = UbiiNetworkClient.DEFAULT_TOPICDATA_CONNECTION_MODE,
        string topicDataAddress = UbiiNetworkClient.DEFAULT_LOCALHOST_ADDRESS_TOPICDATA_WS)
    {
        UbiiConstants constants = UbiiConstants.Instance;  // needs to be instantiated on main thread
        this.InitClientSpecification();

        this.ctsInitConnection = new CancellationTokenSource();
        bool connected = false;
        try
        {
            connected = await Task.Run(async () =>
            {
                int connectionTry = 0;
                bool success = false;
                while (!success && !this.ctsInitConnection.IsCancellationRequested)
                {
                    connectionTry++;
                    success = await InitNetworkConnection(serviceConnectionMode, serviceAddress, topicDataConnectionMode, topicDataAddress);
                    if (!success)
                    {
                        int delay = Math.Min(CONNECTION_RETRY_MAX_DELAY_SECONDS, connectionTry * CONNECTION_RETRY_INCREMENT_SECONDS);
                        Debug.LogError("UBII - failed to establish network connection to master node, retrying in " + delay + "s");
                        Task.Delay(delay * 1000).Wait(this.ctsInitConnection.Token);
                    }
                }

                return true;
            }, this.ctsInitConnection.Token);
        }
        catch (Exception ex)
        {
            Debug.LogError("UBII - connection task exception: " + ex.ToString());
        }

        if (connected)
        {
            processingModuleManager = new ProcessingModuleManager(this.Id, null, this.processingModuleDatabase, this.topicDataProxy);
            await SubscribeSessionInfo();
            OnConnected?.Invoke();
            OnInitialized?.Invoke();
        }
    }

    private void InitClientSpecification()
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
    }

    private async Task<bool> InitNetworkConnection(UbiiNetworkClient.SERVICE_CONNECTION_MODE serviceConnectionMode, string serviceAddress, UbiiNetworkClient.TOPICDATA_CONNECTION_MODE topicDataConnectionMode, string topicDataAddress)
    {
        networkClient = new UbiiNetworkClient(serviceConnectionMode, serviceAddress, topicDataConnectionMode, topicDataAddress);
        Ubii.Clients.Client serverClientSpecs = await networkClient.Initialize(clientNodeSpecification);
        if (serverClientSpecs == null)
        {
            return false;
        }
        else
        {
            clientNodeSpecification = serverClientSpecs;
        }

        topicData = new TopicDataBuffer();
        topicDataProxy = new TopicDataProxy(topicData, networkClient);
        topicDataProxy.SetPublishDelay(msPublishInterval);

        Debug.Log("UBII - client connected: " + clientNodeSpecification);
        return true;
    }

    public async Task<bool> Disconnect()
    {
        Debug.Log("UBII - Shutting down UbiiClient");
        ctsInitConnection?.Cancel();
        topicDataProxy?.StopPublishing();

        await DeregisterAllDevices();
        return await networkClient?.ShutDown();
    }

    /*private async void Reconnect()
    {
        Debug.Log("Reconnect()");
        await TearDown();
        Debug.Log("Reconnect() after teardown");

        Task.Run(async () =>
        {
            Debug.Log("Reconnect() task");
            int tries = 0;
            int msDelayNextTry = 100;
            while (!connected && !ctsConnect.IsCancellationRequested)
            {
                Debug.Log("Reconnect() - try=" + tries + ", connected=" + connected + ", delay=" + msDelayNextTry);
                tries++;
                await Initialize();
                if (!connected)
                {
                    await Task.Delay(msDelayNextTry, ctsConnect.Token);
                    msDelayNextTry = Math.Min(10000, msDelayNextTry * 2);
                }
                else
                {
                    Debug.Log("Reconnect() - try=" + tries + ", connected=" + connected + ", delay=" + msDelayNextTry);
                }
            }
        });
    }*/

    #endregion

    public bool IsConnected()
    {
        return networkClient != null && networkClient.IsConnected();
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

    public async Task<bool> PublishImmediately(TopicDataRecord record)
    {
        return await topicDataProxy.PublishImmediately(record);
    }

    public async Task<bool> PublishImmediately(TopicDataRecordList recordList)
    {
        return await topicDataProxy.PublishImmediately(recordList);
    }

    public void SetPublishInterval(int millisecs)
    {
        topicDataProxy.SetPublishDelay(millisecs);
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
        return this.topicDataProxy?.Unsubscribe(token);
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
            Debug.LogError("UBII UbiiNode.DeregisterDevice() - Device " + ubiiDevice.Name + " could not be removed from local list.");
        else
            Debug.Log("UBII - Deregistering " + ubiiDevice + " successful!");
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
    private async void OnStartSession(TopicDataRecord record)
    {
        List<ProcessingModule> localPMs = new List<ProcessingModule>();
        foreach (Ubii.Processing.ProcessingModule pm in record.Session.ProcessingModules)
        {
            if (pm.NodeId == this.Id)
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
                Debug.LogError("UBII UbiiNode.OnStartSession() - " + e.ToString());
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
    private void OnStopSession(TopicDataRecord msgSession)
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
                Debug.LogError("UBII UbiiNode.OnStopSession() - " + e.ToString());
            }
        }
        else
        {
            //TODO: delete modules 
        }*/
    }
}

