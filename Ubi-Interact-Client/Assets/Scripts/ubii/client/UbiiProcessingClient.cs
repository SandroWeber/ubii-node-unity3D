using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.Interactions;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using Google.Protobuf.Collections;
using UnityEngine;
using System.Linq;

public class UbiiProcessingClient : MonoBehaviour
{
    private string servicehost;
    private int serviceport;
    private string topicdatahost;
    private int topicdataport;
    private string clientID;
    RequestSocket serviceSocket;
    DealerSocket topicdataSocket;
    bool connected = false;

    TaskCompletionSource<ServiceReply> promise = new TaskCompletionSource<ServiceReply>();

    private bool startProcessing = false;
    private List<ProcessingModule> processingModules;
    private List<TopicData> inputTopicDatas;
    private List<TopicData> outputTopicDatas;
    private Dictionary<string, TopicDataRecordList> gatheredOutputs;

    private bool running = false;
    private Task processIncomingMessages = null;
    NetMQPoller poller;

    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken cancellationToken;

    //TODO replace with pm.frequency
    const int delay = 3000; // milliseconds

    public UbiiProcessingClient(string servicehost, int serviceport, string topicdatahost, int topicdataport, string clientID)
    {
        this.servicehost = servicehost;
        this.serviceport = serviceport;
        this.topicdatahost = topicdatahost;
        this.topicdataport = topicdataport;
        this.clientID = clientID;

        processingModules = new List<ProcessingModule>();
        inputTopicDatas = new List<TopicData>();
        outputTopicDatas = new List<TopicData>();
        gatheredOutputs = new Dictionary<string, TopicDataRecordList>();

        Initialize();

        if (connected)
        {
            getProcessingModuleLoop();
            LockstepMaster();
            AsyncFrequencyMaster();
            AsyncMaster();
        }
    }

    private void StartSockets()
    {
        AsyncIO.ForceDotNet.Force();
        serviceSocket = new RequestSocket();
        try
        {
            serviceSocket.Connect("ipc://" + servicehost + ":" + serviceport);
            Debug.Log("Create servicesocket successful. Host: " + servicehost + ":" + serviceport);
            connected = true;
        }catch(Exception ex)
        {
            Debug.LogError("ProcessingClient, StartSocket(), Exception occured: " + ex.ToString());
        }
        if (connected)
        {
            try
            {
                topicdataSocket.Connect("ipc://" + topicdatahost + ":" + topicdataport);
                Debug.Log("Create topicdatasocket successful. Host: " + topicdatahost + ":" + topicdataport);
            }
            catch (Exception ex)
            {
                connected = false;
                Debug.LogError("ProcessingClient, StartSocket(), Exception occured: " + ex.ToString());
            }
        }
    }

    private void Initialize()
    {
        running = true;
        cancellationToken = cts.Token;

        processIncomingMessages = Task.Factory.StartNew(() =>
        {
            // instantiate poller and socket
            poller = new NetMQPoller();
            topicdataSocket = new DealerSocket();
            topicdataSocket.Options.Identity = Encoding.UTF8.GetBytes(clientID); // socket needs clientID for Dealer-Router
            topicdataSocket.ReceiveReady += OnMessage; // event on receive data

            StartSockets();

            poller.Add(topicdataSocket);
            poller.RunAsync();

            while (running)
            {
                // TODO if pm mode frequency
                Thread.Sleep(delay);
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Cancelling task");
                    break;
                }
            }
        }, cancellationToken);
    }

    public bool IsConnected()
    {
        return connected;
    }

    async private void getProcessingModuleLoop()
    {
        while (connected)
        {
            //TODO fill in service request get pm specs
            Ubii.Services.ServiceReply newProcessingModule = await CallService(new Ubii.Services.ServiceRequest
            {

            });

            processingModules.Add(newProcessingModule.ProcessingModule);
            int index = processingModules.Count - 1;

            // call oncreated function
            if (newProcessingModule.ProcessingModule.OnCreated != "")
            {
                string[] function = Regex.Split(newProcessingModule.ProcessingModule.OnCreated, ".");
                executeFunction(function[0], function[1]);
            }

            newProcessingModule.ProcessingModule.Status = InteractionStatus.Initialized;

            switch (newProcessingModule.ProcessingModule.Mode)
            {
                case ProcessingMode.Lockstep:
                    LockstepMaster();
                    break;
                case ProcessingMode.Atfrequency:
                    AsyncFrequencyMode(index);
                    break;
                case ProcessingMode.Atnewtopicdata:
                    AsyncMode(index);
                    break;
            }
        }
    }

    //TODO make those multi threaded
    async private void LockstepMaster()
    {
        while (connected)
        {
            await GetProcessingCall();
            bool allFinished = true;
            foreach(ProcessingModule pm in processingModules)
            {
                if(pm.Mode == ProcessingMode.Lockstep)
                {
                    // start processing
                    if (pm.Status != InteractionStatus.Halted)
                    {
                        allFinished = false;
                    }
                }
            }
            if (allFinished)
            {
                //TODO fill in service request to send output TopicDataRecordList
                Ubii.Services.ServiceReply newProcessingModule = await CallService(new Ubii.Services.ServiceRequest
                {

                });
            }
        }
    }

    async private Task GetProcessingCall()
    {
        // wait for call to process
        await CallService(new Ubii.Services.ServiceRequest
        {

        });

        startProcessing = true;
    }

    private void LockstepMode(int index)
    {
        Task processStep = Task.Factory.StartNew(() =>
        {
            while (!startProcessing)
            {

            }
            // process
            if (processingModules[index].OnProcessing != "")
            {
                processingModules[index].Status = InteractionStatus.Processing;
                string[] function = Regex.Split(processingModules[index].OnProcessing, ".");
                executeFunction(function[0], function[1]);
                Type type = Type.GetType(function[0]);

                TopicDataRecordList output = (TopicDataRecordList)type.InvokeMember(
                    function[1],
                    BindingFlags.InvokeMethod | BindingFlags.Public |
                    BindingFlags.Static, null, null, processingModules[index].Lockstep.InputTopicDataRecordList.ToArray<TopicDataRecordList>());
                // gather topicdatarecordlist
                gatheredOutputs.Add(processingModules[index].Id, output);
            }
        });        
    }

    async private void AsyncFrequencyMaster()
    {
        while (connected)
        {

        }
    }

    async private void AsyncFrequencyMode(int index)
    {

    }

    async private void AsyncMaster()
    {
        while (connected)
        {

        }
    }

    async private void AsyncMode(int index)
    {

    }

    private void executeFunction(string clss, string name)
    {
        Type type = Type.GetType(clss);

        type.InvokeMember(
            name,
            BindingFlags.InvokeMethod | BindingFlags.Public |
            BindingFlags.Static, null, null, null);
    }

    public Task<ServiceReply> CallService(ServiceRequest srq)
    {
        // Convert serviceRequest into byte array which is then sent to server as frame
        byte[] buffer = srq.ToByteArray();
        serviceSocket.SendFrame(buffer);

        // Receive, return Task
        promise = new TaskCompletionSource<ServiceReply>();
        promise.TrySetResult(ServiceReply.Parser.ParseFrom(serviceSocket.ReceiveFrameBytes()));
        return promise.Task;
    }

    // called when topicdata received
    void OnMessage(object sender, NetMQSocketEventArgs e)
    {
        e.Socket.ReceiveFrameBytes(out bool hasmore);
        TopicData newTopicData = new TopicData { };
        if (hasmore)
        {
            newTopicData.MergeFrom(e.Socket.ReceiveFrameBytes(out hasmore));
        }

        //TODO make regex possible
        // refresh inputTopicdatas
        if (newTopicData.TopicDataRecord != null)
        {
            string topic = newTopicData.TopicDataRecord.Topic;
            int index = 0;
            foreach(TopicData inputTopicData in inputTopicDatas)
            {
                if(inputTopicData.TopicDataRecord.Topic == topic)
                {
                    break;
                }
                index++;
            }
            inputTopicDatas[index] = newTopicData;
        }
        // catch possible error
        else if (newTopicData.Error != null)
        {
            Debug.LogError("topicData Error: " + newTopicData.Error.ToString());
            return;
        }
    }

    public void SendTopicData(TopicData td)
    {
        byte[] buffer = td.ToByteArray();
        topicdataSocket.SendFrame(buffer);
    }

    public void TearDown()
    {
        serviceSocket.Close();
        NetMQConfig.Cleanup(false);
        connected = false;

        cts.Cancel();
        running = false;
        NetMQConfig.Cleanup(false);
        if (poller.IsRunning)
        {
            poller.StopAsync();
            poller.Stop();
        }
    }
}
