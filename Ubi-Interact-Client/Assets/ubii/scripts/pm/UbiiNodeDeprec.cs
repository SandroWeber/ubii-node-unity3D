using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.Devices;
using Ubii.Services;
using Ubii.TopicData;
using UnityEngine;

public class UbiiNodeDeprec : MonoBehaviour
{
    public string nodeName = "Unity3D Ubii Node";
    public UbiiNode ubiiNode;
    private ProcessingModuleManager processingModuleManager;
    private ProcessingModule processingModule;
    public TopicDataProxy topicdataProxy;
    public TopicDataBuffer topicData = new TopicDataBuffer();

    async void Start()
    {
        await ubiiNode.WaitForConnection();
        await Initialize();
    }

    /// <summary>
    /// Init topicDataProxy and PM Manager, subscribes sessions
    /// </summary>
    /// <returns>Task, async function</returns>
    private async Task Initialize()
    {
        topicdataProxy = new TopicDataProxy(topicData);
        processingModuleManager = new ProcessingModuleManager(ubiiNode.GetID(), null, topicdataProxy);
        await SubscribeSessions();
    }

    /// <summary>
    /// Subscribes session start/stop
    /// </summary>
    /// <returns></returns>
    private async Task SubscribeSessions()
    {
        await ubiiNode.Subscribe(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.START_SESSION, OnStartSession);
        await ubiiNode.Subscribe(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.STOP_SESSION, OnStopSession);
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
}
