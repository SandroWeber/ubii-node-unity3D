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
    private string client_id;
    private string host;
    private int port;
    private ClientWebSocket client_websocket = null;

    private Dictionary<string, Action<TopicDataRecord>> topicdata_callbacks = null;
    private bool running = false;
    private Task taskProcessIncomingMsgs = null;
    private CancellationToken cancelTokenTaskProcessIncomingMsgs;

    public UbiiTopicDataClientWS(string client_id = null, string host = "localhost", int port = 8104)
    {
        this.client_id = client_id;
        this.host = host;
        this.port = port;

        topicdata_callbacks = new Dictionary<string, Action<TopicDataRecord>>();

        Initialize();
    }

    private async void Initialize()
    {
        client_websocket = new ClientWebSocket();

        Uri url = new Uri("ws://" + this.host + ":" + this.port + "?clientID=" + this.client_id);
        CancellationToken cancellation_token_connect = new CancellationToken();
        await client_websocket.ConnectAsync(url, cancellation_token_connect);

        running = true;
        cancelTokenTaskProcessIncomingMsgs = new CancellationToken();
        taskProcessIncomingMsgs = Task.Run(async () =>
        {
            while (running)
            {
                ArraySegment<Byte> buffer = new ArraySegment<Byte>();
                await client_websocket.ReceiveAsync(buffer, cancelTokenTaskProcessIncomingMsgs);
                TopicData topicdata = TopicData.Parser.ParseFrom(buffer.Array);
                Debug.Log(topicdata);

                if (topicdata.TopicDataRecord != null)
                {
                    topicdata_callbacks[topicdata.TopicDataRecord.Topic]?.Invoke(topicdata.TopicDataRecord);
                }
                else if (topicdata.Error != null)
                {
                    Debug.LogError(topicdata.Error.ToString());
                }
            }
        });
    }

    public async void DeInitialize()
    {
        running = false;
        if (client_websocket != null)
        {
            CancellationToken cancellation_token = new CancellationToken();
            await client_websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "de-initializing unity websocket client", cancellation_token);
            client_websocket.Dispose();
        }
    }

    public void TearDown()
    {
        //TODO
    }

    public bool IsConnected()
    {
        //TOOD
        return false;
    }

    public bool IsSubscribed(string topicOrRegex)
    {
        //TODO
        return false;
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
        this.topicdata_callbacks.Add(topic, callback);
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
        await client_websocket.SendAsync(array_segment, WebSocketMessageType.Binary, true, cancellation_token);
        /*try
        {
            await client_websocket.SendAsync(array_segment, WebSocketMessageType.Binary, true, cancellation_token);
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
