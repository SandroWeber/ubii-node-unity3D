using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

using Ubii.Services;
using Ubii.TopicData;
using Ubii.Services.Request;
using Google.Protobuf.Collections;

namespace ubii
{
    public struct DEFAULT_TOPICS
    {
        public struct SERVICES
        {
            public const string CLIENT_REGISTRATION = "/services/client_registration";
            public const string DEVICE_REGISTRATION = "/services/device_registration";
            public const string SERVER_CONFIG = "/services/server_configuration";
            public const string SESSION_REGISTRATION = "/services/session/registration";
            public const string SESSION_START = "/services/session/start";
            public const string SESSION_STOP = "/services/session/stop";
            public const string TOPIC_LIST = "/services/topic_list";
            public const string TOPIC_SUBSCRIPTION = "/services/topic_subscription";
        }
    }
}

public class UbiiWebClient : MonoBehaviour, IUbiiClient
{
    public string host = "localhost";
    public int service_port = 8102;
    public string client_name = "unity client";

    private UbiiServiceClientREST service_client = null;
    private UbiiTopicDataClientWS topicdata_client = null;

    private Ubii.Servers.Server server_config = null;
    private Ubii.Clients.Client client_specification = null;

    void Start()
    {
        this.Initialize();
    }

    void OnDestroy()
    {
        if (topicdata_client != null)
        {
            topicdata_client.DeInitialize();
        }
    }

    public Task<ServiceReply> CallService(ServiceRequest request)
    {
        return this.service_client.CallService(request);
    }

    private async void Initialize()
    {
        this.service_client = new UbiiServiceClientREST(this.host, this.service_port);

        // get server configuration
        ServiceRequest serverConfigRequest = new ServiceRequest { Topic = ubii.DEFAULT_TOPICS.SERVICES.SERVER_CONFIG };
        ServiceReply reply = await this.service_client.CallService(serverConfigRequest);
        if (reply.Server == null)
        {
            Debug.LogError("CallService(SERVER_CONFIG) failed");
            return;
        }
        server_config = reply.Server;
        Debug.Log("server_config: " + server_config);

        // register client
        ServiceRequest clientRegistrationRequest = new ServiceRequest {
            Topic = ubii.DEFAULT_TOPICS.SERVICES.CLIENT_REGISTRATION,
            Client = new Ubii.Clients.Client { Name = client_name }
        };
        reply = await this.service_client.CallService(clientRegistrationRequest);
        if (reply.Client == null)
        {
            Debug.LogError("CallService(CLIENT_REGISTRATION) failed");
            return;
        }
        client_specification = reply.Client;
        Debug.Log("client_specification: " + client_specification);

        // setup topic data socket
        topicdata_client = new UbiiTopicDataClientWS(client_specification.Id, host, Int32.Parse(server_config.PortTopicDataWs));

        var test_topic = "/test/topic/unity";
        await Subscribe(test_topic, (TopicDataRecord record) =>
        {
            Debug.Log(record);
        });
        topicdata_client.SendTestTopicData(test_topic);
    }

    public void Publish(TopicData topicdata)
    {

    }

    public async Task<ServiceReply> Subscribe(string topic, Action<TopicDataRecord> callback)
    {
        RepeatedField<string> topic_subscriptions = new RepeatedField<string>();
        topic_subscriptions.Add(topic);
        ServiceRequest subscription_request = new ServiceRequest
        {
            Topic = ubii.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription
            {
                ClientId = client_specification.Id,
                SubscribeTopics = { topic_subscriptions }
            }
        };
        var reply = await service_client.CallService(subscription_request);

        if (reply.Success != null)
        {
            Debug.Log("subscribe to topic " + topic + " successful");
            topicdata_client.AddTopicDataCallback(topic, callback);
        }

		return reply;
    }

    public Task<ServiceReply> Unsubscribe(string topic)
    {
		return null;
    }
}
