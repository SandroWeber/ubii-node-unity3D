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
    private string interactionID = "02bc02ea-8228-43cf-8d80-88f8923e912a";
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
    List<float> testIn = new List<float> { 1.0f, 2.0f, 3.0f };

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
        output.Topic = "topicSubscribe";
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
        Debug.Log(output.Topic);
        Debug.Log(outputMapping);
        IOMapping = new Ubii.Sessions.IOMapping { InteractionId = this.ubiiInteraction.Id };
        IOMapping.InputMappings.Add(inputMapping);
        IOMapping.OutputMappings.Add(outputMapping);
        ubiiSession.IoMappings.Add(IOMapping);
    }
}
