﻿using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using Ubii.TopicData;
using System.Runtime.InteropServices;

public class UbiiTopicDataClientNetMQ : ITopicDataClient
{
    static string LOG_TAG = "UbiiTopicDataClientNetMQ";
    static int TIMEOUT_SECONDS_SEND = 3;

    private string address;
    private string clientID;

    private DealerSocket socket;
    private bool connected = false;

    private Task taskProcessIncomingMessages = null;
    NetMQPoller poller;

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

        taskProcessIncomingMessages = Task.Factory.StartNew(() =>
        {
            poller = new NetMQPoller();
            socket = new DealerSocket();
            socket.Options.Identity = Encoding.UTF8.GetBytes(clientID); // socket needs clientID for communication Dealer-Router
            socket.ReceiveReady += OnMessage;

            StartSocket();

            poller.Add(socket);
            poller.RunAsync();
        }, ctsProcessIncomingMsgs.Token);
    }

    /// <summary>
    /// Close the client.
    /// </summary>
    public async Task<bool> TearDown()
    {
        connected = false;
        try
        {
            ctsProcessIncomingMsgs.Cancel();
            await taskProcessIncomingMessages;
            if (poller.IsRunning)
            {
                poller.Stop();
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
    }

    /// <summary>
    /// Called when messages are received.
    /// </summary>
    private void OnMessage(object sender, NetMQSocketEventArgs eventArgs)
    {
        try
        {
            eventArgs.Socket.ReceiveFrameBytes(out bool hasmore);
            TopicData topicData = new TopicData { };
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
                    topicData.MergeFrom(bytes);
                }
            }

            CbHandleMessage(topicData);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }
}