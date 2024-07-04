using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using Ubii.TopicData;
using System.Collections.Concurrent;

public class UbiiTopicDataClientNetMQ : ITopicDataClient
{
    static string LOG_TAG = "UbiiTopicDataClientNetMQ";
    static int TIMEOUT_SECONDS_SEND = 3;

    private string address;
    private string clientID;

    private DealerSocket socket;
    private bool connected = false;

    NetMQPoller poller;
    //NetMQQueue<byte[]> netMQQueue;
    //NetMQQueue<string> netMQQueueString;
    ConcurrentBag<byte[]> concurrentBagSendData;

    CancellationTokenSource ctsProcessIncomingMsgs = new CancellationTokenSource();

    private UbiiNetworkClient.CbHandleTopicData CbHandleMessage = null;
    private UbiiNetworkClient.CbTopicDataConnectionLost CbTopicDataConnectionLost = null;

    public UbiiTopicDataClientNetMQ(
        string clientID,
        string address = "localhost:8103",
        UbiiNetworkClient.CbHandleTopicData cbHandleMessage = null,
        UbiiNetworkClient.CbTopicDataConnectionLost CbTopicDataConnectionLost = null)
    {
        this.clientID = clientID; //global variable not neccesarily needed; only for socket.Options.Identity
        this.address = address;
        this.CbHandleMessage = cbHandleMessage;
        this.CbTopicDataConnectionLost = CbTopicDataConnectionLost;
        concurrentBagSendData = new ConcurrentBag<byte[]>();

        Initialize();
    }

    private void StartSocket()
    {
        try
        {
            socket.Connect("tcp://" + this.address);
            connected = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("UBII - " + LOG_TAG + ".StartSocket(): " + ex.ToString());
        }
    }

    /// <summary>
    /// Initialize socket connection and run task to receive data.
    /// </summary>
    private void Initialize()
    {
        if (CbHandleMessage == null)
        {
            Debug.LogError("UBII - " + LOG_TAG + " has no callback for handling TopicData");
            return;
        }

        socket = new DealerSocket();
        socket.Options.Identity = Encoding.UTF8.GetBytes(clientID); // socket needs clientID for Dealer-Router communication
        socket.ReceiveReady += OnMessage;
        socket.SendReady += EmptySendQueue;
        StartSocket();

        poller = new NetMQPoller();
        poller.Add(socket);
        poller.RunAsync();
    }

    public async Task<bool> ShutDownGracefully()
    {
        connected = false;
        try
        {
            ctsProcessIncomingMsgs.Cancel();
            if (poller.IsRunning)
            {
                poller.StopAsync();
                NetMQConfig.Cleanup(false);
                return true;
            }
        }
        catch (TerminatingException) { }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return false;
    }

    public bool ShutDownImmediately()
    {
        connected = false;
        try
        {
            if (poller.IsRunning)
            {
                poller.StopAsync();
                NetMQConfig.Cleanup(false);
                return true;
            }
        }
        catch (TerminatingException) { }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return false;
    }

    /// <summary>
    /// Get connection status.
    /// </summary>
    public bool IsConnected()
    {
        return connected;
    }

    /// <summary>
    /// Used to send TopicData to the master node.
    /// </summary>
    /// <param name="topicData">Data to be sent.</param>
    /// <param name="ct">CancellationToken (not used in this implementation of interface ITopicDataClient).</param>
    public Task<bool> Send(TopicData topicData, CancellationToken ct)
    {
        try
        {
            byte[] buffer = topicData.ToByteArray();
            AddToSendQueue(buffer);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public void AddToSendQueue(byte[] data)
    {
        this.concurrentBagSendData.Add(data);
    }

    private void EmptySendQueue(object sender, NetMQSocketEventArgs e)
    {
        while (!concurrentBagSendData.IsEmpty)
        {
            try
            {
                byte[] data;
                concurrentBagSendData.TryTake(out data);
                SendBinary(data);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }
    }

    private bool SendBinary(byte[] buffer)
    {
        try
        {
            bool success = socket.TrySendFrame(TimeSpan.FromSeconds(TIMEOUT_SECONDS_SEND), buffer);
            if (!success)
            {
                Debug.LogError("UBII - SendBinary() failed");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            return false;
        }

        return true;
    }

    /// <summary>
    /// Used to send TopicData to the master node.
    /// </summary>
    /// <param name="topicData">Data to be sent.</param>
    /// <param name="ct">CancellationToken (not used in this implementation of interface ITopicDataClient).</param>
    /*public Task<bool> SendDirectly(TopicData topicData, CancellationToken ct)
    {
        try
        {
            byte[] buffer = topicData.ToByteArray();
            bool success = socket.TrySendFrame(TimeSpan.FromSeconds(TIMEOUT_SECONDS_SEND), buffer);
            if (!success)
            {
                Debug.LogError("UBII - TopicData could not be sent: " + topicData.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }*/

    /// <summary>
    /// Called when messages are received.
    /// </summary>
    private void OnMessage(object sender, NetMQSocketEventArgs eventArgs)
    {
        try
        {
            eventArgs.Socket.ReceiveFrameBytes(out bool hasmore);
            if (hasmore)
            {
                byte[] bytes = eventArgs.Socket.ReceiveFrameBytes(out hasmore);

                if (bytes.Length == 4)
                {
                    string msgString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    if (msgString == "PING")
                    {
                        socket.TrySendFrame(Encoding.UTF8.GetBytes("PONG"));
                        return;
                    }
                }
                else
                {
                    TopicData topicData = new TopicData { };
                    topicData.MergeFrom(bytes);
                    CbHandleMessage(topicData);
                }
            }
        }
        catch (NetMQ.TerminatingException ex) {}
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }
}