using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.UtilityFunctions.Parser;
using Ubii.Devices;
using System.Collections.Generic;

public class UbiiNode : MonoBehaviour, IUbiiNode
{
    public delegate void InitializedEventHandler();
    public static event InitializedEventHandler OnInitialized;


    [Header("Network configuration")]
    [Tooltip("Host ip the client connects to. Default is localhost.")]
    public string ip = "localhost";
    [Tooltip("Port for the client connection to the server. Default is 8101.")]
    public int port = 8101;
    [Tooltip("Name for the client connection to the server. Default is Unity3D Client.")]
    public string clientName = "Unity3D Client Node";
    public bool isUbiiNode;

    protected NetMQUbiiClient networkClient;
    private ProcessingModuleManager processingModuleManager;
    private TopicDataProxy topicdataProxy;
    private TopicDataBuffer topicData = new TopicDataBuffer();

    async private void Start()
    {
        await InitializeClient();
        await SubscribeSessionInfo();
    }

    public async Task InitializeClient()
    {
        networkClient = new NetMQUbiiClient(clientName, ip, port, topicData);
        await networkClient.Initialize(isUbiiNode);
        OnInitialized();
    }

    public string GetID()
    {
        return networkClient.GetClientID();
    }

    public Task<ServiceReply> CallService(ServiceRequest request)
    {
        return networkClient.CallService(request);
    }

    public void Publish(TopicData topicData)
    {
        networkClient.Publish(topicData);
    }

    public async Task<SubscriptionToken> Subscribe(string topic, Action<TopicDataRecord> callback)
    {
        List<SubscriptionToken> subscriptions = topicData.GetTopicSubscriptionTokens(topic);
        if (subscriptions == null || subscriptions.Count == 0)
            await networkClient.SubscribeTopic(topic, callback);

        return topicData.Subscribe(topic, callback); // Add to a dictionary as well?
    }

    public Task<bool> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        List<SubscriptionToken> subscriptions = topicData.GetRegexSubscriptionTokens(regex);
        if (subscriptions == null || subscriptions.Count == 0)
            await networkClient.SubscribeRegex(regex, callback);

        return topicData.SubscribeRegex(regex, callback);
    }

    public Task<bool> Unsubscribe(string topic, Action<TopicDataRecord> callback)
    {
        return networkClient.UnsubscribeTopic(topic, callback);
    }

    public Task<ServiceReply> RegisterDevice(Ubii.Devices.Device ubiiDevice)
    {
        return networkClient.RegisterDevice(ubiiDevice);
    }

    public Task<ServiceReply> DeregisterDevice(Ubii.Devices.Device ubiiDevice)
    {
        return networkClient.DeregisterDevice(ubiiDevice);
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

    private void OnDisable()
    {
        if (networkClient != null)
        {
            networkClient.ShutDown();
        }
        Debug.Log("Shutting down UbiiClient");
    }

    /// <summary>
    /// Generates a timestamp
    /// </summary>
    /// <returns></returns>
    private Timestamp GenerateTimeStamp()
    {
        // TODO: Should be the same as in nodeJS implementation
        return new Timestamp
        {
            Seconds = DateTime.Now.Second,
            Nanos = (int)DateTime.Now.Ticks
        };
    }

    /// <summary>
    /// Subscribes session start/stop
    /// </summary>
    /// <returns></returns>
    private async Task SubscribeSessionInfo()
    {
        await Subscribe(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.START_SESSION, OnStartSession);
        await Subscribe(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.STOP_SESSION, OnStopSession);
    }

    /// <summary>
    /// Callback for start session subscription
    /// </summary>
    /// <param name="record"></param>
    private async void OnStartSession(TopicDataRecord record)
    {
        Debug.Log(nameof(OnStartSession));
        Debug.Log(record);
        List<ProcessingModule> localPMs = new List<ProcessingModule>();

        foreach (Ubii.Processing.ProcessingModule pm in record.ProcessingModuleList.Elements)
        {
            if (pm.NodeId == this.processingModule.nodeID)
            {
                ProcessingModule newModule = this.processingModuleManager.CreateModule(pm);
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

        ServiceReply reply = await CallService(pmRuntimeAddRequest);
        if (reply.Success != null)
        {
            
            this.processingModuleManager.ApplyIOMappings(record.Session.IoMappings, record.Session.Id);
            foreach (var pm in localPMs)
            {
                this.processingModuleManager.StartModule(pm);
            }
        }
    }

    /// <summary>
    /// Callback for stop session subscription
    /// </summary>
    /// <param name="msgSession"></param>
    private void OnStopSession(TopicDataRecord msgSession)
    {
        Debug.Log(nameof(OnStopSession));
        foreach (ProcessingModule pm in processingModuleManager.processingModules.Values)
        {
            if (pm.sessionID == msgSession.Session.Id)
            {
                processingModuleManager.StopModule(pm);
                processingModuleManager.RemoveModule(pm);
            }
        }
    }
}

