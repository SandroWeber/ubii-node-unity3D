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
using System.Threading;

class NetMQServiceClient : IUbiiServiceClient
{
    static int MAX_RETRIES_CALL_SERVICE = 3;
    static int TIMEOUT_SECONDS_CALL_SERVICE_RECEIVE = 1;

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

    /*public Task<ServiceReply> CallService(ServiceRequest request)
    {
        TaskCompletionSource<ServiceReply> promise = new TaskCompletionSource<ServiceReply>();
        bool success = false;
        // TODO: This loops will stay endless 
        int tries = MAX_RETRIES_CALL_SERVICE;
        while (!success && tries > 0)
        {
            tries--;
            try
            {
                // Convert serviceRequest into byte array which is then sent to server as frame
                byte[] buffer = request.ToByteArray();
                socket.SendFrame(buffer);

                // Receive, return Task
                promise = new TaskCompletionSource<ServiceReply>();
                CancellationToken ctoken = new CancellationTokenSource(TimeSpan.FromSeconds(3).Milliseconds).Token;
                // Receive, return Task
                promise = new TaskCompletionSource<ServiceReply>();
                Task.Run(() => promise.TrySetResult(ServiceReply.Parser.ParseFrom(socket.ReceiveFrameBytes())), ctoken);
                success = true;

            }
            catch (Exception exception)
            {
                Debug.LogError("UBII NetMQServiceClient.CallService(): " + exception.ToString());
                Task.Delay(100).Wait();
            }
        }

        return promise.Task;
    }*/

    public Task<ServiceReply> CallService(ServiceRequest request)
    {
        CancellationToken ctoken = new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token;
        return Task.Run(() =>
        {
            ServiceReply response = null;
            bool success = false;
            int tries = MAX_RETRIES_CALL_SERVICE;
            while (!success && tries > 0)
            {
                Debug.LogError("UBII NetMQServiceClient.CallService(): tries remaining " + tries);
                tries--;
                try
                {
                    byte[] buffer = request.ToByteArray();
                    socket.SendFrame(buffer);
                    byte[] responseByteArray;
                    bool received = socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(TIMEOUT_SECONDS_CALL_SERVICE_RECEIVE), out responseByteArray);
                    response = ServiceReply.Parser.ParseFrom(responseByteArray);
                    success = true;
                }
                catch (Exception exception)
                {
                    Debug.LogError("UBII NetMQServiceClient.CallService(): " + exception.ToString());
                    Task.Delay(100).Wait();
                }
            }

            return response;
        }, ctoken);
    }

    public void TearDown()
    {
        socket.Close();
        NetMQConfig.Cleanup(false);
    }
}
