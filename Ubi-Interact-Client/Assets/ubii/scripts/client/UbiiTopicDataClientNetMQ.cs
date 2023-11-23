﻿using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using Ubii.TopicData;

public class UbiiTopicDataClientNetMQ : ITopicDataClient
{
    static string LOG_TAG = "UbiiTopicDataClientNetMQ";
    static int TIMEOUT_SECONDS_SEND = 3;

    private string address;
    private string clientID;

    private DealerSocket socket;
    private bool connected = false;

    private Task processIncomingMessages = null;
    NetMQPoller poller;

    CancellationTokenSource ctsProcessIncomingMsgs = new CancellationTokenSource();

    private UbiiNetworkClient.CbHandleTopicData CbHandleMessage = null;

    public UbiiTopicDataClientNetMQ(string clientID, UbiiNetworkClient.CbHandleTopicData cbHandleMessage, string address = "localhost:8103")
    {
        this.address = address;
        this.clientID = clientID; //global variable not neccesarily needed; only for socket.Options.Identity
        this.CbHandleMessage = cbHandleMessage;

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

    private void Initialize()
    {
        if (CbHandleMessage == null)
        {
            Debug.LogError("UBII - " + LOG_TAG + " has no callback for handling TopicData");
            return;
        }

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
        }, ctsProcessIncomingMsgs.Token);
    }

    public void TearDown()
    {
        ctsProcessIncomingMsgs.Cancel();
        connected = false;

        try
        {
            if (poller.IsRunning)
            {
                poller.Stop();
                NetMQConfig.Cleanup(false);
            }
        }
        catch (TerminatingException) { }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public bool IsConnected()
    {
        return connected;
    }

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

    // Called when data received
    void OnMessage(object sender, NetMQSocketEventArgs e)
    {
        try
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