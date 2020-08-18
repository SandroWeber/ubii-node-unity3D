using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ubii.TopicData;
using Ubii.Clients;
using Ubii.Devices;
using Ubii.Processing;
using Ubii.Services;
using Ubii.Servers;
using Ubii.Services.Request;
using Google.Protobuf.Collections;
using UnityEngine;
using NetMQ;
using Google.Protobuf;
using System.Threading;
using System.ComponentModel.Design;
using NetMQ.Sockets;
using System.Text.RegularExpressions;
using System.Reflection;

public class NetMQUbiiClient
{
    private string id;
    private string name;
    private string host;
    private int port;
    private bool hasProcessing;
    private List<ProcessingModule> processingModules;
    private int responseport;

    private bool hasLockstep;
    private ResponseSocket responseSocket;
    private NetMQPoller responsePoller;
    private CancellationTokenSource cts = new CancellationTokenSource();
    private Task[] lockstepProcessingTasks;
    private TopicDataRecordList currentLockstepTD;
    private TopicDataRecordList gatheredLockstepOutputs;

    private Client clientSpecification;

    private NetMQServiceClient netmqServiceClient;

    private NetMQTopicDataClient netmqTopicDataClient;

    private List<Device> deviceSpecifications = new List<Device>();

    private Server serverSpecification;

    public NetMQUbiiClient(string id, string name, string host, int port, bool hasProcessing, List<ProcessingModule> processingModules = null, int responseport=0)
    {
        this.id = id;
        this.name = name;
        this.host = host;
        this.port = port;
        this.hasProcessing = hasProcessing;
        AddProcessingModules(processingModules);
        this.responseport = responseport;
    }

    public string GetClientID()
    {
        return clientSpecification.Id;
    }

    // Initialize the ubiiClient, serviceClient and topicDataClient
    public async Task Initialize()
    {
        netmqServiceClient = new NetMQServiceClient(host, port);
        await InitServerSpec();
        Debug.Log("ServerSpecs: " + serverSpecification);
        await InitClientReg();
        InitTopicDataClient();
        //await InitTestDe<vices(); // Initialize some test devices for demonstration purposes, thus commented out
        // 
        if (hasProcessing)
        {
            HasLockstep();
        }
    }

    public bool IsConnected()
    {
        /*Debug.Log("IsConnected()");
        Debug.Log("id=" + id);
        Debug.Log("client=" + netmqTopicDataClient);
        if (netmqTopicDataClient != null)
        {
            Debug.Log("connected=" + netmqTopicDataClient.IsConnected());
        }*/
        return (clientSpecification != null && clientSpecification.Id != null && netmqTopicDataClient != null && netmqTopicDataClient.IsConnected());
    }

    public void AddProcessingModules(List<ProcessingModule> processingModules)
    {
        foreach(ProcessingModule pm in processingModules)
        {
            if (pm.OnCreated != "")
            {
                string[] function = Regex.Split(pm.OnCreated, ".");
                executeFunction(function[0], function[1]);
            }

            pm.Status = ProcessingModule.Types.Status.Created;
        }

        this.processingModules.AddRange(processingModules);
        HasLockstep();
        Array.Resize<Task>(ref lockstepProcessingTasks, processingModules.Count);
    }

    public void RemoveProcessingModules(ProcessingModule pm)
    {
        if (pm.OnDestroyed != "")
        {
            string[] function = Regex.Split(pm.OnDestroyed, ".");
            executeFunction(function[0], function[1]);
        }
        pm.Status = ProcessingModule.Types.Status.Destroyed;
        processingModules.Remove(pm);
        HasLockstep();
    }

    /*public Task WaitForConnection()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;
        return Task.Run(() =>
        {
            int maxRetries = 100;
            int currentTry = 1;
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
    }*/

    // CallService function called from upper layer (i.e. some MonoBehavior), returns a Task
    public Task<ServiceReply> CallService(ServiceRequest srq)
    {
        //Debug.Log("CallService: " + srq.Topic);
        return Task.Run(() => netmqServiceClient.CallService(srq));
    }


    public void Publish(TopicData topicData)
    {
        netmqTopicDataClient.SendTopicData(topicData);
    }

    public async Task<ServiceReply> Subscribe(string topic, Action<TopicDataRecord> function)
    {
        // Repeated fields cannot be instantiated in SerivceRequest creation
        RepeatedField<string> subscribeTopics = new RepeatedField<string>();
        subscribeTopics.Add(topic);
        //Debug.Log("Subscribing to topic: " + topic + " (backend), subscribeRepeatedField: " + subscribeTopics.Count);
        ServiceRequest topicSubscription = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription
            {
                ClientId = clientSpecification.Id,
                SubscribeTopics = { subscribeTopics }
            }
        };

        var task = CallService(topicSubscription);
        ServiceReply subReply = await task;

        if (subReply.Error != null)
        {
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            return null;
        }

        // adding callback function to dictionary
        netmqTopicDataClient.AddTopicDataCallback(topic, function);

        return subReply;
    }

    public async Task<ServiceReply> SubscribeRegex(string regex, Action<TopicDataRecord> function)
    {
        //Debug.Log("Subscribing to topic: " + topic + " (backend), subscribeRepeatedField: " + subscribeTopics.Count);
        ServiceRequest topicSubscription = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription
            {
                ClientId = clientSpecification.Id,
                SubscribeTopicRegexp = regex
            }
        };

        ServiceReply subReply = await CallService(topicSubscription);

        if (subReply.Error != null)
        {
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            return null;
        }

        // adding callback function to dictionary
        netmqTopicDataClient.AddTopicDataRegexCallback(regex, function);

        return subReply;
    }

    public async Task<ServiceReply> Unsubscribe(string topic)
    {
        // Repeated fields cannot be instantiated in SerivceRequest creation; adding topic to unsubscribeTopics which is later sent to server with clientID
        RepeatedField<string> unsubscribeTopics = new RepeatedField<string>();
        unsubscribeTopics.Add(topic);

        ServiceRequest topicUnsubscription = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.TOPIC_SUBSCRIPTION,
            TopicSubscription = new TopicSubscription
            {
                ClientId = clientSpecification.Id,
                UnsubscribeTopics = { unsubscribeTopics }
            }
        };

        var task = CallService(topicUnsubscription);
        ServiceReply subReply = await task;

        if (subReply.Error != null)
        {
            Debug.LogError("subReply Error! Error msg: " + subReply.Error.ToString());
            return null;
        }

        // removing callback function from dictionary
        netmqTopicDataClient.RemoveTopicDataCallback(topic);

        return subReply;
    }

    #region Initialize Functions
    private async Task InitServerSpec()
    {
        // Call Service to receive serverSpecifications
        ServiceRequest serverConfigRequest = new ServiceRequest { Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SERVER_CONFIG };

        var task = CallService(serverConfigRequest);
        ServiceReply rep = await task;
        serverSpecification = rep.Server;
    }

    private async Task InitClientReg()
    {
        // Client Registration
        //TODO add processing info
        ServiceRequest clientRegistration = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_REGISTRATION,
            Client = new Client { Name = name, },
        };

        if (id != null && id != "")
            clientRegistration.Client.Id = id;

        var task = CallService(clientRegistration);
        ServiceReply rep = await task;
        clientSpecification = rep.Client;
        Debug.Log("ClientSpec: " + clientSpecification);
    }

    private void InitTopicDataClient()
    {
        netmqTopicDataClient = new NetMQTopicDataClient(clientSpecification.Id, host, int.Parse(serverSpecification.PortTopicDataZmq));
    }

    #endregion

    #region Processing

    #region Lockstep

    private async Task InitLockstep()
    {
        bool running = true;
        CancellationToken cancellationToken = cts.Token;
        responsePoller = new NetMQPoller();
        lockstepProcessingTasks = new Task[processingModules.Count];
        currentLockstepTD = new TopicDataRecordList();
        gatheredLockstepOutputs = new TopicDataRecordList();
        await StartResponseSocket();

        Task processIncomingMessages = Task.Factory.StartNew(() =>
        {
            responseSocket.Options.Identity = Encoding.UTF8.GetBytes(id);
            responseSocket.ReceiveReady += OnRequests; // event on receive data

            responsePoller.Add(responseSocket);
            responsePoller.RunAsync();

            while (running)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Cancelling task");
                    break;
                }
            }

        }, cancellationToken);
    }

    private async Task StartResponseSocket()
    {
        AsyncIO.ForceDotNet.Force();
        responseSocket = new ResponseSocket();
        try
        {
            //TODO change to icp/tcp
            responseSocket.Connect("tcp://*:" + responseport);
            Debug.Log("Create Response Socket successful. Host: " + host + ":" + serverSpecification.PortServiceZmq);
        }
        catch (Exception ex)
        {
            Debug.LogError("NetMQUbiiClient, StartResponseSocket(), Exception occured: " + ex.ToString());
        }
    }
    async void OnRequests(object sender, NetMQSocketEventArgs e)
    {
        ServiceRequest request = ServiceRequest.Parser.ParseFrom(e.Socket.ReceiveFrameBytes(out bool hasmore));
        currentLockstepTD = request.LockstepProcessingRequest.Records;

        // process
        await Task.WhenAll(lockstepProcessingTasks);

        // send output
        ServiceReply reply = new ServiceReply
        {
            LockstepProcessingReply = { Records = gatheredLockstepOutputs },
        };
        byte[] buffer = gatheredLockstepOutputs.ToByteArray();
        responseSocket.SendFrame(buffer);

        // execute onHalted
        foreach (ProcessingModule pm in processingModules)
        {
            if(pm.ProcessingMode.ModeCase == ProcessingMode.ModeOneofCase.Lockstep)
            {
                if (pm.OnHalted != "")
                {
                    string[] function = Regex.Split(pm.OnHalted, ".");
                    executeFunction(function[0], function[1]);
                }
                
                pm.Status = ProcessingModule.Types.Status.Halted;
            }
        }
    }

    private void CreateOnProcessingTask(ProcessingModule pm)
    {
        // creates task to be executed in the future
        Task onProcess = new Task(() =>
        {
            // process
            if (pm.OnProcessing != "")
            {
                pm.Status = ProcessingModule.Types.Status.Processing;
                string[] function = Regex.Split(pm.OnProcessing, ".");
                Type type = Type.GetType(function[0]);

                TopicDataRecordList output = (TopicDataRecordList)type.InvokeMember(
                    function[1],
                    BindingFlags.InvokeMethod | BindingFlags.Public |
                    BindingFlags.Static, null, null, 
                    currentLockstepTD.Elements.ToArray<TopicDataRecord>());
                // gather topicdatarecordlist
                gatheredLockstepOutputs.Elements.AddRange(output.Elements);
            }
        });
        lockstepProcessingTasks[Array.IndexOf(lockstepProcessingTasks, null)] = onProcess;
    }

    private void executeFunction(string clss, string name)
    {
        Type type = Type.GetType(clss);

        type.InvokeMember(
            name,
            BindingFlags.InvokeMethod | BindingFlags.Public |
            BindingFlags.Static, null, null, null);
    }

    private bool HasLockstep()
    {
        if (processingModules != null)
        {
            foreach (ProcessingModule pm in processingModules)
            {
                if (pm.ProcessingMode.ModeCase == ProcessingMode.ModeOneofCase.Lockstep)
                {
                    if (!hasLockstep)
                    {
                        hasLockstep = true;
                        InitLockstep();
                        break;
                    }
                    hasLockstep = true;
                    break;
                }
            }
        }
        return hasLockstep;
    }

    #endregion

    #endregion

    #region Test Functions
    // This is only for testing / demonstration purposes on how a device registration works with ubiiClient
    private async Task InitTestDevices()
    {
        // RepeatedFields have to be declared separately and then added to the service request
        RepeatedField<Ubii.Devices.Component> components = new RepeatedField<Ubii.Devices.Component>();
        components.Add(new Ubii.Devices.Component { Topic = "TestBool", MessageFormat = "boolean", IoType = Ubii.Devices.Component.Types.IOType.Publisher });

        // Device Registration
        ServiceRequest deviceRegistration = new ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.DEVICE_REGISTRATION,
            Device = new Device
            {
                Name = "TestDevice1",
                DeviceType = Device.Types.DeviceType.Participant,
                ClientId = clientSpecification.Id,
                Components = { components }
            }
        };

        Debug.Log("DeviceRegistration #1 Components: " + deviceRegistration.ToString());

        var task = CallService(deviceRegistration);
        ServiceReply svr = await task;

        if (svr.Device != null)
        {
            deviceSpecifications.Add(svr.Device);
            Debug.Log("DeviceSpecifications #1: " + deviceSpecifications[0].ToString());
            Debug.Log("DeviceSpecifications #1 Components: " + deviceSpecifications[0].Components.ToString());
        }
        else if (svr.Error != null)
            Debug.Log("SVR Error: " + svr.Error.ToString());
        else
            Debug.Log("An unknown error occured");
    }


    #endregion

    async public void ShutDown()
    {
        await CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_DEREGISTRATION,
            Client = clientSpecification
        });
        cts.Cancel();
        netmqServiceClient.TearDown();
        netmqTopicDataClient.TearDown();
    }
}
