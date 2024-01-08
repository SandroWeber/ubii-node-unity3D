using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using Ubii.TopicData;
using Ubii.Clients;
using Ubii.Devices;
using Ubii.Services;
using Ubii.Servers;
using Ubii.Services.Request;
using Google.Protobuf.Collections;

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
    public enum TOPICDATA_CONNECTION_MODE
    {
        ZEROMQ = 0,
        HTTP = 1,
        HTTPS = 2
    }

    public const SERVICE_CONNECTION_MODE DEFAULT_SERVICE_CONNECTION_MODE = SERVICE_CONNECTION_MODE.HTTP;
    public const TOPICDATA_CONNECTION_MODE DEFAULT_TOPICDATA_CONNECTION_MODE = TOPICDATA_CONNECTION_MODE.HTTP;
    public const string DEFAULT_LOCALHOST_ADDRESS_SERVICE_ZMQ = "localhost:8101",
        DEFAULT_LOCALHOST_ADDRESS_SERVICE_HTTP = "localhost:8102/services/binary",
        DEFAULT_LOCALHOST_ADDRESS_TOPICDATA_ZMQ = "localhost:8103",
        DEFAULT_LOCALHOST_ADDRESS_TOPICDATA_WS = "localhost:8104";


    public delegate void CbHandleTopicData(TopicData topicData);
    public delegate void CbTopicDataConnectionLost();
    private CbHandleTopicData CbOnTopicDataMessage = null;

    private SERVICE_CONNECTION_MODE serviceConnectionMode = SERVICE_CONNECTION_MODE.ZEROMQ;
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
        InitServiceClient();
        if (serviceClient == null) return null;

        serverSpecification = await RetrieveServerConfig();
        if (serverSpecification == null) return null;
        clientSpecification = await RegisterAsClient(clientSpecs);
        if (clientSpecification == null) return null;

        InitTopicDataClient();

        return clientSpecification;
    }

    private IUbiiServiceClient InitServiceClient()
    {
        string hostURL = serviceAddress;
        if (serviceConnectionMode == SERVICE_CONNECTION_MODE.ZEROMQ)
        {
            serviceClient = new UbiiServiceClientNetMQ(serviceAddress);
        }
        else if (serviceConnectionMode == SERVICE_CONNECTION_MODE.HTTP)
        {
            if (!hostURL.StartsWith("http://"))
            {
                hostURL = "http://" + hostURL;
            }
            serviceClient = new UbiiServiceClientHTTP(hostURL);
        }
        else if (serviceConnectionMode == SERVICE_CONNECTION_MODE.HTTPS)
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
        }

        return serviceClient;
    }

    private ITopicDataClient InitTopicDataClient()
    {
        if (this.topicDataConnectionMode == TOPICDATA_CONNECTION_MODE.ZEROMQ)
        {
            int port = int.Parse(serverSpecification.PortTopicDataZmq);
            this.topicDataClient = new UbiiTopicDataClientNetMQ(clientSpecification.Id, topicDataAddress, OnTopicDataMessage, OnTopicDataConnectionLost);
        }
        else if (this.topicDataConnectionMode == TOPICDATA_CONNECTION_MODE.HTTP)
        {
            string hostURL = topicDataAddress;
            if (!hostURL.StartsWith("ws://"))
            {
                hostURL = "ws://" + hostURL;
            }
            int port = int.Parse(serverSpecification.PortTopicDataWs);
            this.topicDataClient = new UbiiTopicDataClientWS(clientSpecification.Id, hostURL, OnTopicDataMessage, OnTopicDataConnectionLost);
        }
        else if (topicDataConnectionMode == TOPICDATA_CONNECTION_MODE.HTTPS)
        {
            string hostURL = topicDataAddress;
            if (!hostURL.StartsWith("wss://"))
            {
                hostURL = "wss://" + hostURL;
            }
            int port = int.Parse(serverSpecification.PortTopicDataWs);
            topicDataClient = new UbiiTopicDataClientWS(clientSpecification.Id, hostURL, OnTopicDataMessage, OnTopicDataConnectionLost);
        }

        if (topicDataClient == null)
        {
            Debug.LogError("UBII UbiiNetworkClient.InitTopicDataClient() - topic data client connection null");
        }

        return topicDataClient;
    }

    private async Task<Ubii.Servers.Server> RetrieveServerConfig()
    {
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

    public async Task<bool> ShutDown()
    {
        bool success = true;
        if (IsConnected() && clientSpecification != null)
        {
            ServiceReply reply = await CallService(new ServiceRequest
            {
                Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_DEREGISTRATION,
                Client = clientSpecification
            });
            if (reply.Error != null) {
                Debug.LogError(reply.Error);
                success = false;
            }
        }
        serviceClient?.TearDown();
        topicDataClient?.TearDown();

        return success;
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

    public async Task<bool> Send(TopicDataRecord record, CancellationToken ct)
    {
        return await topicDataClient?.Send(new TopicData
        {
            TopicDataRecord = record
        }, ct);
    }

    public async Task<bool> Send(TopicDataRecordList recordList, CancellationToken ct)
    {
        return await topicDataClient?.Send(new TopicData
        {
            TopicDataRecordList = recordList
        }, ct);
    }

    public async Task<bool> Send(TopicData topicData, CancellationToken ct)
    {
        return await topicDataClient?.Send(topicData, ct);
    }

    private void OnTopicDataMessage(TopicData topicData)
    {
        this.CbOnTopicDataMessage?.Invoke(topicData);
    }

    private void OnTopicDataConnectionLost()
    {
        Debug.Log("OnTopicDataConnectionLost");
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

    public void SetCbHandleTopicData(CbHandleTopicData callback)
    {
        CbOnTopicDataMessage = callback;
    }

    public async Task<bool> SubscribeTopic(string topic)
    {
        return await SubscribeTopics(new List<string>() { topic });
    }

    public async Task<bool> SubscribeTopics(List<string> topics)
    {
        if (topicDataClient == null) return false;
        if (CbOnTopicDataMessage == null)
        {
            Debug.LogError("UBII UbiiNetworkClient.SubscribeTopic() - callback is NULL!");
            return false;
        }

        // Repeated fields cannot be instantiated in SerivceRequest creation
        RepeatedField<string> subscribeTopics = new RepeatedField<string>();
        subscribeTopics.Add(topics);

        ServiceRequest topicSubscription = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription
            {
                ClientId = clientSpecification.Id,
                SubscribeTopics = { subscribeTopics }
            }
        };

        ServiceReply reply = await CallService(topicSubscription);
        if (reply.Error != null)
        {
            Debug.LogError("UBII UbiiNetworkClient.SubscribeTopic() - Server Error: " + reply.Error.ToString());
            return false;
        }

        return true;
    }

    public async Task<bool> UnsubscribeTopic(string topic)
    {
        return await UnsubscribeTopics(new List<string>() { topic });
    }

    private async Task<bool> UnsubscribeTopics(List<string> topics)
    {
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

        ServiceReply reply = await CallService(topicUnsubscription);
        if (reply.Error != null)
        {
            Debug.LogError("UBII UbiiNetworkClient.UnsubscribeTopic() - Server Error: " + reply.Error.ToString());
            return false;
        }

        return true;
    }

    #endregion

    #region Regex
    public async Task<bool> SubscribeRegex(string regex)
    {
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

        return true;
    }

    public async Task<bool> UnsubscribeRegex(string regex)
    {
        return await UnsubscribeRegexes(new List<string>() { regex });
    }

    private async Task<bool> UnsubscribeRegexes(List<string> regexes)
    {
        ServiceRequest unsubscribeRequest = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription { ClientId = clientSpecification.Id }
        };
        unsubscribeRequest.TopicSubscription.UnsubscribeTopicRegexp.Add(regexes);

        ServiceReply reply = await CallService(unsubscribeRequest);

        if (reply.Error != null)
        {
            Debug.LogError("UBII UbiiNetworkClient.UnsubscribeRegex() - Server Error: " + reply.Error.ToString());
            return false;
        }

        return true;
    }

    #endregion

    #endregion
}
