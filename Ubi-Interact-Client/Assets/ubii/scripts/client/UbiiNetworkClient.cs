using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.TopicData;
using Ubii.Clients;
using Ubii.Devices;
using Ubii.Services;
using Ubii.Servers;
using Ubii.Services.Request;
using Google.Protobuf.Collections;
using UnityEngine;
using System.Threading;

/// <summary>
/// This class manages network connections based on NetMQ to the Ubi-Interact master node.
/// </summary>
public class UbiiNetworkClient
{

    public enum SERVICE_CONNECTION_MODE
    {
        ZEROMQ = 0,
        HTTP = 1,
        HTTPS = 2
    }
    private SERVICE_CONNECTION_MODE serviceConnectionMode = SERVICE_CONNECTION_MODE.ZEROMQ;

    public enum TOPICDATA_CONNECTION_MODE
    {
        ZEROMQ = 0,
        HTTP = 1,
        HTTPS = 2
    }
    private TOPICDATA_CONNECTION_MODE topicDataConnectionMode = TOPICDATA_CONNECTION_MODE.ZEROMQ;
    private string serviceAddress = "localhost:8101";
    private string topicDataAddress = "localhost:8103";


    private Client clientSpecification;

    private IUbiiServiceClient serviceClient = null;

    private ITopicDataClient topicDataClient = null;

    private Server serverSpecification = null;

    public UbiiNetworkClient(SERVICE_CONNECTION_MODE serviceConnectionMode, string serviceAddress, TOPICDATA_CONNECTION_MODE topicDataConnectionMode, string topicDataAddress)
    {
        this.serviceConnectionMode = serviceConnectionMode;
        this.serviceAddress = serviceAddress;
        this.topicDataConnectionMode = topicDataConnectionMode;
        this.topicDataAddress = topicDataAddress;
    }

    #region Initialize/Teardown Functions

    // Initialize the ubiiClient, serviceClient and topicDataClient
    public async Task<Client> Initialize(Ubii.Clients.Client clientSpecs)
    {
        string hostURL = this.serviceAddress;
        Debug.Log("UBII - UbiiNetworkClient.Initialize() connecting to " + hostURL);
        if (this.serviceConnectionMode == SERVICE_CONNECTION_MODE.ZEROMQ)
        {
            serviceClient = new NetMQServiceClient(this.serviceAddress);
            Debug.Log("UBII - UbiiNetworkClient.Initialize() new NetMQServiceClient");
        }
        else if (this.serviceConnectionMode == SERVICE_CONNECTION_MODE.HTTP)
        {
            if (!hostURL.StartsWith("http://"))
            {
                hostURL = "http://" + hostURL;
            }
            serviceClient = new UbiiServiceClientHTTP(hostURL);
        }
        else if (this.serviceConnectionMode == SERVICE_CONNECTION_MODE.HTTPS)
        {
            if (!hostURL.StartsWith("https://"))
            {
                hostURL = "https://" + hostURL;
            }
            serviceClient = new UbiiServiceClientHTTP(hostURL);
        }

        if (serviceClient == null)
        {
            Debug.LogError("UBII - service connection client could not be created");
            return null;
        }


        this.serverSpecification = await RetrieveServerConfig();
        if (this.serverSpecification == null) return null;
        this.clientSpecification = await RegisterAsClient(clientSpecs);
        if (this.clientSpecification == null) return null;
        
        InitTopicDataClient();

        return clientSpecification;
    }

    private async Task<Ubii.Servers.Server> RetrieveServerConfig()
    {
        // Call Service to receive serverSpecifications
        ServiceRequest serverConfigRequest = new ServiceRequest { Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SERVER_CONFIG };

        ServiceReply reply = await CallService(serverConfigRequest);
        if (reply == null)
        {
            Debug.LogError("UBII - could not retrieve server configuration, reply is null");
            return null;
        }

        if (reply.Server != null)
        {
            return reply.Server;
        }
        else if (reply.Error != null)
        {
            Debug.LogError(reply.Error.ToString());
        }
        else
        {
            Debug.LogError("UBII UbiiNetworkClient - unkown server response during server specification retrieval");
        }

        return null;
    }

    private async Task<Ubii.Clients.Client> RegisterAsClient(Ubii.Clients.Client clientSpecs)
    {
        ServiceRequest clientRegistration = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_REGISTRATION,
            Client = clientSpecs
        };
        //if(isDedicatedProcessingNode)
        //  TODO:  clientRegistration.Client.ProcessingModules = ...

        ServiceReply reply = await CallService(clientRegistration);
        if (reply == null)
        {
            Debug.LogError("UBII UbiiNetworkClient.RegisterAsClient() - could not register client, response null");
            return null;
        }

        if (reply.Client != null)
        {
            return reply.Client;
        }
        else if (reply.Error != null)
        {
            Debug.LogError("UBII UbiiNetworkClient.RegisterAsClient() - server error:" + reply);
        }

        return null;
    }

    private void InitTopicDataClient()
    {
        if (this.topicDataConnectionMode == TOPICDATA_CONNECTION_MODE.ZEROMQ)
        {
            int port = int.Parse(serverSpecification.PortTopicDataZmq);
            this.topicDataClient = new NetMQTopicDataClient(clientSpecification.Id, topicDataAddress);
        }
        else if (this.topicDataConnectionMode == TOPICDATA_CONNECTION_MODE.HTTP)
        {
            string hostURL = topicDataAddress;
            if (!hostURL.StartsWith("ws://"))
            {
                hostURL = "ws://" + hostURL;
            }
            int port = int.Parse(serverSpecification.PortTopicDataWs);
            this.topicDataClient = new UbiiTopicDataClientWS(clientSpecification.Id, hostURL);
        }
        else if (this.topicDataConnectionMode == TOPICDATA_CONNECTION_MODE.HTTPS)
        {
            string hostURL = topicDataAddress;
            if (!hostURL.StartsWith("wss://"))
            {
                hostURL = "wss://" + hostURL;
            }
            int port = int.Parse(serverSpecification.PortTopicDataWs);
            this.topicDataClient = new UbiiTopicDataClientWS(clientSpecification.Id, hostURL);
        }

        if (this.topicDataClient == null)
        {
            Debug.LogError("UBII UbiiNetworkClient.InitTopicDataClient() - topic data client connection null");
        }
    }

    async public void ShutDown()
    {
        if (IsConnected())
        {
            await CallService(new Ubii.Services.ServiceRequest
            {
                Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_DEREGISTRATION,
                Client = clientSpecification
            });
        }
        serviceClient?.TearDown();
        topicDataClient?.TearDown();
    }

    #endregion

    public string GetClientID()
    {
        return clientSpecification?.Id;
    }

    public bool IsConnected()
    {
        return clientSpecification != null && clientSpecification.Id != null && topicDataClient != null && topicDataClient.IsConnected();
    }

    public void SetPublishDelay(int millisecs)
    {
        topicDataClient?.SetPublishDelay(millisecs);
    }

    public Task WaitForConnection()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;
        return Task.Run(() =>
        {
            int maxRetries = 100;
            int currentTry = 1;
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

    public async Task<ServiceReply> CallService(ServiceRequest srq)
    {
        return await serviceClient.CallService(srq);
    }

    public void Publish(TopicDataRecord record)
    {
        topicDataClient?.SendTopicDataRecord(record);
    }

    public void PublishImmediately(TopicDataRecord record)
    {
        topicDataClient?.SendTopicDataImmediately(new Ubii.TopicData.TopicData
        {
            TopicDataRecord = record
        });
    }

    public void PublishImmediately(TopicDataRecordList recordList)
    {
        topicDataClient?.SendTopicDataImmediately(new Ubii.TopicData.TopicData
        {
            TopicDataRecordList = recordList
        });
    }

    #region Devices
    public async Task<ServiceReply> RegisterDevice(Device ubiiDevice)
    {
        ServiceReply reply = await CallService(new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.DEVICE_REGISTRATION,
            Device = ubiiDevice
        });

        return reply;
    }

    public async Task<ServiceReply> DeregisterDevice(Device ubiiDevice)
    {
        ServiceReply reply = await CallService(new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.DEVICE_DEREGISTRATION,
            Device = ubiiDevice
        });

        if (reply.Error != null)
        {
            Debug.LogError("UBII UbiiNetworkClient.DeregisterDevice() - Deregister Device Error: " + reply.Error.Message);
        }

        return reply;
    }


    #endregion

    #region Subscriptions

    #region Topics
    public async Task<bool> SubscribeTopic(string topic, Action<TopicDataRecord> callback)
    {
        if (this.topicDataClient == null) return false;
        if (callback == null)
        {
            Debug.LogError("UBII UbiiNetworkClient.SubscribeTopic() - callback is NULL!");
            return false;
        }

        if (this.topicDataClient.IsSubscribed(topic))
        {
            topicDataClient.AddTopicCallback(topic, callback);
            return true;
        }

        // Repeated fields cannot be instantiated in SerivceRequest creation
        RepeatedField<string> subscribeTopics = new RepeatedField<string>();

        subscribeTopics.Add(topic);

        //Debug.Log("Subscribing to topic: " + topic + " (backend), subscribeRepeatedField: " + subscribeTopics.Count);
        ServiceRequest topicSubscription = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription
            {
                ClientId = clientSpecification.Id,
                SubscribeTopics = { subscribeTopics }
            }
        };

        // adding callback function to dictionary
        topicDataClient.AddTopicCallback(topic, callback);

        ServiceReply subReply = await CallService(topicSubscription);
        //Debug.Log("UbiiNetworkClient.SubscribeTopic() - " + topic + " - reply: " + subReply);
        if (subReply.Error != null)
        {
            Debug.LogError("UBII UbiiNetworkClient.SubscribeTopic() - Server Error: " + subReply.Error.ToString());
            topicDataClient.RemoveTopicCallback(topic, callback);
            return false;
        }

        return true;
    }

    public async Task<bool> UnsubscribeTopic(string topic, Action<TopicDataRecord> callback)
    {
        if (this.topicDataClient == null) return false;
        if (callback == null)
        {
            Debug.LogError("UBII UbiiNetworkClient.UnsubscribeTopic() - callback is NULL!");
            return false;
        }

        // removing callback function for this topic from dictionary
        topicDataClient.RemoveTopicCallback(topic, callback);

        // check if there are any callbacks left for this topic, if not, unsubscribe from topic
        if (!this.topicDataClient.HasTopicCallbacks(topic))
        {
            return await UnsubscribeTopic(new List<string>() { topic });
        }

        return true;
    }
    private async Task<bool> UnsubscribeTopic(List<string> topics)
    {
        // Repeated fields cannot be instantiated in SerivceRequest creation; adding topic to unsubscribeTopics which is later sent to server with clientID
        RepeatedField<string> unsubscribeTopics = new RepeatedField<string>();
        unsubscribeTopics.Add(topics);
        ServiceRequest topicUnsubscription = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription
            {
                ClientId = clientSpecification.Id,
                UnsubscribeTopics = { unsubscribeTopics }
            }
        };

        ServiceReply subReply = await CallService(topicUnsubscription);
        if (subReply.Error != null)
        {
            Debug.LogError("UBII UbiiNetworkClient.UnsubscribeTopic() - Server Error: " + subReply.Error.ToString());
            return false;
        }

        foreach (string topic in topics)
        {
            topicDataClient.RemoveAllTopicCallbacks(topic);
        }
        return true;
    }

    #endregion

    #region Regex
    public async Task<bool> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        if (callback == null)
        {
            Debug.LogError("UBII UbiiNetworkClient.SubscribeRegex() - callback is NULL!");
            return false;
        }

        if (this.topicDataClient.IsSubscribed(regex))
        {
            topicDataClient.AddTopicRegexCallback(regex, callback);
            return true;
        }

        //Debug.Log("Subscribing to topic: " + topic + " (backend), subscribeRepeatedField: " + subscribeTopics.Count);
        ServiceRequest subscriptionRequest = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription { ClientId = clientSpecification.Id }
        };
        subscriptionRequest.TopicSubscription.SubscribeTopicRegexp.Add(regex);

        ServiceReply subReply = await CallService(subscriptionRequest);

        if (subReply.Error != null)
        {
            Debug.LogError("UBII UbiiNetworkClient.SubscribeRegex() - Server Error: " + subReply.Error.ToString());
            return false;
        }

        // adding callback function to dictionary
        topicDataClient.AddTopicRegexCallback(regex, callback);

        return true;
    }

    public async Task<bool> UnsubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        if (callback == null)
        {
            Debug.LogError("UBII UbiiNetworkClient.UnsubscribeRegex() - callback is NULL!");
            return false;
        }

        // removing callback function from dictionary
        topicDataClient.RemoveTopicRegexCallback(regex, callback);

        if (!this.topicDataClient.HasTopicRegexCallbacks(regex))
        {
            // remove regex sub completely as no callbacks are left
            return await UnsubscribeRegex(regex);
        }
        return true;
    }

    private async Task<bool> UnsubscribeRegex(string regex)
    {
        ServiceRequest unsubscribeRequest = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription { ClientId = clientSpecification.Id }
        };
        unsubscribeRequest.TopicSubscription.UnsubscribeTopicRegexp.Add(regex);

        ServiceReply subReply = await CallService(unsubscribeRequest);

        if (subReply.Error != null)
        {
            Debug.LogError("UBII UbiiNetworkClient.UnsubscribeRegex() - Server Error: " + subReply.Error.ToString());
            return false;
        }
        // remove regex from topicdataclient
        topicDataClient.RemoveAllTopicRegexCallbacks(regex);
        return true;
    }

    #endregion

    #endregion
}
