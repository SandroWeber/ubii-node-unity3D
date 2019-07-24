using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubii.Services;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using UnityEngine;
using System.Threading.Tasks;

class NetMQServiceClient: IUbiiServiceClient
{
    private string host;
    private int port;

    RequestSocket socket;

    TaskCompletionSource<ServiceReply> promise = new TaskCompletionSource<ServiceReply>();

    public NetMQServiceClient(string host = "localhost", int port = 8101)
    {
        this.host = host;
        this.port = port;
        StartSocket();
    }

    // creates tcp connection to given host and port
    private void StartSocket()
    {
        AsyncIO.ForceDotNet.Force();
        socket = new RequestSocket();
        try
        {
            socket.Connect("tcp://" + host + ":" + port);
            Debug.Log("Create Socket successful. Host: " + host + ":" + port);
        }
        catch (Exception ex)
        {
            Debug.LogError("NetMQServiceClient, StartSocket(), Exception occured: " + ex.ToString());
        }
    }

    public Task<ServiceReply> CallService(ServiceRequest srq)
    {
        // Convert serviceRequest into byte array which is then sent to server as frame
        byte[] buffer = srq.ToByteArray();
        socket.SendFrame(buffer);

        // Receive, return Task
        promise = new TaskCompletionSource<ServiceReply>();
        promise.TrySetResult(ServiceReply.Parser.ParseFrom(socket.ReceiveFrameBytes()));
        return promise.Task;
    }

    public void TearDown()
    {
        socket.Close();
        NetMQConfig.Cleanup(false);
    }
}
