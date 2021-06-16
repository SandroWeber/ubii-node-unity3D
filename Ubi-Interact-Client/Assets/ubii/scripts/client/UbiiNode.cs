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

    [Header("Network configuration")]
    [Tooltip("Host ip the client connects to. Default is localhost.")]
    public string ip = "localhost";
    [Tooltip("Port for the client connection to the server. Default is 8101.")]
    public int port = 8101;
    [Tooltip("Name for the client connection to the server. Default is Unity3D Client.")]
    public string clientName = "Unity3D Client Node";

    [Tooltip("Automatically connect on start.")]
    public bool autoConnect = true;
    [Tooltip("Ubi-Interact node is used exclusively for processing modules.")]
    public bool isDedicatedProcessingNode = false;

    protected NetMQUbiiClient networkClient;

    private ProcessingModuleDatabase processingModuleDatabase = new ProcessingModuleDatabase();
    private ProcessingModuleManager processingModuleManager;
    private TopicDataProxy topicdataProxy;
    private TopicDataBuffer topicData = new TopicDataBuffer();

    private Dictionary<string, Device> registeredDevices = new Dictionary<string, Device>();

    async private void Start()
    {
        if (autoConnect)
        {
            Connect();
        }
    }

    public async Task Connect()
    {
        await InitializeClient();
        await SubscribeSessionInfo();
        processingModuleManager = new ProcessingModuleManager(this.GetID(), null, null);
    }

    public async Task InitializeClient()
    {
        networkClient = new NetMQUbiiClient(clientName, ip, port);
        await networkClient.Initialize(isDedicatedProcessingNode);
        OnInitialized?.Invoke();
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

    public async Task<SubscriptionToken> SubscribeTopic(string topic, Action<TopicDataRecord> callback)
    {
        List<SubscriptionToken> subscriptions = topicData.GetTopicSubscriptionTokens(topic);
        if (subscriptions == null || subscriptions.Count == 0)
        {
            await networkClient.SubscribeTopic(topic, OnTopicDataRecord);
        }

        return topicData.SubscribeTopic(topic, callback); // Add to a dictionary as well?
    }

    public async Task<SubscriptionToken> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        List<SubscriptionToken> subscriptions = topicData.GetRegexSubscriptionTokens(regex);
        if (subscriptions == null || subscriptions.Count == 0)
        {
            bool success = await networkClient.SubscribeRegex(regex, OnTopicDataRecord);
        }

        return topicData.SubscribeRegex(regex, callback);
    }

    public async Task<bool> Unsubscribe(SubscriptionToken token)
    {
        this.topicData.Unsubscribe(token);

        if (token.type == SUBSCRIPTION_TOKEN_TYPE.TOPIC)
        {
            List<SubscriptionToken> subscriptions = topicData.GetTopicSubscriptionTokens(token.topic);
            if (subscriptions == null || subscriptions.Count == 0)
            {
                await networkClient.UnsubscribeTopic(token.topic, OnTopicDataRecord);
            }
        }
        else if (token.type == SUBSCRIPTION_TOKEN_TYPE.REGEX)
        {
            List<SubscriptionToken> subscriptions = topicData.GetRegexSubscriptionTokens(token.topic);
            if (subscriptions == null || subscriptions.Count == 0)
            {
                await networkClient.UnsubscribeRegex(token.topic, OnTopicDataRecord);
            }
        }

        return true;
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

    private async void OnDisable()
    {
        await DeregisterAllDevices();
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
        await SubscribeTopic(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.START_SESSION, OnStartSession);
        await SubscribeTopic(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.STOP_SESSION, OnStopSession);
    }

    /// <summary>
    /// Callback for start session subscription
    /// </summary>
    /// <param name="record"></param>
    private async void OnStartSession(TopicDataRecord record)
    {
        Debug.Log(nameof(OnStartSession));
        Debug.Log(record);
        /*List<ProcessingModule> localPMs = new List<ProcessingModule>();

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
        }*/
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

    private void OnTopicDataRecord(TopicDataRecord record)
    {
        this.topicData.Publish(record);
    }
}

