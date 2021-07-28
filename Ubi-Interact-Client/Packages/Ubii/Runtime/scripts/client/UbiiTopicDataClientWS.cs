using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Ubii.TopicData;
using Google.Protobuf;

public class UbiiTopicDataClientWS
{
    private string client_id;
    private string host;
    private int port;
    private ClientWebSocket client_websocket = null;

    private Dictionary<string, Action<TopicDataRecord>> topicdata_callbacks = null;
    private bool running = false;
    private Task process_incoming_msgs = null;
    private CancellationToken cancellation_token_process_incoming_msgs;

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
        cancellation_token_process_incoming_msgs = new CancellationToken();
        process_incoming_msgs = Task.Run(async () =>
        {
            while (running)
            {
                ArraySegment<Byte> buffer = new ArraySegment<Byte>();
                await client_websocket.ReceiveAsync(buffer, cancellation_token_process_incoming_msgs);
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

    public void AddTopicDataCallback(string topic, Action<TopicDataRecord> callback)
    {
        this.topicdata_callbacks.Add(topic, callback);
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
