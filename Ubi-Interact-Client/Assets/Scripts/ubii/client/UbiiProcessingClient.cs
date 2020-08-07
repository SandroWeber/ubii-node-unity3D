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
using Ubii.Clients;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using Google.Protobuf.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Linq;
using System.Diagnostics;

public class UbiiProcessingClient : MonoBehaviour
{
    private string servicehost;
    private int serviceport;
    private int responseport;
    private string topicdatahost;
    private int topicdataport;
    private string clientID;
    private List<ProcessingModule> processingModules;

    RequestSocket serviceSocket;
    ResponseSocket responseSocket;
    DealerSocket topicdataSocket;
    bool connected = false;
    bool registered = false;
    TaskCompletionSource<ServiceReply> promise = new TaskCompletionSource<ServiceReply>();
    Task[] tasks;

    private bool startProcessing = false;
    private List<TopicData> inputTopicDatas;
    private List<TopicData> outputTopicDatas;
    private Dictionary<string, TopicDataRecordList> gatheredOutputs;

    private bool running = false;
    private bool lockstepMasterRunning = false;
    private bool atFrequencyMasterRunning = false;
    private bool atNewTopicDataMasterRunning = false;
    private Task processIncomingMessages = null;
    NetMQPoller poller;
    NetMQPoller responsePoller;

    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken cancellationToken;

    public UbiiProcessingClient(string servicehost, int serviceport, int responseport, string topicdatahost, int topicdataport, string clientID, List<ProcessingModule> processingModules = null)
    {
        this.servicehost = servicehost;
        this.serviceport = serviceport;
        this.responseport = responseport;
        this.topicdatahost = topicdatahost;
        this.topicdataport = topicdataport;
        this.clientID = clientID;
        this.processingModules = processingModules;
        if(processingModules == null)
        {
            this.processingModules = new List<ProcessingModule>();
        }

        inputTopicDatas = new List<TopicData>();
        outputTopicDatas = new List<TopicData>();
        gatheredOutputs = new Dictionary<string, TopicDataRecordList>();

        Initialize();

        int tries = 100;
        while (!connected && tries > 0)
        {
            Initialize();
            tries--;
        }
        if (connected)
        {
            // execute oncreated functions
            foreach(ProcessingModule pm in this.processingModules)
            {
                AddProcessingModule(pm, false);
            }
            tasks = new Task[processingModules.Count];
            // register processingClient
            ServiceRequest registerClient = new ServiceRequest();
            CallService(registerClient);
        }
        else
        {
            Debug.Log("Failed to connect to the server. Try again!");
        }
    }

    private void StartSockets()
    {
        AsyncIO.ForceDotNet.Force();
        serviceSocket = new RequestSocket();
        responseSocket = new ResponseSocket();

        try
        {
            serviceSocket.Connect("ipc://" + servicehost + ":" + serviceport);
            Debug.Log("Create servicesocket successful. Host: " + servicehost + ":" + serviceport);
            connected = true;
        }catch(Exception ex)
        {
            Debug.LogError("ProcessingClient, StartSocket(), Exception occured: " + ex.ToString());
        }
        try
        {
            responseSocket.Connect("ipc://*:" + responseport);
            Debug.Log("Create response socket successful. Port:" + responseport);
            connected = true;
        }
        catch(Exception ex)
        {
            connected = false;
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
            responsePoller = new NetMQPoller();
            poller = new NetMQPoller();
            topicdataSocket = new DealerSocket();
            topicdataSocket.Options.Identity = Encoding.UTF8.GetBytes(clientID); // socket needs clientID for Dealer-Router
            topicdataSocket.ReceiveReady += OnTopicDataMessage; // event on receive topicdata

            StartSockets();

            responseSocket.Options.Identity = Encoding.UTF8.GetBytes(clientID);
            responseSocket.ReceiveReady += OnRequests; // event on receive data

            if (connected)
            {
                responsePoller.Add(responseSocket);
                responsePoller.RunAsync();
                poller.Add(topicdataSocket);
                poller.RunAsync();

                while (running)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.Log("Cancelling task");
                        break;
                    }
                }
            }
            else
            {
                running = false;
            }
        }, cancellationToken);
    }

    public bool IsConnected()
    {
        return connected;
    }

    // @param register if true: means last processingModule has been added -> register processingClient at server
    public void AddProcessingModule(ProcessingModule pm, bool register = true)
    {
        this.processingModules.Add(pm);

        registered = false;
        // call oncreated function
        if (pm.OnCreated != "")
        {
            string[] function = Regex.Split(pm.OnCreated, ".");
            executeFunction(function[0], function[1]);
        }

        pm.Status = ProcessingModuleStatus.Created;

        // register processingClient with processingModules
        if (register)
        {
            ServiceRequest registerClient = new ServiceRequest();
            CallService(registerClient);
            Array.Resize<Task>(ref tasks, processingModules.Count);
        }

        switch (pm.Mode)
        {
            case ProcessingMode.Lockstep:
                if (!lockstepMasterRunning)
                {
                    LockstepMaster();
                }
                break;
            case ProcessingMode.Atfrequency:
                if (!atFrequencyMasterRunning)
                {
                    AtFrequencyMaster();
                }
                break;
            case ProcessingMode.Atnewtopicdata:
                if (!atNewTopicDataMasterRunning)
                {
                    AtNewTopicDataMaster();
                }
                break;
        }
    }

    //TODO make those multi threaded
    async private void LockstepMaster()
    {
        lockstepMasterRunning = true;
        bool tasksCreated = false;
        while (connected)
        {
            if (!startProcessing && registered && !tasksCreated)
            {
                // let all pms of mode lockstep process
                int index = 0;
                foreach(ProcessingModule pm in processingModules)
                {
                    if(pm.Mode == ProcessingMode.Lockstep)
                    {
                        CreateOnProcessingTask(index);
                    }
                    ++index;
                }
                tasksCreated = true;
            }

            if (startProcessing)
            {
                Parallel.ForEach<Task>(tasks, (t) => { t.Start(); });
                Task.WaitAll(tasks);

                startProcessing = false;
                foreach (ProcessingModule pm in processingModules)
                {
                    if (pm.Mode == ProcessingMode.Lockstep)
                    {
                        if(pm.Status != ProcessingModuleStatus.End)
                        {
                            pm.Status = ProcessingModuleStatus.Halted;
                        }
                    }
                }
                //TODO sort through / order outputs...

                //TODO fill in service request to send output TopicDataRecordList
                Ubii.Services.ServiceReply newProcessingModule = await CallService(new Ubii.Services.ServiceRequest
                {
                    Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_REGISTRATION,
                    Client = new Client { Name = name, },
                });
            }
        }
        lockstepMasterRunning = false;
    }

    private void CreateOnProcessingTask(int index)
    {
        // creates task to be executed in the future
        Task onProcess = new Task(() =>
        {
            // process
            if (processingModules[index].OnProcessing != "")
            {
                processingModules[index].Status = ProcessingModuleStatus.Processing;
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
        tasks.Add(onProcess);
    }

    private void AtFrequencyMaster()
    {
        Task processStep = Task.Factory.StartNew(async () =>
        {
            atFrequencyMasterRunning = true;
            while (connected)
            {

            }
            atFrequencyMasterRunning = false;
        });
    }

    async private void AtFrequencyMode(int index)
    {
        Task processStep = Task.Factory.StartNew(async () =>
        {

        });
    }

    async private void AtNewTopicDataMaster()
    {
        Task processStep = Task.Factory.StartNew(async () =>
        {
            atNewTopicDataMasterRunning = true;
            while (connected)
            {

            }
            atNewTopicDataMasterRunning = false;
        });
    }

    async private void AtNewTopicDataMode(int index)
    {
        Task processStep = Task.Factory.StartNew(async () =>
        {

        });
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
    void OnTopicDataMessage(object sender, NetMQSocketEventArgs e)
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

    // receives requests like start processing
    void OnRequests(object sender, NetMQSocketEventArgs e)
    {
        //e.Socket.ReceiveFrameBytes(out bool hasmore);
        startProcessing = true;

    }

    public void SendTopicData(TopicData td)
    {
        byte[] buffer = td.ToByteArray();
        topicdataSocket.SendFrame(buffer);
    }

    public void TearDown()
    {
        serviceSocket.Close();
        responseSocket.Close();
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
