using System;
using Ubii.Services;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

class UbiiServiceClientNetMQ : IUbiiServiceClient
{
    static string LOG_TAG = "UbiiServiceClientNetMQ";
    static int TIMEOUT_SECONDS_CALLSERVICE = 5;

    private string masterNodeAddress;
    private CancellationTokenSource ctsCallService = null;

    private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    RequestSocket socket;

    public UbiiServiceClientNetMQ(string masterNodeAddress = "localhost:8101")
    {
        this.masterNodeAddress = masterNodeAddress;
        StartSocket();
    }

    // creates tcp connection to given host and port
    private void StartSocket()
    {
        try
        {
            if (socket != null)
            {
                Debug.Log("[Ubii] Disconnect before reconnect Socket");
                socket.Disconnect("tcp://" + masterNodeAddress);
                socket.Dispose();
                socket = null;
            }
            AsyncIO.ForceDotNet.Force();
            socket = new RequestSocket();
            socket.Connect("tcp://" + masterNodeAddress);
        }
        catch (Exception ex)
        {
            Debug.LogError("UBII - " + LOG_TAG + ".StartSocket(): " + ex.ToString());
        }
    }

    public Task<ServiceReply> CallService(ServiceRequest request)
    {
        ctsCallService = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SECONDS_CALLSERVICE));
        return Task.Run(() =>
        {
            ServiceReply response = null;
            bool success = false;

            while (!success && !ctsCallService.IsCancellationRequested)
            {
                try
                {
                    _semaphoreSlim.Wait(ctsCallService.Token);
                    {
                        byte[] buffer = request.ToByteArray();
                        socket.SendFrame(buffer);
                        byte[] responseByteArray = socket.ReceiveFrameBytes();
                        response = ServiceReply.Parser.ParseFrom(responseByteArray);
                        success = true;
                    }
                    _semaphoreSlim.Release();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("UBII - " + LOG_TAG + ".CallService(): " + exception.ToString());
                    this.StartSocket();
                    Task.Delay(100).Wait(ctsCallService.Token);
                }
            }

            return response;
        }, ctsCallService.Token);
    }

    public void TearDown()
    {
        ctsCallService?.Cancel();
        socket.Close();
        _semaphoreSlim.Wait();
        if (socket != null)
        {
            socket.Disconnect("tcp://" + masterNodeAddress);
            socket.Dispose();
            socket = null;
        }
        NetMQConfig.Cleanup(false);
    }
}
