using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ubii.TopicData;
using Ubii.Clients;
using Ubii.Devices;
using Ubii.Services;
using Ubii.Servers;
using Ubii.Services.Request;
using Google.Protobuf.Collections;
using UnityEngine;
using NetMQ;
using Google.Protobuf;
using System.Threading;
using UnityEngine.Apple.ReplayKit;

public class NetMQUbiiClient
{
    private string id;
    private string name;
    private string host;
    private int port;

    private Client clientSpecification;

    private NetMQServiceClient netmqServiceClient;

    private NetMQTopicDataClient netmqTopicDataClient;

    private Dictionary<string, Device> registeredDevices = new Dictionary<string, Device>();

    private Server serverSpecification;

    public NetMQUbiiClient(string id, string name, string host, int port)
    {
        this.id = id;
        this.name = name;
        this.host = host;
        this.port = port;
    }

    public string GetClientID()
    {
        return clientSpecification.Id;
    }

    // Initialize the ubiiClient, serviceClient and topicDataClient
    public async Task Initialize()
    {
        netmqServiceClient = new NetMQServiceClient(host, port);
        await InitServerSpec();
        Debug.Log("ServerSpecs: " + serverSpecification);
        await InitClientReg();
        InitTopicDataClient();
    }

    public bool IsConnected()
    {
        /*Debug.Log("IsConnected()");
        Debug.Log("id=" + id);
        Debug.Log("client=" + netmqTopicDataClient);
        if (netmqTopicDataClient != null)
        {
            Debug.Log("connected=" + netmqTopicDataClient.IsConnected());
        }*/
        return (clientSpecification != null && clientSpecification.Id != null && netmqTopicDataClient != null && netmqTopicDataClient.IsConnected());
    }

    /*public Task WaitForConnection()
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
    }*/

    // CallService function called from upper layer (i.e. some MonoBehavior), returns a Task
    public Task<ServiceReply> CallService(ServiceRequest srq)
    {
        //Debug.Log("CallService: " + srq.Topic);
        return Task.Run(() => netmqServiceClient.CallService(srq));
    }

    public void Publish(TopicData topicData)
    {
        netmqTopicDataClient.SendTopicData(topicData);
    }

    #region Devices
    public async Task<ServiceReply> RegisterDevice(Device ubiiDevice)
    {
        ServiceReply reply = await CallService(new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.DEVICE_REGISTRATION,
            Device = ubiiDevice
        });

        if (reply.Error == null)
        {
            registeredDevices.Add(reply.Device.Id, reply.Device);
        }

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
            Debug.LogError("Deregister Device Error: " + reply.Error.Message);
        }
        if (!registeredDevices.Remove(ubiiDevice.Id))
        {
            Debug.LogError("Device " + ubiiDevice.Name + " could not be removed from local list.");
        }
        Debug.Log("Deregistering " + ubiiDevice + " successful!");
        return reply;
    }

    private async Task DeregisterAllDevices()
    {
        foreach (Device device in registeredDevices.Values.ToList())
        {
            await DeregisterDevice(device);
        }
    }
    #endregion

    #region Subscriptions

    #region Topics
    public async Task<bool> SubscribeTopic(string topic, Action<TopicDataRecord> callback)
    {
        if (callback == null)
        {
            Debug.LogError("SubscribeTopic() - callback is NULL!");
            return false;
        }

        if (this.netmqTopicDataClient.IsSubscribed(topic))
        {
            netmqTopicDataClient.AddTopicDataCallback(topic, callback);
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

        var task = CallService(topicSubscription);
        ServiceReply subReply = await task;

        if (subReply.Error != null)
        {
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            return false;
        }

        // adding callback function to dictionary
        netmqTopicDataClient.AddTopicDataCallback(topic, callback);
        return true;
    }

    public async Task<bool> UnsubscribeTopic(string topic, Action<TopicDataRecord> callback)
    {
        if (callback == null)
        {
            Debug.LogError("UnsubscribeTopic() - callback is NULL!");
            return false;
        }

        // removing callback function for this topic from dictionary
        netmqTopicDataClient.RemoveTopicDataCallback(topic, callback);

        // check if there are any callbacks left for this topic, if not, unsubscribe from topic
        if (!this.netmqTopicDataClient.HasTopicCallbacks(topic))
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

        var task = CallService(topicUnsubscription);
        ServiceReply subReply = await task;

        if (subReply.Error != null)
        {
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            return false;
        }

        foreach (string topic in topics)
        {
            netmqTopicDataClient.RemoveTopicData(topic);
        }
        return true;
    }
    #endregion

    #region Regex
    public async Task<bool> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        if (callback == null)
        {
            Debug.LogError("SubscribeRegex() - callback is NULL!");
            return false;
        }

        if (this.netmqTopicDataClient.IsSubscribed(regex))
        {
            netmqTopicDataClient.AddTopicDataRegexCallback(regex, callback);
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
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            return false;
        }

        // adding callback function to dictionary
        netmqTopicDataClient.AddTopicDataRegexCallback(regex, callback);

        return true;
    }

    public async Task<bool> UnsubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        if (callback == null)
        {
            Debug.LogError("UnsubscribeRegex() - callback is NULL!");
            return false;
        }

        // removing callback function from dictionary
        netmqTopicDataClient.RemoveTopicDataRegexCallback(regex, callback);

        if (!this.netmqTopicDataClient.HasTopicRegexCallbacks(regex))
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
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            return false;
        }
        // remove regex from topicdataclient
        netmqTopicDataClient.RemoveTopicDataRegex(regex);
        return true;
    }
    #endregion
    /// <summary>
    /// Unsubscribe from all topics/regex, called before shutdown
    /// </summary>
    /// <returns></returns>
    private async Task UnsubscribeAll()
    {
        await UnsubscribeTopic(netmqTopicDataClient.GetAllSubscribedTopics());

        List<string> subscribedRegex = netmqTopicDataClient.GetAllSubscribedRegex();
        foreach (string regex in subscribedRegex.ToList())
        {
            await UnsubscribeRegex(regex);
        }
    }
    #endregion

    #region Initialize Functions
    private async Task InitServerSpec()
    {
        // Call Service to receive serverSpecifications
        ServiceRequest serverConfigRequest = new ServiceRequest { Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SERVER_CONFIG };

        var task = CallService(serverConfigRequest);
        ServiceReply rep = await task;
        serverSpecification = rep.Server;
    }

    private async Task InitClientReg()
    {
        // Client Registration
        ServiceRequest clientRegistration = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_REGISTRATION,
            Client = new Client { Name = name, },
        };

        if (id != null && id != "")
            clientRegistration.Client.Id = id;

        var task = CallService(clientRegistration);
        ServiceReply rep = await task;
        clientSpecification = rep.Client;
        Debug.Log("ClientSpec: " + clientSpecification);
    }

    private void InitTopicDataClient()
    {
        netmqTopicDataClient = new NetMQTopicDataClient(clientSpecification.Id, host, int.Parse(serverSpecification.PortTopicDataZmq));
    }

    #endregion

    async public void ShutDown()
    {
        await DeregisterAllDevices();
        await UnsubscribeAll();
        await CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_DEREGISTRATION,
            Client = clientSpecification
        });
        netmqServiceClient.TearDown();
        netmqTopicDataClient.TearDown();
    }
}
