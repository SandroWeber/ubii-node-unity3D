using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.UtilityFunctions.Parser;

public class UbiiClient : MonoBehaviour, IUbiiClient
{
    protected NetMQUbiiClient client;

    [Header("Network configuration")]
    [Tooltip("Host ip the client connects to. Default is localhost.")]
    public string ip = "localhost";
    [Tooltip("Port for the client connection to the server. Default is 8101.")]
    public int port = 8101;
    [Tooltip("Name for the client connection to the server. Default is Unity3D Client.")]
    public string clientName = "Unity3D Client";

    public async Task InitializeClient()
    {
        client = new NetMQUbiiClient(null, clientName, ip, port);
        await client.Initialize();
    }

	public string GetID()
	{
		return client.GetClientID();
	}

    public Task<ServiceReply> CallService(ServiceRequest request)
    {
        return client.CallService(request);
    }

    public void Publish(TopicData topicData)
    {
        client.Publish(topicData);
    }

    public Task<bool> Subscribe(string topic, Action<TopicDataRecord> callback)
    {
        return client.SubscribeTopic(topic, callback);
    }

    public Task<bool> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        return client.SubscribeRegex(regex, callback);
    }

    public Task<bool> Unsubscribe(string topic, Action<TopicDataRecord> callback)
    {
        return client.UnsubscribeTopic(topic, callback);
    }

    public bool IsConnected()
    {
        return client.IsConnected();
    }

    public Task WaitForConnection()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;
        return Task.Run(() =>
        {
            int maxRetries = 100;
            int currentTry = 1;
            while (client == null && currentTry <= maxRetries)
            {
                currentTry++;
                Thread.Sleep(100);
            }

            while (!IsConnected() && currentTry <= maxRetries)
            {
                currentTry++;
                Thread.Sleep(100);
            }

            if (currentTry > maxRetries)
            {
                cts.Cancel();
            }
        }, token);

        //return client.WaitForConnection();
    }

    async private void Start()
    {
        await InitializeClient();
    }

    private void OnDisable()
    {
        if (client != null)
        {
            client.ShutDown();
        }
        Debug.Log("Shutting down UbiiClient");
    }
}

