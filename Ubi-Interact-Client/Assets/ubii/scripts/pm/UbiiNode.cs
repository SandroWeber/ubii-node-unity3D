using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.Devices;
using Ubii.Services;
using Ubii.TopicData;
using UnityEngine;

public class UbiiNode : MonoBehaviour
{
    public string nodeName = "Unity3D Ubii Node";
    public UbiiClient ubiiNode;
    private ProcessingModuleManager processingModuleManager;
    private ProcessingModule processingModule;
    public TopicDataProxy topicdataProxy;
    public RuntimeTopicData topicData = new RuntimeTopicData();

    async void Start()
    {
        await ubiiNode.WaitForConnection();
        await Initialize();
    }

    private async Task Initialize()
    {
        topicdataProxy = new TopicDataProxy(this);
        processingModuleManager = new ProcessingModuleManager(ubiiNode.GetID(), null, topicdataProxy);
        await SubscribeSessions();
    }

    private async Task SubscribeSessions()
    {
        await ubiiNode.Subscribe(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.START_SESSION, OnStartSession);
        await ubiiNode.Subscribe(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.STOP_SESSION, OnStopSession);
    }

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

    #region Interface Implementation
    public Task<ServiceReply> CallService(ServiceRequest request)
    {
        return ubiiNode.CallService(request);
    }

    public bool IsConnected()
    {
        return ubiiNode.IsConnected();
    }

    public void Publish(TopicData topicdata)
    {
        ubiiNode.Publish(topicdata);
    }

    public async void Subscribe(string topic, Action<TopicDataRecord> callback)
    {
        List<SubscriptionToken> subscriptions = topicData.GetSubscriptionTokens(topic);
        if (subscriptions == null || subscriptions.Count == 0)
            await ubiiNode.Subscribe(topic, callback);

        SubscriptionToken token = topicData.Subscribe(topic, callback); // Add to a dictionary as well?
    }

    public Task<bool> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        return ubiiNode.SubscribeRegex(regex, callback);
    }

    public async void Unsubscribe(SubscriptionToken token)
    {
        topicData.Unsubscribe(token);
        List<SubscriptionToken> subscriptions = topicData.GetSubscriptionTokens(token.topic);
        if (subscriptions == null || subscriptions.Count == 0)
            await ubiiNode.Unsubscribe(token.topic, token.callback);
    }

    public Task<ServiceReply> RegisterDevice(Device ubiiDevice)
    {
        return ubiiNode.RegisterDevice(ubiiDevice);
    }

    public Task<ServiceReply> DeregisterDevice(Device ubiiDevice)
    {
        return ubiiNode.RegisterDevice(ubiiDevice);
    }
    #endregion

    private Timestamp GenerateTimeStamp()
    {
        // TODO: Should be the same as in nodeJS implementation
        return new Timestamp
        {
            Seconds = DateTime.Now.Second,
            Nanos = (int)DateTime.Now.Ticks
        };
    }
}

public class TopicDataProxy
{
    private UbiiNode ubiiNode;

    public TopicDataProxy(UbiiNode ubiiNode)
    {
        this.ubiiNode = ubiiNode;
    }

    public void Publish(TopicDataRecord topicDataRecord)
    {
        // publish = "push" from buffer
        ubiiNode.topicData.Push(topicDataRecord);
    }

    public TopicDataRecord Pull(string topic)
    {
        return ubiiNode.topicData.Pull(topic);
    }

    public SubscriptionToken Subscribe(string topic, Action<TopicDataRecord> callback)
    {
        return ubiiNode.topicData.Subscribe(topic, callback);
    }

    public void Unsubscribe(SubscriptionToken token)
    {
        ubiiNode.topicData.Unsubscribe(token);
    }
}
