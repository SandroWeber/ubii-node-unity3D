﻿using System;
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

    private string host;
    private int portServiceZMQ;
    private int portServiceREST;

    private Client clientSpecification;

    private IUbiiServiceClient serviceClient;

    private ITopicDataClient topicDataClient;

    private Server serverSpecification;

    public UbiiNetworkClient(string host, int portServiceZMQ, int portServiceREST, SERVICE_CONNECTION_MODE serviceConnectionMode, TOPICDATA_CONNECTION_MODE topicDataConnectionMode)
    {
        this.host = host;
        this.portServiceZMQ = portServiceZMQ;
        this.portServiceREST = portServiceREST;
        this.serviceConnectionMode = serviceConnectionMode;
        this.topicDataConnectionMode = topicDataConnectionMode;
    }

    #region Initialize/Teardown Functions

    // Initialize the ubiiClient, serviceClient and topicDataClient
    public async Task<Client> Initialize(Ubii.Clients.Client clientSpecs)
    {
        if (this.serviceConnectionMode == SERVICE_CONNECTION_MODE.ZEROMQ)
        {
            serviceClient = new NetMQServiceClient(host, portServiceZMQ);
        }
        else if (this.serviceConnectionMode == SERVICE_CONNECTION_MODE.HTTP)
        {
            string hostURL = host;
            if (!hostURL.StartsWith("http://"))
            {
                hostURL = "http://" + hostURL;
            }
            serviceClient = new UbiiServiceClientREST(hostURL, portServiceREST);
        }
        else if (this.serviceConnectionMode == SERVICE_CONNECTION_MODE.HTTPS)
        {
            string hostURL = host;
            if (!hostURL.StartsWith("https://"))
            {
                hostURL = "https://" + hostURL;
            }
            serviceClient = new UbiiServiceClientREST(hostURL, portServiceREST);
        }

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

        ServiceReply reply = await CallService(serverConfigRequest);
        if (reply.Server != null)
        {
            serverSpecification = reply.Server;
        }
        else if (reply.Error != null)
        {
            Debug.LogError(reply.Error.ToString());
        }
        else
        {
            Debug.LogError("UbiiNetworkClient - unkown server response during server specification retrieval");
        }
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
            Debug.LogError("UbiiNetworkClient.InitClientRegistration() - " + reply);
        }

        return false;
    }

    private void InitTopicDataClient()
    {
        if (this.topicDataConnectionMode == TOPICDATA_CONNECTION_MODE.ZEROMQ)
        {
            int port = int.Parse(serverSpecification.PortTopicDataZmq);
            this.topicDataClient = new NetMQTopicDataClient(clientSpecification.Id, host, port);
        }
        else if (this.topicDataConnectionMode == TOPICDATA_CONNECTION_MODE.HTTP)
        {
            string hostURL = host;
            if (!hostURL.StartsWith("ws://"))
            {
                hostURL = "ws://" + hostURL;
            }
            int port = int.Parse(serverSpecification.PortTopicDataWs);
            this.topicDataClient = new UbiiTopicDataClientWS(clientSpecification.Id, hostURL, port);
        }
        else if (this.topicDataConnectionMode == TOPICDATA_CONNECTION_MODE.HTTPS)
        {
            string hostURL = host;
            if (!hostURL.StartsWith("wss://"))
            {
                hostURL = "wss://" + hostURL;
            }
            int port = int.Parse(serverSpecification.PortTopicDataWs);
            this.topicDataClient = new UbiiTopicDataClientWS(clientSpecification.Id, hostURL, port);
        }
    }

    async public void ShutDown()
    {
        await CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_DEREGISTRATION,
            Client = clientSpecification
        });
        serviceClient.TearDown();
        topicDataClient.TearDown();
    }

    #endregion

    public string GetClientID()
    {
        return clientSpecification?.Id;
    }

    public bool IsConnected()
    {
        return (clientSpecification != null && clientSpecification.Id != null && topicDataClient != null && topicDataClient.IsConnected());
    }

    public void SetPublishDelay(int millisecs)
    {
        topicDataClient.SetPublishDelay(millisecs);
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
        topicDataClient.SendTopicDataRecord(record);
    }

    public void PublishImmediately(TopicDataRecord record)
    {
        topicDataClient.SendTopicDataImmediately(new Ubii.TopicData.TopicData
        {
            TopicDataRecord = record
        });
    }

    public void PublishImmediately(TopicDataRecordList recordList)
    {
        topicDataClient.SendTopicDataImmediately(new Ubii.TopicData.TopicData
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
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            topicDataClient.RemoveTopicCallback(topic, callback);
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
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
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
            Debug.LogError("SubscribeRegex() - callback is NULL!");
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
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
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
            Debug.LogError("UnsubscribeRegex() - callback is NULL!");
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
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            return false;
        }
        // remove regex from topicdataclient
        topicDataClient.RemoveAllTopicRegexCallbacks(regex);
        return true;
    }

    #endregion

    #endregion
}
