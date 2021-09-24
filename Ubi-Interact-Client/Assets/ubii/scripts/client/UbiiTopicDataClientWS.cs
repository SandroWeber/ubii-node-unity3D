using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Ubii.TopicData;
using Google.Protobuf;

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
    private CancellationToken cancelTokenTaskProcessIncomingMsgs;

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
        CancellationToken cancelTokenConnect = new CancellationToken();
        await clientWebsocket.ConnectAsync(url, cancelTokenConnect);

        Debug.Log("websocket after ConnectAsync()");
        Debug.Log(clientWebsocket);

        connected = true;
        cancelTokenTaskProcessIncomingMsgs = new CancellationToken();
        taskProcessIncomingMsgs = Task.Run(async () =>
        {
            while (connected)
            {
                ArraySegment<Byte> buffer = new ArraySegment<Byte>();
                await clientWebsocket.ReceiveAsync(buffer, cancelTokenTaskProcessIncomingMsgs);
                TopicData topicdata = TopicData.Parser.ParseFrom(buffer.Array);
                Debug.Log(topicdata);

                if (topicdata.TopicDataRecord != null)
                {
                    topicCallbacks[topicdata.TopicDataRecord.Topic]?.Invoke(topicdata.TopicDataRecord);
                }
                else if (topicdata.Error != null)
                {
                    Debug.LogError(topicdata.Error.ToString());
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
        //TODO
        return this.topicCallbacks.ContainsKey(topicOrRegex) || this.topicRegexCallbacks.ContainsKey(topicOrRegex);
    }

    public bool HasTopicCallbacks(string topic)
    {
        //TODO
        return false;
    }

    public bool HasTopicRegexCallbacks(string regex)
    {
        //TODO
        return false;
    }

    public List<string> GetAllSubscribedTopics()
    {
        //TODO
        return new List<string>();
    }

    public List<string> GetAllSubscribedRegex()
    {
        //TODO
        return new List<string>();
    }

    public void SetPublishDelay(int millisecs)
    {
        //TODO
    }

    public void AddTopicDataCallback(string topic, Action<TopicDataRecord> callback)
    {
        //TODO
        this.topicCallbacks.Add(topic, callback);
    }

    public void AddTopicDataRegexCallback(string regex, Action<TopicDataRecord> callback)
    {
        //TODO
    }

    public void RemoveAllTopicCallbacks(string topic)
    {
        //TODO
    }

    public void RemoveTopicCallback(string topic, Action<TopicDataRecord> callback)
    {
        //TODO
    }

    public void RemoveAllTopicRegexCallbacks(string regex)
    {
        //TODO
    }

    public void RemoveTopicRegexCallback(string regex, Action<TopicDataRecord> callback)
    {
        //TODO
    }

    public void SendTopicDataRecord(TopicDataRecord record)
    {
        //TODO
    }

    public void SendTopicDataImmediately(TopicData td)
    {
        //TODO
    }

    public async void Send(TopicData topicdata)
    {
        Debug.Log("topicdata: " + topicdata);
        
        MemoryStream memory_stream = new MemoryStream();
        CodedOutputStream coded_output_stream = new CodedOutputStream(memory_stream);
        topicdata.WriteTo(coded_output_stream);
        coded_output_stream.Flush();
        Debug.Log("coded_output_stream: " + coded_output_stream);
        var bytebuffer = memory_stream.ToArray();
        Debug.Log("buffer length: " + bytebuffer.Length);

        var array_segment = new ArraySegment<Byte>(bytebuffer);
        Debug.Log("array_segment length: " + array_segment.Count);
        CancellationToken cancellation_token = new CancellationToken();
        await clientWebsocket.SendAsync(array_segment, WebSocketMessageType.Binary, true, cancellation_token);
        /*try
        {
            await clientWebsocket.SendAsync(array_segment, WebSocketMessageType.Binary, true, cancellation_token);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }*/
    }

    public void SendTestTopicData(string topic)
    {
        try
        {
            TopicData topicdata = new TopicData { TopicDataRecord = new TopicDataRecord { Topic = topic, Double = 1 } };
            Send(topicdata);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }
}
