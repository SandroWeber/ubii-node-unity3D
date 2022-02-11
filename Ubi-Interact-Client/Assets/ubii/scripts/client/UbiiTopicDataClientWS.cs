﻿using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private static int RECEIVE_BUFFER_SIZE = 5120;

    private string clientId;
    private string host;
    private int port;

    

#if WINDOWS_UWP
    private Windows.Networking.Sockets.MessageWebSocket clientWebsocket = null;
#else
    private System.Net.WebSockets.ClientWebSocket clientWebsocket = null;
#endif

    private Dictionary<string, List<Action<TopicDataRecord>>> topicCallbacks = new Dictionary<string, List<Action<TopicDataRecord>>>();
    private Dictionary<string, List<Action<TopicDataRecord>>> topicRegexCallbacks =
        new Dictionary<string, List<Action<TopicDataRecord>>>();

    private bool connected = false;
    private Task taskProcessIncomingMsgs = null, taskFlushOutgoingMsgs = null;
    //TODO: merge cancel tokens?
    private CancellationToken cancelTokenReadSocket, cancelTokenWriteSocket;
    private ConcurrentBag<TopicDataRecord> recordsToPublish = new ConcurrentBag<TopicDataRecord>();
    private MemoryStream msReadBuffer = new MemoryStream();

    private int publishInterval = 25; // milliseconds

    public UbiiTopicDataClientWS(string clientId = null, string host = "https://localhost", int port = 8104)
    {
        this.clientId = clientId;
        this.host = host;
        this.port = port;

        Initialize();
    }

    private async void Initialize()
    {
        Uri uri = new Uri(this.host + ":" + this.port + "?clientID=" + this.clientId);
        Debug.LogError("WS Initialize() url=" + uri);
        
        try
        {
#if WINDOWS_UWP
            clientWebsocket = new Windows.Networking.Sockets.MessageWebSocket();
            clientWebsocket.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Binary;
            await clientWebsocket.ConnectAsync(uri);
#else
            clientWebsocket = new System.Net.WebSockets.ClientWebSocket();
            CancellationToken cancelTokenConnect = new CancellationToken();
            await clientWebsocket.ConnectAsync(uri, cancelTokenConnect);
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
            return;
        }
        
        connected = true;

        /*cancelTokenReadSocket = new CancellationToken();
        taskProcessIncomingMsgs = Task.Run(ReadSocket, cancelTokenReadSocket);

        cancelTokenWriteSocket = new CancellationToken();
        taskFlushOutgoingMsgs = Task.Run(WriteSocket, cancelTokenWriteSocket);*/
    }

    public async void TearDown()
    {
        connected = false;
        if (clientWebsocket != null)
        {
#if WINDOWS_UWP
            Debug.LogError("UBII UbiiTopicDataClientWS.TearDown()");
            clientWebsocket.Close(1000, "Client Node stopped");  // constants defined somewhere?
            clientWebsocket.Dispose();
#else
            CancellationToken cancellationToken = new CancellationToken();
            await clientWebsocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "de-initializing unity websocket client", cancellationToken);
            clientWebsocket.Dispose();
#endif
        }
    }

#if WINDOWS_UWP
    private async void ReadSocket()
    {
        Debug.LogError("WINDOWS_UWP UbiiTopicDataClientWS.ReadSocket()");
    }

    private async void WriteSocket()
    {
        Debug.LogError("WINDOWS_UWP UbiiTopicDataClientWS.WriteSocket()");
    }

    public async Task<CancellationToken> SendBytes(byte[] bytes)
    {
        Debug.LogError("WINDOWS_UWP UbiiTopicDataClientWS.SendBytes()");
        return new CancellationToken();
    }
#else
    private async void ReadSocket()
    {
        while (clientWebsocket.State == System.Net.WebSockets.WebSocketState.Open && !cancelTokenReadSocket.IsCancellationRequested)
        {
            byte[] bytebuffer = new byte[RECEIVE_BUFFER_SIZE];
            ArraySegment<byte> arraySegment = new ArraySegment<byte>(bytebuffer);
            System.Net.WebSockets.WebSocketReceiveResult receiveResult = null;

            try
            {
                receiveResult = await this.clientWebsocket.ReceiveAsync(arraySegment, cancelTokenReadSocket);
            }
            catch (Exception ex)
            {
                Debug.LogError("WebSocket receive exception: " + ex.ToString());
            }

            if (receiveResult.EndOfMessage)
            {
                // PING message
                if (receiveResult.Count == 4)
                {
                    string msgString = Encoding.UTF8.GetString(arraySegment.AsMemory().ToArray(), 0, receiveResult.Count);
                    if (msgString == "PING")
                    {
                        await this.SendBytes(Encoding.UTF8.GetBytes("PONG"));
                    }
                }
                // topic data
                else
                {
                    await msReadBuffer.WriteAsync(arraySegment.Array, arraySegment.Offset, receiveResult.Count, cancelTokenReadSocket);
                    TopicData topicdata = null;
                    try
                    {
                        topicdata = TopicData.Parser.ParseFrom(msReadBuffer.GetBuffer(), 0, (int)msReadBuffer.Position);

                        if (topicdata.TopicDataRecord != null)
                        {
                            this.InvokeTopicCallbacks(topicdata.TopicDataRecord);
                        }

                        if (topicdata.TopicDataRecordList != null)
                        {
                            foreach (TopicDataRecord record in topicdata.TopicDataRecordList.Elements)
                            {
                                this.InvokeTopicCallbacks(record);
                            }
                        }

                        if (topicdata.Error != null)
                        {
                            Debug.LogError(topicdata.Error.ToString());
                        }

                        msReadBuffer.Position = 0;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                }
            }
        }
    }

    private async void WriteSocket()
    {
        //TODO: introduce publish frequency settings
        while (clientWebsocket.State == System.Net.WebSockets.WebSocketState.Open && !cancelTokenWriteSocket.IsCancellationRequested)
        {
            try
            {
                await FlushRecordsToPublish();
            }
            catch (Exception ex)
            {
                Debug.LogError("WebSocket send exception: " + ex.ToString());
            }
        }
    }

    public async Task<CancellationToken> SendBytes(byte[] bytes)
    {
        var arraySegment = new ArraySegment<Byte>(bytes);
        CancellationToken cancellationToken = new CancellationToken();
        await clientWebsocket.SendAsync(arraySegment, System.Net.WebSockets.WebSocketMessageType.Binary, true, cancellationToken);
        return cancellationToken;
    }
#endif

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
        recordsToPublish.Add(record);
    }

    #region private-methods

    private async Task<CancellationToken> FlushRecordsToPublish()
    {
        if (recordsToPublish.IsEmpty) return new CancellationToken(true);

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

        return await SendTopicDataImmediately(topicData);
    }

    public async Task<CancellationToken> SendTopicDataImmediately(TopicData topicData)
    {
        MemoryStream memoryStream = new MemoryStream();
        CodedOutputStream codedOutputStream = new CodedOutputStream(memoryStream);
        topicData.WriteTo(codedOutputStream);
        codedOutputStream.Flush();
        var bytebuffer = memoryStream.ToArray();
        return await this.SendBytes(bytebuffer);
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
