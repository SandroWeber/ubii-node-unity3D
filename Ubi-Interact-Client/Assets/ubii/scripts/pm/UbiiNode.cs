using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.Devices;
using Ubii.Services;
using Ubii.TopicData;
using UnityEngine;

public class UbiiNode : MonoBehaviour, IUbiiClient
{
	public string nodeName = "Unity3D Ubii Node";
	public UbiiClient client;
	private ProcessingModuleManager processingModuleManager;
	private ProcessingModule processingModule;
	private TopicDataProxy topicdataProxy;

	async void Start()
	{
		await client.WaitForConnection();
		await Initialize();
	}

	private async Task Initialize()
	{
		await RegisterNode();
		await SubscribeSessions();
		// processingModuleManager = new ProcessingModuleManager(pm.id, ...)
	}

	private async Task RegisterNode()
	{
		// Separate client reg for the node as the client params are different
		ServiceReply regReply = await client.CallService(new ServiceRequest
		{
			Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_REGISTRATION,
			Client = new Ubii.Clients.Client
			{
				Name = nodeName,
				IsDedicatedProcessingNode = true,
				// TODO: ProcessingModules = ProcessingModuleStorage.GetAllSpecs();
			}
		});
		if (regReply.Client != null)
		{
			//this.clientSpec = regReply.Client --> is eig alles im client, aber wsl noch separat speichern weils anders ist als der client
		}
		topicdataProxy = new TopicDataProxy(this);
		processingModuleManager = new ProcessingModuleManager(client.GetID(), null, topicdataProxy);
	}

	private async Task SubscribeSessions()
	{
		await client.Subscribe(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.START_SESSION, OnStartSession);
		await client.Subscribe(UbiiConstants.Instance.DEFAULT_TOPICS.INFO_TOPICS.STOP_SESSION, OnStopSession);
	}

	private async void OnStartSession(TopicDataRecord msgSession)
	{
		Debug.Log(nameof(OnStartSession));
		Debug.Log(msgSession);
		List<ProcessingModule> localPMs = new List<ProcessingModule>();

		foreach (Ubii.Processing.ProcessingModule pm in msgSession.ProcessingModuleList.Elements)
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
			// TODO: this.processingModuleManager.ApplyIOMappings(msgSession.IoMappings, msgSession.id);
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
			// TODO: add Ids to classes and topicDataRecordif (pm.sessionId == msgSession.Id)
			{
				processingModuleManager.StopModule(pm);
				processingModuleManager.RemoveModule(pm);
			}
		}
	}

	#region Interface Implementation
	public Task<ServiceReply> CallService(ServiceRequest request)
	{
		return client.CallService(request);
	}

	public bool IsConnected()
	{
		return client.IsConnected();
	}

	public void Publish(TopicData topicdata)
	{
		client.Publish(topicdata);
	}

	public Task<bool> Subscribe(string topic, Action<TopicDataRecord> callback)
	{
		return client.Subscribe(topic, callback);
	}

	public Task<bool> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
	{
		return client.SubscribeRegex(regex, callback);
	}

	public Task<bool> Unsubscribe(string topic, Action<TopicDataRecord> callback)
	{
		return Unsubscribe(topic, callback);
	}

	public Task<ServiceReply> RegisterDevice(Device ubiiDevice)
	{
		return client.RegisterDevice(ubiiDevice);
	}

	public Task<ServiceReply> DeregisterDevice(Device ubiiDevice)
	{
		return client.RegisterDevice(ubiiDevice);
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

	public void Publish(string topic, object value, object type, Timestamp timestamp)
	{
		var msgTopicData = new TopicData
		{
			TopicDataRecord = new TopicDataRecord
			{
				Topic = topic,
				Timestamp = timestamp
			}
		};
		// TODO: How can this be translated? msgTopicData.TopicDataRecord[type] = value;
		ubiiNode.client.Publish(msgTopicData);
	}

	public void Pull(string topic)
	{
		// TODO: Translate how ? -> return ubiiNode?.topicData.pull(topic);
	}

	public async Task<bool> Subscribe(string topic, Action<TopicDataRecord> callback)
	{
		return await ubiiNode.Subscribe(topic, callback);
	}

	public void Unsubscribe(string token)
	{
		// TODO: Is this correct translation? -> ubiiNode.Unsubscribe(token, null);		#nodeJS: this.topicdata.unsubscribe(token)
	}
}
