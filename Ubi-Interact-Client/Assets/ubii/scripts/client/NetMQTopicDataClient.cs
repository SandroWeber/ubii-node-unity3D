using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using NetMQ;
using NetMQ.Sockets;

using Google.Protobuf;
using Google.Protobuf.Collections;
using Ubii.TopicData;

//TODO: both NetMQ and Websocket client share too much code, merge both and keep only base
// socket functionality separate
public class NetMQTopicDataClient : ITopicDataClient
{
    static int TIMEOUT_SECONDS_SEND = 3;

    private string address;
    private string clientID;

    private DealerSocket socket;
    private bool connected = false;

    //TODO: to be removed and replaced with TopicDataBuffer
    private Dictionary<string, List<Action<TopicDataRecord>>> topicCallbacks = new Dictionary<string, List<Action<TopicDataRecord>>>();
    private Dictionary<string, List<Action<TopicDataRecord>>> topicRegexCallbacks =
        new Dictionary<string, List<Action<TopicDataRecord>>>();

    private ITopicDataBuffer topicDataBuffer = null;

    private bool running = false;
    private Task processIncomingMessages = null;
    NetMQPoller poller;

    private ConcurrentBag<TopicDataRecord> recordsToPublish = new ConcurrentBag<TopicDataRecord>();

    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken cancellationToken;

    private int publishInterval = 25; // milliseconds

    public NetMQTopicDataClient(string clientID, string address = "localhost:8103")
    {
        this.address = address;
        this.clientID = clientID; //global variable not neccesarily needed; only for socker.Options.Identity

        topicCallbacks = new Dictionary<string, List<Action<TopicDataRecord>>>();
        topicRegexCallbacks = new Dictionary<string, List<Action<TopicDataRecord>>>();

        Initialize();
    }

    private void StartSocket()
    {
        try
        {
            socket.Connect("tcp://" + this.address);
            //Debug.Log("Create Socket successful. Host: " + host + ":" + port);
            connected = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("UBII NetMQTopicDataClient.StartSocket(): " + ex.ToString());
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
                Thread.Sleep(publishInterval);
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Cancelling task");
                    break;
                }
                FlushRecordsToPublish();
            }
        }, cancellationToken);
    }

    public void TearDown()
    {
        Debug.Log("TearDown NetMQ TopicDataClient");
        SetPublishDelay(1);
        topicCallbacks.Clear();
        topicRegexCallbacks.Clear();
        cts.Cancel();
        running = false;
        connected = false;

        NetMQConfig.Cleanup(false);

        try
        {
            if (poller.IsRunning)
            {
                poller.StopAsync();
                poller.Stop();
            }
        }
        catch (Exception)
        {
        }
    }

    private void FlushRecordsToPublish()
    {
        if (recordsToPublish.IsEmpty) return;

        RepeatedField<TopicDataRecord> repeatedField = new RepeatedField<TopicDataRecord>();
        while (!recordsToPublish.IsEmpty)
        {
            if (recordsToPublish.TryTake(out TopicDataRecord record))
            {
                repeatedField.Add(record);
            }
        }

        TopicDataRecordList recordList = new TopicDataRecordList()
        {
            Elements = { repeatedField },
        };

        TopicData topicData = new TopicData()
        {
            TopicDataRecordList = recordList
        };

        SendTopicDataImmediately(topicData);
    }

    public bool IsConnected()
    {
        return connected;
    }

    public bool IsSubscribed(string topicOrRegex)
    {
        return this.topicCallbacks.ContainsKey(topicOrRegex) || this.topicRegexCallbacks.ContainsKey(topicOrRegex);
    }

    public bool HasTopicCallbacks(string topic)
    {
        if (!this.IsSubscribed(topic))
        {
            return false;
        }

        return this.topicCallbacks[topic].Count > 0;
    }

    public bool HasTopicRegexCallbacks(string regex)
    {
        if (!this.IsSubscribed(regex))
        {
            return false;
        }

        return this.topicRegexCallbacks[regex].Count > 0;
    }

    public void AddTopicCallback(string topic, Action<TopicDataRecord> callback)
    {
        if (!this.topicCallbacks.ContainsKey(topic))
        {
            this.topicCallbacks.Add(topic, new List<Action<TopicDataRecord>>());
        }

        this.topicCallbacks[topic].Add(callback);
    }

    public void AddTopicRegexCallback(string regex, Action<TopicDataRecord> callback)
    {
        if (!this.topicRegexCallbacks.ContainsKey(regex))
        {
            this.topicRegexCallbacks.Add(regex, new List<Action<TopicDataRecord>>());
        }

        this.topicRegexCallbacks[regex].Add(callback);
    }

    public void RemoveAllTopicCallbacks(string topic)
    {
        this.topicCallbacks.Remove(topic);
    }

    public void RemoveTopicCallback(string topic, Action<TopicDataRecord> callback)
    {
        this.topicCallbacks[topic].Remove(callback);
    }

    public void RemoveAllTopicRegexCallbacks(string regex)
    {
        this.topicRegexCallbacks.Remove(regex);
    }

    public void RemoveTopicRegexCallback(string regex, Action<TopicDataRecord> callback)
    {
        this.topicRegexCallbacks[regex].Remove(callback);
    }

    public List<string> GetAllSubscribedTopics()
    {
        return topicCallbacks.Keys.ToList();
    }

    public List<string> GetAllSubscribedRegex()
    {
        return topicRegexCallbacks.Keys.ToList();
    }

    public void SetPublishDelay(int millisecs)
    {
        publishInterval = millisecs;
    }

    public void SendTopicDataRecord(TopicDataRecord record)
    {
        recordsToPublish.Add(record);
    }

    public Task<CancellationToken> SendTopicDataImmediately(TopicData topicData)
    {
        try
        {
            byte[] buffer = topicData.ToByteArray();
            bool success = socket.TrySendFrame(TimeSpan.FromSeconds(TIMEOUT_SECONDS_SEND), buffer);
            if (!success) {
                Debug.LogError("UBII - TopicData could not be sent: " + topicData.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return Task.FromResult(new CancellationToken(false));
    }

    // Called when data received
    void OnMessage(object sender, NetMQSocketEventArgs e)
    {
        e.Socket.ReceiveFrameBytes(out bool hasmore);
        TopicData topicData = new TopicData { };
        if (hasmore)
        {
            byte[] bytes = e.Socket.ReceiveFrameBytes(out hasmore);

            if (bytes.Length == 4)
            {
                string msgString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                if (msgString == "PING")
                {
                    // PING message
                    socket.SendFrame(Encoding.UTF8.GetBytes("PONG"));
                    return;
                }
            }
            else
            {
                topicData.MergeFrom(bytes);
            }
        }

        // single record
        if (topicData.TopicDataRecord != null)
        {
            this.InvokeTopicCallbacks(topicData.TopicDataRecord);
        }
        // list of records
        if (topicData.TopicDataRecordList != null)
        {
            foreach (TopicDataRecord record in topicData.TopicDataRecordList.Elements)
            {
                this.InvokeTopicCallbacks(record);
            }
        }
        // catch possible error
        if (topicData.Error != null)
        {
            Debug.LogError("UBII NetMQTopicDataClient.OnMessage(): " + topicData.Error.ToString());
            return;
        }
    }

    private void InvokeTopicCallbacks(TopicDataRecord record)
    {
        string topic = record.Topic;
        if (topicCallbacks.ContainsKey(topic))
        {
            foreach (Action<TopicDataRecord> callback in topicCallbacks[topic])
            {
                callback.Invoke(record);
            }
        }
        else
        {
            foreach (KeyValuePair<string, List<Action<TopicDataRecord>>> entry in topicRegexCallbacks)
            {
                Match m = Regex.Match(topic, entry.Key);
                if (m.Success)
                {
                    foreach (Action<TopicDataRecord> callback in entry.Value)
                    {
                        callback.Invoke(record);
                    }
                }
            }
        }
    }
}