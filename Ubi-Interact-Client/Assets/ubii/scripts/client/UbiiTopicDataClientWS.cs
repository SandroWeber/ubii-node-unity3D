using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

using Google.Protobuf;
using Google.Protobuf.Collections;
using Ubii.TopicData;

//TODO: both NetMQ and Websocket client share too much code, merge both and keep only base
// socket functionality separate
public class UbiiTopicDataClientWS : ITopicDataClient
{
    private string clientId;
    private string host;
    private int port;
    private ClientWebSocket clientWebsocket = null;

    private Dictionary<string, List<Action<TopicDataRecord>>> topicCallbacks = new Dictionary<string, List<Action<TopicDataRecord>>>();
    private Dictionary<string, List<Action<TopicDataRecord>>> topicRegexCallbacks =
        new Dictionary<string, List<Action<TopicDataRecord>>>();

    private bool connected = false;
    private Task taskProcessIncomingMsgs = null;
    //private Task taskSendOutgoingMessages = null;
    private CancellationToken cancelTokenTaskProcessIncomingMsgs;
    private ConcurrentBag<TopicDataRecord> recordsToPublish = new ConcurrentBag<TopicDataRecord>();

    private int publishInterval = 25; // milliseconds

    public UbiiTopicDataClientWS(string clientId = null, string host = "localhost", int port = 8104)
    {
        this.clientId = clientId;
        this.host = host;
        this.port = port;

        Initialize();
    }

    private async void Initialize()
    {
        clientWebsocket = new ClientWebSocket();

        Uri url = new Uri("ws://" + this.host + ":" + this.port + "?clientID=" + this.clientId);
        Debug.Log("UbiiTopicDataClientWS connecting to " + url);
        CancellationToken cancelTokenConnect = new CancellationToken();
        await clientWebsocket.ConnectAsync(url, cancelTokenConnect);

        Debug.Log("websocket after ConnectAsync()");
        Debug.Log("clientWebsocket.State = " + clientWebsocket.State);

        connected = true;

        //TODO: refactor
        cancelTokenTaskProcessIncomingMsgs = new CancellationToken();
        taskProcessIncomingMsgs = Task.Run(async () =>
        {
            while (connected)
            {
                try
                {
                    var bytebuffer = new byte[1024];
                    ArraySegment<byte> arraySegment = new ArraySegment<byte>(bytebuffer);
                    WebSocketReceiveResult receiveResult = await clientWebsocket.ReceiveAsync(arraySegment, cancelTokenTaskProcessIncomingMsgs);
                    Debug.Log("ws receiveResult.EndOfMessage = " + receiveResult.EndOfMessage);
                    if (receiveResult.EndOfMessage)
                    {
                        Debug.Log("ws receiveResult.Count = " + receiveResult.Count);
                        if (receiveResult.Count == 4)
                        {
                            Debug.Log("PING probably");
                            string msgString = Encoding.UTF8.GetString(arraySegment.AsMemory().ToArray(), 0, receiveResult.Count);
                            if (msgString == "PING")
                            {
                                Debug.Log("PING received, sending PONG");
                                // PING message
                                await this.SendBytes(Encoding.UTF8.GetBytes("PONG"));
                            }
                        }
                        else
                        {
                            TopicData topicdata = TopicData.Parser.ParseFrom(arraySegment.Array, 0, receiveResult.Count);
                            Debug.Log(topicdata);

                            if (topicdata.TopicDataRecord != null)
                            {
                                this.InvokeTopicCallbacks(topicdata.TopicDataRecord);
                            }

                            if (topicdata.Error != null)
                            {
                                Debug.LogError(topicdata.Error.ToString());
                            }
                        }
                    }

                    FlushRecordsToPublish();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
            }
        });
    }

    public async void TearDown()
    {
        connected = false;
        if (clientWebsocket != null)
        {
            CancellationToken cancellationToken = new CancellationToken();
            await clientWebsocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "de-initializing unity websocket client", cancellationToken);
            clientWebsocket.Dispose();
        }
    }

    public bool IsConnected()
    {
        return this.connected;
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

    public void SendTopicDataRecord(TopicDataRecord record)
    {
        Debug.Log("WS SendTopicDataRecord() record = " + record);
        recordsToPublish.Add(record);
    }

    public void SendTopicDataImmediately(TopicData topicData)
    {
        Debug.Log("WS SendTopicDataImmediately() topicData = " + topicData);
        Send(topicData);
    }

    public async Task<CancellationToken> SendBytes(byte[] bytes)
    {
        var arraySegment = new ArraySegment<Byte>(bytes);
        Debug.Log("arraySegment.Count: " + arraySegment.Count);
        CancellationToken cancellationToken = new CancellationToken();
        await clientWebsocket.SendAsync(arraySegment, WebSocketMessageType.Binary, true, cancellationToken);
        return cancellationToken;
    }

    public async void Send(TopicData topicdata)
    {
        Debug.Log("WS Send() topicdata = " + topicdata);

        MemoryStream memoryStream = new MemoryStream();
        CodedOutputStream codedOutputStream = new CodedOutputStream(memoryStream);
        topicdata.WriteTo(codedOutputStream);
        codedOutputStream.Flush();
        Debug.Log("codedOutputStream: " + codedOutputStream);
        var bytebuffer = memoryStream.ToArray();
        Debug.Log("buffer length: " + bytebuffer.Length);

        /*var array_segment = new ArraySegment<Byte>(bytebuffer);
        Debug.Log("array_segment length: " + array_segment.Count);
        CancellationToken cancellation_token = new CancellationToken();
        await clientWebsocket.SendAsync(array_segment, WebSocketMessageType.Binary, true, cancellation_token);*/
        await this.SendBytes(bytebuffer);


        /*try
        {
            await clientWebsocket.SendAsync(array_segment, WebSocketMessageType.Binary, true, cancellation_token);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }*/
    }

    #region private-methods

    private void FlushRecordsToPublish()
    {
        Debug.Log("FlushRecordsToPublish");
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

    #endregion private-methods
}
