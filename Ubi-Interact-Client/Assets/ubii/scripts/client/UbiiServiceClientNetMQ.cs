using System;
using System.Threading.Tasks;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

using Google.Protobuf;
using Ubii.Services;

class UbiiServiceClientNetMQ : IUbiiServiceClient
{
    static string LOG_TAG = "UbiiServiceClientNetMQ";
    static int TIMEOUT_SECONDS_CALLSERVICE = 5;

    private string masterNodeAddress;
    private RequestSocket socket;
    private CancellationTokenSource ctsCallService = null;
    private Task<ServiceReply> taskCallService = null;

    private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

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
        taskCallService = Task.Run(() =>
        {
            ServiceReply response = null;
            bool success = false;

            while (!success && !ctsCallService.IsCancellationRequested)
            {
                _semaphoreSlim.Wait(ctsCallService.Token);
                try
                {
                    byte[] buffer = request.ToByteArray();
                    if (!socket.TrySendFrame(buffer))
                    {
                        throw new Exception("Failed to send buffer on socket.");
                    }
                    byte[] responseByteArray;
                    if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(0.5f), out responseByteArray))
                    {
                        response = ServiceReply.Parser.ParseFrom(responseByteArray);
                        success = true;
                    }
                    _semaphoreSlim.Release();
                }
                catch (Exception exception)
                {
                    _semaphoreSlim.Release();
                    Debug.LogWarning("UBII - " + LOG_TAG + ".CallService(): " + exception.ToString());
                    this.StartSocket();
                    Task.Delay(100).Wait(ctsCallService.Token);
                }
            }

            return response;
        }, ctsCallService.Token);

        return taskCallService;
    }

    public async void TearDown()
    {
        try
        {
            ctsCallService?.Cancel();
            await taskCallService;
            _semaphoreSlim.Dispose();

            if (socket != null)
            {
                socket.Disconnect("tcp://" + masterNodeAddress);
                socket.Close();
                socket.Dispose();
                socket = null;
            }

            NetMQConfig.Cleanup(false);
        }
        catch (Exception ex) {
            Debug.LogError(ex.ToString());
        }
    }
}
