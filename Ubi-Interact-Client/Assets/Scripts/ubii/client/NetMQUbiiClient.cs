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

public class NetMQUbiiClient
{
    private string id;
    private string name;
    private string host;
    private int port;

    private Client clientSpecification;

    private NetMQServiceClient netmqServiceClient;

    private NetMQTopicDataClient netmqTopicDataClient;

    private List<Device> deviceSpecifications = new List<Device>();

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
        //await InitTestDe<vices(); // Initialize some test devices for demonstration purposes, thus commented out
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
        ServiceRequest topicSubscription = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription
            {
                ClientId = clientSpecification.Id,
                SubscribeTopicRegexp = regex
            }
        };

        ServiceReply subReply = await CallService(topicSubscription);

        if (subReply.Error != null)
        {
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            return false;
        }

        // adding callback function to dictionary
        netmqTopicDataClient.AddTopicDataRegexCallback(regex, callback);

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
            return await UnsubscribeTopic(topic);
        }

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
            //TODO: server side implementation
            return await UnsubscribeRegex(regex);
        }
        return true;
    }

    /// <summary>
    /// Unsubscribe from all topics/regex, called before shutdown
    /// </summary>
    /// <returns></returns>
    private async Task UnsubscribeAll()
    {
        List<string> subscribedTopics = netmqTopicDataClient.GetAllSubscribedTopics();
        List<string> subscribedRegex = netmqTopicDataClient.GetAllSubscribedRegex();

        foreach (string topic in subscribedTopics)
        {
            await UnsubscribeTopic(topic);
        }

        foreach (string regex in subscribedRegex)
        {
            await UnsubscribeRegex(regex);
        }


    }

    private async Task<bool> UnsubscribeTopic(string topic)
    {
        // Repeated fields cannot be instantiated in SerivceRequest creation; adding topic to unsubscribeTopics which is later sent to server with clientID
        RepeatedField<string> unsubscribeTopics = new RepeatedField<string>();
        unsubscribeTopics.Add(topic);

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
        return true;
    }

    private async Task<bool> UnsubscribeRegex(string regex)
    {
        //TODO: server side implementation
        return false;
    }


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

    #region Test Functions
    // This is only for testing / demonstration purposes on how a device registration works with ubiiClient
    private async Task InitTestDevices()
    {
        // RepeatedFields have to be declared separately and then added to the service request
        RepeatedField<Ubii.Devices.Component> components = new RepeatedField<Ubii.Devices.Component>();
        components.Add(new Ubii.Devices.Component { Topic = "TestBool", MessageFormat = "boolean", IoType = Ubii.Devices.Component.Types.IOType.Publisher });

        // Device Registration
        ServiceRequest deviceRegistration = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.DEVICE_REGISTRATION,
            Device = new Device
            {
                Name = "TestDevice1",
                DeviceType = Device.Types.DeviceType.Participant,
                ClientId = clientSpecification.Id,
                Components = { components }
            }
        };

        Debug.Log("DeviceRegistration #1 Components: " + deviceRegistration.ToString());

        var task = CallService(deviceRegistration);
        ServiceReply svr = await task;

        if (svr.Device != null)
        {
            deviceSpecifications.Add(svr.Device);
            Debug.Log("DeviceSpecifications #1: " + deviceSpecifications[0].ToString());
            Debug.Log("DeviceSpecifications #1 Components: " + deviceSpecifications[0].Components.ToString());
        }
        else if (svr.Error != null)
            Debug.Log("SVR Error: " + svr.Error.ToString());
        else
            Debug.Log("An unknown error occured");
    }


    #endregion

    async public void ShutDown()
    {
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
