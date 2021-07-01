﻿using System;
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

/// <summary>
/// This class manages network connections based on NetMQ to the Ubi-Interact master node.
/// </summary>
public class NetMQUbiiClient
{
    private string host;
    private int port;

    private Client clientSpecification;

    private NetMQServiceClient netmqServiceClient;

    private NetMQTopicDataClient netmqTopicDataClient;

    private Server serverSpecification;

    public NetMQUbiiClient(string host, int port)
    {
        this.host = host;
        this.port = port;
    }

    public string GetClientID()
    {
        return clientSpecification?.Id;
    }

    public bool IsConnected()
    {
        return (clientSpecification != null && clientSpecification.Id != null && netmqTopicDataClient != null && netmqTopicDataClient.IsConnected());
    }

    public void SetPublishDelay(int millisecs)
    {
        netmqTopicDataClient.SetPublishDelay(millisecs);
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

    // CallService function called from upper layer (i.e. some MonoBehavior), returns a Task
    public Task<ServiceReply> CallService(ServiceRequest srq)
    {
        return Task.Run(() => netmqServiceClient.CallService(srq));
    }

    public void Publish(TopicDataRecord record)
    {
        netmqTopicDataClient.SendTopicData(record);
    }

    public void PublishImmediately(TopicDataRecord record)
    {
        netmqTopicDataClient.SendTopicDataImmediately(new Ubii.TopicData.TopicData {
            TopicDataRecord = record
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
            Debug.LogError("Deregister Device Error: " + reply.Error.Message);
        }
       
        return reply;
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

        // adding callback function to dictionary
        netmqTopicDataClient.AddTopicDataCallback(topic, callback);

        ServiceReply subReply = await CallService(topicSubscription);
        //Debug.Log("NetMQUbiiClient.SubscribeTopic() - " + topic + " - reply: " + subReply);
        if (subReply.Error != null)
        {
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            netmqTopicDataClient.RemoveTopicDataCallback(topic, callback);
            return false;
        }

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

    #endregion

    #region Initialize Functions

    // Initialize the ubiiClient, serviceClient and topicDataClient
    public async Task<Client> Initialize(Ubii.Clients.Client clientSpecs)
    {
        netmqServiceClient = new NetMQServiceClient(host, port);
        await InitServerSpec();
        //Debug.Log("ServerSpecs: " + serverSpecification);
        bool success = await InitClientRegistration(clientSpecs);
        InitTopicDataClient();

        return clientSpecification;
    }

    private async Task InitServerSpec()
    {
        // Call Service to receive serverSpecifications
        ServiceRequest serverConfigRequest = new ServiceRequest { Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SERVER_CONFIG };

        var task = CallService(serverConfigRequest);
        ServiceReply rep = await task;
        serverSpecification = rep.Server;
    }

    private async Task<bool> InitClientRegistration(Ubii.Clients.Client clientSpecs)
    {
        ServiceRequest clientRegistration = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_REGISTRATION,
            Client = clientSpecs
        };
        //if(isDedicatedProcessingNode)
        //  TODO:  clientRegistration.Client.ProcessingModules = ...

        ServiceReply reply = await CallService(clientRegistration);
        if (reply.Client != null)
        {
            clientSpecification = reply.Client;
            return true;
        }
        else if (reply.Error != null)
        {
            Debug.LogError("NetMQUbiiClient.InitClientRegistration() - " + reply);
        }
        
        return false;
    }

    private void InitTopicDataClient()
    {
        int port = int.Parse(serverSpecification.PortTopicDataZmq);
        netmqTopicDataClient = new NetMQTopicDataClient(clientSpecification.Id, host, port);
    }

    #endregion

    async public void ShutDown()
    {
        await CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_DEREGISTRATION,
            Client = clientSpecification
        });
        netmqServiceClient.TearDown();
        netmqTopicDataClient.TearDown();
    }
}
