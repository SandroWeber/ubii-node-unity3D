using System;
using System.Collections;
using System.Collections.Concurrent;
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
using Google.Protobuf.Collections;

public class NetMQTopicDataClient
{
    private string host;
    private int port;
    private string clientID;

    private DealerSocket socket;
    private bool connected = false;

    private Dictionary<string, List<Action<TopicDataRecord>>> topicdataCallbacks = new Dictionary<string, List<Action<TopicDataRecord>>>();

    private Dictionary<string, List<Action<TopicDataRecord>>> topicdataRegexCallbacks =
        new Dictionary<string, List<Action<TopicDataRecord>>>();

    private bool running = false;
    private Task processIncomingMessages = null;
    NetMQPoller poller;

    private ConcurrentBag<TopicDataRecord> recordsToPublish = new ConcurrentBag<TopicDataRecord>();

    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken cancellationToken;

    private int delay = 25; // millis

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
            //Debug.Log("Create Socket successful. Host: " + host + ":" + port);
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
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Cancelling task");
                    try
                    {
                        poller.Remove(socket);
                        socket.Close();
                        socket.Dispose();
                        poller.StopAsync();
                        poller.Stop();
                    }
                    catch (Exception e)
                    {
                        Debug.Log("ERR: " + e);
                    }

                    break;
                }

                FlushRecordsToPublish();
            }
        }, cancellationToken);
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
            Elements = {repeatedField},
        };

        TopicData td = new TopicData()
        {
            TopicDataRecordList = recordList
        };

        SendTopicDataImmediately(td);
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
        if (!this.topicdataCallbacks.ContainsKey(topic))
        {
            this.topicdataCallbacks.Add(topic, new List<Action<TopicDataRecord>>());
        }

        this.topicdataCallbacks[topic].Add(callback);
    }

    public void AddTopicDataRegexCallback(string regex, Action<TopicDataRecord> callback)
    {
        if (!this.topicdataRegexCallbacks.ContainsKey(regex))
        {
            this.topicdataRegexCallbacks.Add(regex, new List<Action<TopicDataRecord>>());
        }

        this.topicdataRegexCallbacks[regex].Add(callback);
    }

    public void RemoveTopicData(string topic)
    {
        this.topicdataCallbacks.Remove(topic);
    }

    public void RemoveTopicDataCallback(string topic, Action<TopicDataRecord> callback)
    {
        this.topicdataCallbacks[topic].Remove(callback);
    }

    public void RemoveTopicDataRegex(string regex)
    {
        this.topicdataRegexCallbacks.Remove(regex);
    }

    public void RemoveTopicDataRegexCallback(string regex, Action<TopicDataRecord> callback)
    {
        this.topicdataRegexCallbacks[regex].Remove(callback);
    }

    public List<string> GetAllSubscribedTopics()
    {
        return topicdataCallbacks.Keys.ToList();
    }

    public List<string> GetAllSubscribedRegex()
    {
        return topicdataRegexCallbacks.Keys.ToList();
    }

    public void SetPublishDelay(int millisecs)
    {
        delay = millisecs;
    }

    public void SendTopicData(TopicDataRecord record)
    {
        recordsToPublish.Add(record);
    }

    public void SendTopicDataImmediately(TopicData td)
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
        else if (topicData.TopicDataRecordList != null)
        {
            foreach (TopicDataRecord record in topicData.TopicDataRecordList.Elements)
            {
                this.InvokeTopicCallbacks(record);
            }
        }
        // catch possible error
        else if (topicData.Error != null)
        {
            Debug.LogError("topicData Error: " + topicData.Error.ToString());
            return;
        }
    }

    private void InvokeTopicCallbacks(TopicDataRecord record)
    {
        string topic = record.Topic;
        if (topicdataCallbacks.ContainsKey(topic))
        {
            foreach (Action<TopicDataRecord> callback in topicdataCallbacks[topic])
            {
                callback.Invoke(record);
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
                        callback.Invoke(record);
                    }
                }
            }
        }
    }

    public void TearDown()
    {
        Debug.Log("TearDown TopicDataClient");
        SetPublishDelay(1);
        topicdataCallbacks.Clear();
        topicdataRegexCallbacks.Clear();
        cts.Cancel();
        running = false;
        connected = false;
        NetMQConfig.Cleanup(false);
    }
}