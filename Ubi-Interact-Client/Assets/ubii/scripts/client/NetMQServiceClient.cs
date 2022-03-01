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

class NetMQServiceClient : IUbiiServiceClient
{
    private string masterNodeAddress;
    private int port;

    RequestSocket socket;

    public NetMQServiceClient(string masterNodeAddress = "localhost:8101")
    {
        this.masterNodeAddress = masterNodeAddress;
        StartSocket();
    }

    // creates tcp connection to given host and port
    private void StartSocket()
    {
        AsyncIO.ForceDotNet.Force();
        socket = new RequestSocket();
        try
        {
            socket.Connect("tcp://" + masterNodeAddress);
        }
        catch (Exception ex)
        {
            Debug.LogError("UBII NetMQServiceClient.StartSocket(): " + ex.ToString());
        }
    }

    public Task<ServiceReply> CallService(ServiceRequest srq)
    {
        TaskCompletionSource<ServiceReply> promise = new TaskCompletionSource<ServiceReply>();
        bool success = false;
        while (!success)
        {
            try
            {
                // Convert serviceRequest into byte array which is then sent to server as frame
                byte[] buffer = srq.ToByteArray();
                socket.SendFrame(buffer);

                // Receive, return Task
                promise = new TaskCompletionSource<ServiceReply>();
                promise.TrySetResult(ServiceReply.Parser.ParseFrom(socket.ReceiveFrameBytes()));
                success = true;
            }
            catch (Exception exception)
            {
                Debug.LogError("UBII NetMQServiceClient.CallService(): " + exception.ToString());
                Task.Delay(100).Wait();
            }
        }
        return promise.Task;
    }

    public void TearDown()
    {
        socket.Close();
        NetMQConfig.Cleanup(false);
    }
}
