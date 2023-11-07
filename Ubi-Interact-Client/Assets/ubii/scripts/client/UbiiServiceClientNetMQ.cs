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
    static int MAX_RETRIES_CALL_SERVICE = 3;
    static int TIMEOUT_SECONDS_CALLSERVICE = 5;
    static int TIMEOUT_SECONDS_CALLSERVICE_SEND_RECEIVE = 2;

    private string masterNodeAddress;
    private int port;
    private CancellationTokenSource ctsCallService = null;

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
            int tries = MAX_RETRIES_CALL_SERVICE;
            while (!success && tries > 0)
            {
                tries--;
                try
                {
                    byte[] buffer = request.ToByteArray();
                    socket.TrySendFrame(TimeSpan.FromSeconds(TIMEOUT_SECONDS_CALLSERVICE_SEND_RECEIVE), buffer);
                    byte[] responseByteArray;
                    bool received = socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(TIMEOUT_SECONDS_CALLSERVICE_SEND_RECEIVE), out responseByteArray);
                    if (!received) continue;
                    response = ServiceReply.Parser.ParseFrom(responseByteArray);
                    success = true;
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
        NetMQConfig.Cleanup(false);
    }
}
