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

public class NetMQTopicDataClient
{
    private string host;
    private int port;
    private string clientID;

    private DealerSocket socket;
    private bool connected = false;

    private Dictionary<string, Action<TopicDataRecord>> topicdataCallbacks = null;
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

        topicdataCallbacks = new Dictionary<string, Action<TopicDataRecord>>();

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

    public void AddTopicDataCallback(string topic, Action<TopicDataRecord> callback)
    {
        this.topicdataCallbacks.Add(topic, callback);
    }

    public void RemoveTopicDataCallback(string topic)
    {
        Debug.Log("removing topicDataCallBack for topic: " + topic + " (backend)");
        this.topicdataCallbacks.Remove(topic);
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

            topicdataCallbacks[topicData.TopicDataRecord.Topic]?.Invoke(topicData.TopicDataRecord);
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
