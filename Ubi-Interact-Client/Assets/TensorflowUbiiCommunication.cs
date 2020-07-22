using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using pbc = global::Google.Protobuf.Collections;

struct TensorflowTopic
{
    public string Topic;
    public string MessageFormat ;
    public string Name;
    public Ubii.DataStructure.FloatList Data;
}

public class TensorflowUbiiCommunication : MonoBehaviour
{

    //Ubii Components
    private UbiiClient ubiiClient = null;
    private string deviceName = "TensorflowCommunication - Unity";
    private string interactionID = "b36585a9-a57c-49c1-b29f-e95fe8c6bbbf";
    private Ubii.Devices.Device ubiiDevice = null;
    private Ubii.Sessions.Session ubiiSession = null;
    private Ubii.Interactions.Interaction ubiiInteraction = null;
    private Ubii.Sessions.IOMapping IOMapping = null;
    Ubii.Sessions.InteractionInputMapping inputMapping = null;
    Ubii.Sessions.InteractionOutputMapping outputMapping = null;

    
    //private CancellationTokenSource cts = null;
    private bool testRunning = false;
    private float tLastPublish = 0f;

    TensorflowTopic input;
    TensorflowTopic output;
    List<float> testOut = new List<float>{ 0.0f, 0.0f, 0.0f };  
    List<float> testIn = new List<float> { 0.2111111f, -0.5941272f, -0.02127039f, -0.009392619f, -32.13583f, -25.74573f, -9.315887f, -0.5940099f, -0.02326876f, -0.02047408f, -27.27731f, -33.01651f, -13.89691f, -0.5944558f, -0.08851916f, -0.0745008f, -45.39383f, -34.15411f, -34.04117f, -0.5941317f, -0.1756018f, -0.1187285f, -8.12439f, -49.96463f, -43.32138f, -0.5942634f, -0.03597379f, -0.01802421f, -39.73663f, -98.68097f, -97.82831f, -0.5939654f, -0.0887953f, -0.04463649f, -72.38086f, -101.7171f, -101.7127f, -0.594137f, -0.1320444f, -0.08935773f, -24.98944f, -80.59061f, -78.0748f, -0.5941272f, -0.01107651f, -0.005248189f, -42.94934f, -22.64832f, -5.541687f, -0.5941272f, -0.008579373f, -0.01396692f, -43.64032f, -52.40503f, -53.98773f, -0.5941271f, -0.01413572f, -0.04548872f, -32.44717f, -77.29446f, -68.58716f, -0.5941272f, -0.03763592f, -0.06198895f, -35.95529f, -120.34f, -156.184f, -0.594127f, -0.0663178f, -0.0740484f, -38.19247f, -177.4093f, -178.25f, -0.5941272f, -0.01061165f, -0.04593337f, -36.39246f, -20.02057f, -38.91437f, -0.5941271f, -0.01548338f, -0.02296245f, -7.75592f, -32.73868f, -32.04333f, -0.5941272f, -0.03691864f, -0.03150356f, -19.26733f, -169.3474f, -112.7787f, -0.5941274f, -0.06895018f, -0.03721964f, -16.89462f, -138.0689f, -179.6915f};

    // Start is called before the first frame update
    void Start()
    {
        ubiiClient = FindObjectOfType<UbiiClient>();
        if(ubiiClient == null)
        {
            Debug.Log("Ubii Client missing!");
        }
        else
        {
            StartSetup();
        }

    }

    // Update is called once per frame
    void Update()
    {
 
        float tNow = Time.time;
        if (testRunning && tNow > tLastPublish + 1)
        {
            Ubii.TopicData.TopicData publishdata = new Ubii.TopicData.TopicData
            {
                TopicDataRecord = new Ubii.TopicData.TopicDataRecord
                {
                    Topic = input.Topic,
                    FloatList = input.Data
                }
            };
            ubiiClient.Publish(publishdata);
            tLastPublish = tNow;
        }
    }

    async private void OnDisable()
    {
        testRunning = false;

        if (ubiiSession != null)
        {

            await ubiiClient.CallService(new Ubii.Services.ServiceRequest
            {
                Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_RUNTIME_STOP,
                Session = ubiiSession
            });
        }
        if (ubiiDevice != null)
        {
                await ubiiClient.CallService(new Ubii.Services.ServiceRequest
            {
                Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.DEVICE_DEREGISTRATION,
                Device = ubiiDevice
            });
        }
    }

    async private void OnApplicationQuit()
    {
        if (ubiiSession != null)
        {
            
            await ubiiClient.CallService(new Ubii.Services.ServiceRequest
            {
                Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_RUNTIME_STOP,
                Session = ubiiSession
            });
        }
        if (ubiiDevice != null)
        {
            await ubiiClient.CallService(new Ubii.Services.ServiceRequest
            {
                Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.DEVICE_DEREGISTRATION,
                Device = ubiiDevice
            });
        }
    }

    async private void StartSetup()
    {
        if (ubiiClient == null)
        {
            Debug.LogError("UbiiClient not found!");
            return;
        }

        await ubiiClient.WaitForConnection();

        await Task.Delay(1000);

        CreateTensorflowTopics();
        CreateDeviceSpecs();

        Ubii.Services.ServiceReply deviceRegistrationReply = await ubiiClient.CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.DEVICE_REGISTRATION,
            Device = ubiiDevice
        });
        if (deviceRegistrationReply.Device != null)
        {
            ubiiDevice = deviceRegistrationReply.Device;
        }

        Ubii.Services.ServiceReply interactionReply = await ubiiClient.CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.INTERACTION_DATABASE_GET,
            Interaction = new Ubii.Interactions.Interaction { Id = interactionID }
        });
        if (interactionReply.Interaction != null)
        {
            ubiiInteraction = interactionReply.Interaction;
        }

        MakeUbiiSession();

        Ubii.Services.ServiceReply sessionRequest = await ubiiClient.CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_RUNTIME_START,
            Session = ubiiSession
        });
        if (sessionRequest.Session != null)
        {
            ubiiSession = sessionRequest.Session;
        }

        await ubiiClient.Subscribe(output.Topic, (Ubii.TopicData.TopicDataRecord record) =>
        { 
            Debug.Log(record.Topic);
            output.Data = record.FloatList;
            Debug.Log(output.Data);
        });
        testRunning = true;

        
    }

    private void CreateTensorflowTopics()
    {
        input.Name = "TensorflowInput";
        input.Topic = "/" + ubiiClient.GetID() + "/test_tensorflow/test_publish";
        input.MessageFormat = "ubii.datastructure.FloatList";
        input.Data = new Ubii.DataStructure.FloatList{ };
        input.Data.Elements.AddRange(testIn);

        output.Name = "TensorflowOutput";
        output.Topic = " / " + ubiiClient.GetID() + " / test_tensorflow / test_subscript";
        output.MessageFormat = "ubii.datastructure.FloatList";
        output.Data = new Ubii.DataStructure.FloatList { };
    }

    async private void CreateDeviceSpecs()
    {   
        ubiiDevice = new Ubii.Devices.Device { Name = deviceName, ClientId = ubiiClient.GetID(), DeviceType = Ubii.Devices.Device.Types.DeviceType.Participant };
        ubiiDevice.Components.Add(new Ubii.Devices.Component { IoType = Ubii.Devices.Component.Types.IOType.Publisher, MessageFormat = input.MessageFormat, Topic = input.Topic });
        ubiiDevice.Components.Add(new Ubii.Devices.Component { IoType = Ubii.Devices.Component.Types.IOType.Subscriber, MessageFormat = output.MessageFormat, Topic = output.Topic });
    }

    private void MakeUbiiSession()
    {
        ubiiSession = new Ubii.Sessions.Session
        {
            Name = "TestTensorflowConnection - Session IO",
            //Interactions = other.interactions_.Clone();
            //ioMappings_ = other.ioMappings_.Clone();
            //Tags = "Test";
            Description = "Testing Session",
            ProcessMode = Ubii.Sessions.ProcessMode.IndividualProcessFrequencies
        };
        ubiiSession.Interactions.Add(ubiiInteraction);

        inputMapping = new Ubii.Sessions.InteractionInputMapping { Name = input.Name, Topic = input.Topic };
        outputMapping = new Ubii.Sessions.InteractionOutputMapping { Name = output.Name, Topic = output.Topic };
        IOMapping = new Ubii.Sessions.IOMapping { InteractionId = this.ubiiInteraction.Id };
        IOMapping.InputMappings.Add(inputMapping);
        IOMapping.OutputMappings.Add(outputMapping);
        ubiiSession.IoMappings.Add(IOMapping);
    }
}
