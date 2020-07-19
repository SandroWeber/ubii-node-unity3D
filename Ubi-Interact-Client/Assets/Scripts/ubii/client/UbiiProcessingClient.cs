using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.Interactions;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using UnityEngine;

public class UbiiProcessingClient : MonoBehaviour
{
    private string servicehost;
    private int serviceport;
    private string topicdatahost;
    private int topicdataport;
    private string clientID;
    RequestSocket serviceSocket;
    DealerSocket topicdataSocket;
    bool connected = false;

    TaskCompletionSource<ServiceReply> promise = new TaskCompletionSource<ServiceReply>();

    private Dictionary<string, Action<TopicDataRecordList>> onProcessingCallbacks;
    private Dictionary<string, Action<TopicDataRecordList>> onCreatedCallbacks;
    private bool running = false;
    private Task processIncomingMessages = null;
    NetMQPoller poller;

    private InteractionStatus status;
    private List<ProcessingModule> processingModules;

    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken cancellationToken;

    const int delay = 3000; // milliseconds

    public UbiiProcessingClient(string servicehost, int serviceport, string topicdatahost, int topicdataport string clientID)
    {
        this.servicehost = servicehost;
        this.serviceport = serviceport;
        this.topicdatahost = topicdatahost;
        this.topicdataport = topicdataport;
        this.clientID = clientID;
        onProcessingCallbacks = new Dictionary<string, Action<TopicDataRecordList>>();
        onCreatedCallbacks = new Dictionary<string, Action<TopicDataRecordList>>();

        Initialize();
    }

    private void StartSockets()
    {
        AsyncIO.ForceDotNet.Force();
        serviceSocket = new RequestSocket();
        try
        {
            serviceSocket.Connect("ipc://" + servicehost + ":" + serviceport);
            Debug.Log("Create servicesocket successful. Host: " + servicehost + ":" + serviceport);
            connected = true;
        }catch(Exception ex)
        {
            Debug.LogError("ProcessingClient, StartSocket(), Exception occured: " + ex.ToString());
        }
        if (connected)
        {
            try
            {
                topicdataSocket.Connect("ipc://" + topicdatahost + ":" + topicdataport);
                Debug.Log("Create topicdatasocket successful. Host: " + topicdatahost + ":" + topicdataport);
            }
            catch (Exception ex)
            {
                connected = false;
                Debug.LogError("ProcessingClient, StartSocket(), Exception occured: " + ex.ToString());
            }
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
            topicdataSocket = new DealerSocket();
            topicdataSocket.Options.Identity = Encoding.UTF8.GetBytes(clientID); // socket needs clientID for Dealer-Router
            topicdataSocket.ReceiveReady += OnMessage; // event on receive data

            StartSockets();

            poller.Add(topicdataSocket);
            poller.RunAsync();

            while (running)
            {
                // TODO if pm mode frequency
                Thread.Sleep(delay);
                if (cancellationToken.IsCancellationRequested)
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

    public Task<ServiceReply> CallService(ServiceRequest srq)
    {
        // Convert serviceRequest into byte array which is then sent to server as frame
        byte[] buffer = srq.ToByteArray();
        serviceSocket.SendFrame(buffer);

        // Receive, return Task
        promise = new TaskCompletionSource<ServiceReply>();
        promise.TrySetResult(ServiceReply.Parser.ParseFrom(serviceSocket.ReceiveFrameBytes()));
        return promise.Task;
    }

    public void SendTopicData(TopicData td)
    {
        byte[] buffer = td.ToByteArray();
        topicdataSocket.SendFrame(buffer);
    }

    // called when data received
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
            /*string topic = topicData.TopicDataRecord.Topic;
            if (topicdataCallbacks.ContainsKey(topic))
            {
                topicdataCallbacks[topic].Invoke(topicData.TopicDataRecord);
            }
            else
            {
                foreach (KeyValuePair<string, Action<TopicDataRecord>> entry in topicdataRegexCallbacks)
                {
                    Match m = Regex.Match(topic, entry.Key);
                    if (m.Success)
                    {
                        entry.Value.Invoke(topicData.TopicDataRecord);
                    }
                }
            }*/
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
        serviceSocket.Close();
        NetMQConfig.Cleanup(false);
        connected = false;

        cts.Cancel();
        running = false;
        NetMQConfig.Cleanup(false);
        if (poller.IsRunning)
        {
            poller.StopAsync();
            poller.Stop();
        }
    }
}
