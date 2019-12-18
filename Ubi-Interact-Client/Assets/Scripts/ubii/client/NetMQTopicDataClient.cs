using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ubii.Services;
using Ubii.TopicData;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using System.Threading;
using UnityEngine;
using System.Text.RegularExpressions;

public class NetMQTopicDataClient
{
    private string host;
    private int port;
    private string clientID;

    private DealerSocket socket;
    private bool connected = false;

    private Dictionary<string, List<Action<TopicDataRecord>>> topicdataCallbacks = null;
    private Dictionary<string, List<Action<TopicDataRecord>>> topicdataRegexCallbacks = null;
    private bool running = false;
    private Task processIncomingMessages = null;
    NetMQPoller poller;



    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken cancellationToken;


    const int delay = 3000; // millis

    public NetMQTopicDataClient(string clientID, string host = "localhost", int port = 8103)
    {
        this.host = host;
        this.port = port;
        this.clientID = clientID; //global variable not neccesarily needed; only for socker.Options.Identity

        topicdataCallbacks = new Dictionary<string, List<Action<TopicDataRecord>>>();
        topicdataRegexCallbacks = new Dictionary<string, List<Action<TopicDataRecord>>>();

        Initialize();
    }

    private void StartSocket()
    {
        try
        {
            socket.Connect("tcp://" + host + ":" + port);
            Debug.Log("Create Socket successful. Host: " + host + ":" + port);
            connected = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("NetMQTopicDataClient, StartSocket(), Exception occured: " + ex.ToString());
        }
    }

    private void Initialize()
    {
        running = true;
        cancellationToken = cts.Token;

        processIncomingMessages = Task.Factory.StartNew(() =>
        {
            // instantiate poller and socket
            poller = new NetMQPoller();
            socket = new DealerSocket();
            socket.Options.Identity = Encoding.UTF8.GetBytes(clientID); // socket needs clientID for communication Dealer-Router
            socket.ReceiveReady += OnMessage; // event on receive data

            StartSocket();

            poller.Add(socket);
            poller.RunAsync();

            while (running)
            {
                Thread.Sleep(delay);
                if(cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Cancelling task");
                    break;
                }
            }
        }, cancellationToken);
    }

    public bool IsConnected()
    {
        return connected;
    }

    public bool IsSubscribed(string topicOrRegex)
    {
        return this.topicdataCallbacks.ContainsKey(topicOrRegex) || this.topicdataRegexCallbacks.ContainsKey(topicOrRegex);
    }

    public bool HasTopicCallbacks(string topic)
    {
        if (!this.IsSubscribed(topic))
        {
            return false;
        }

        return this.topicdataCallbacks[topic].Count > 0;
    }

    public bool HasTopicRegexCallbacks(string regex)
    {
        if (!this.IsSubscribed(regex))
        {
            return false;
        }

        return this.topicdataRegexCallbacks[regex].Count > 0;
    }

    public void AddTopicDataCallback(string topic, Action<TopicDataRecord> callback)
    {
        if (!this.topicdataCallbacks.ContainsKey(topic)) {
            this.topicdataCallbacks.Add(topic, new List<Action<TopicDataRecord>>());
        }
        this.topicdataCallbacks[topic].Add(callback);
    }

    public void AddTopicDataRegexCallback(string regex, Action<TopicDataRecord> callback)
    {
        if (!this.topicdataCallbacks.ContainsKey(regex)) {
            this.topicdataCallbacks.Add(regex, new List<Action<TopicDataRecord>>());
        }
        this.topicdataRegexCallbacks[regex].Add(callback);
    }

    public void RemoveTopicDataCallback(string topic, Action<TopicDataRecord> callback)
    {
        //Debug.Log("removing topicDataCallBack for topic: " + topic + " (backend)");
        this.topicdataCallbacks[topic].Remove(callback);
    }

    public void RemoveTopicDataRegexCallback(string regex, Action<TopicDataRecord> callback)
    {
        //Debug.Log("removing topicDataRegexCallBack for regex: " + regex + " (backend)");
        this.topicdataCallbacks[regex].Remove(callback);
    }

    public void SendTopicData(TopicData td)
    {
        byte[] buffer = td.ToByteArray();
        socket.SendFrame(buffer);
    }

    // Called when data received
    void OnMessage(object sender, NetMQSocketEventArgs e)
    {
        e.Socket.ReceiveFrameBytes(out bool hasmore);
        TopicData topicData = new TopicData { };
        if (hasmore)
        {
            topicData.MergeFrom(e.Socket.ReceiveFrameBytes(out hasmore));
        }

        // Invoke callback 
        if (topicData.TopicDataRecord != null)
        {
            string topic = topicData.TopicDataRecord.Topic;
            if (topicdataCallbacks.ContainsKey(topic))
            {
                foreach (Action<TopicDataRecord> callback in topicdataCallbacks[topic])
                {
                    callback.Invoke(topicData.TopicDataRecord);
                }
            }
            else
            {
                foreach (KeyValuePair<string, List<Action<TopicDataRecord>>> entry in topicdataRegexCallbacks)
                {
                    Match m = Regex.Match(topic, entry.Key);
                    if (m.Success)
                    {
                        foreach (Action<TopicDataRecord> callback in entry.Value)
                        {
                            callback.Invoke(topicData.TopicDataRecord);
                        }
                    }
                }
            }
        }
        // catch possible error
        else if (topicData.Error != null)
        {
            Debug.LogError("topicData Error: " + topicData.Error.ToString());
            return;
        }
    }

    public void TearDown()
    {
        Debug.Log("TearDown TopicDataClient");
        cts.Cancel();
        running = false;
        connected = false;
        NetMQConfig.Cleanup(false);
        if (poller.IsRunning)
        {
            poller.StopAsync();
            poller.Stop();
        }
    }

}
