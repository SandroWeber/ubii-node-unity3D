using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using pbc = global::Google.Protobuf.Collections;

public class TestTensorflowCommunication : MonoBehaviour
{

    private UbiiClient ubiiClient = null;
    private string deviceName = "TestTensorflowCommunication - Device";
    private string interactionID = "02bc02ea-8228-43cf-8d80-88f8923e912a";
    private string topicTestSubscribe = null;
    private string topicTestPublish = null;
    private Ubii.Devices.Device ubiiDevice = null;
    private Ubii.Sessions.Session ubiiSession = null;
    private Ubii.Interactions.Interaction ubiiInteraction = null;
    private Ubii.Sessions.IOMapping IOMapping = null;
    private Ubii.DataStructure.Object2DList predictions = null;
    Ubii.Sessions.InteractionInputMapping inputMapping = null;
    Ubii.Sessions.InteractionOutputMapping outputMapping = null;
    double x = 0.0f;

    //debugging
    Ubii.Services.ServiceReply subscriptionReply = null;

    //private CancellationTokenSource cts = null;
    private bool testRunning = false;
    private float tLastPublish = 0f;

    private Vector3 outPublish = new Vector3(1, 1, 1);
    private Vector3 testPosition;
    private Ubii.DataStructure.Object2DList publishList;

    // Start is called before the first frame update
    void Start()
    {
        ubiiClient = FindObjectOfType<UbiiClient>();
        StartTest();
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
                    Topic = topicTestPublish,
                    Double =  4.0
                }
            };
            ubiiClient.Publish(publishdata);
            if(predictions != null)
                Debug.Log(predictions);
            else if( testPosition != null )
                Debug.Log(testPosition);
            else
                Debug.Log(x);
            tLastPublish = tNow;
        }
    }

    async private void OnDisable()
    {
        testRunning = false;

        if (ubiiDevice != null)
        {

            await ubiiClient.CallService(new Ubii.Services.ServiceRequest
            {
                Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_STOP,
                Session = ubiiSession
            });

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
            /*
            await ubiiClient.CallService(new Ubii.Services.ServiceRequest
            {
                Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_STOP,
                Session = ubiiSession
            });*/
        }
    }

    async private void StartTest()
    {
        if (ubiiClient == null)
        {
            Debug.LogError("UbiiClient not found!");
            return;
        }

        await ubiiClient.WaitForConnection();

        await Task.Delay(1000);

        CreateUbiiDeviceSpecs();

        Ubii.Services.ServiceReply deviceRegistrationReply = await ubiiClient.CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.DEVICE_REGISTRATION,
            Device = ubiiDevice
        });
        if (deviceRegistrationReply.Device != null)
        {
            ubiiDevice = deviceRegistrationReply.Device;
        }

        /*this.ubiiInteraction = new Ubii.Interactions.Interaction
        {
            Id = "Unity3D-Client-TestSessions-Interaction-02",
            Name = "Unity3D-Client-TestSessions-Interaction-02",
            ProcessingCallback = "(inputs, outputs, state) => { console.log(inputs.inVec3);",
            ProcessFrequency = 10
        };
        this.ubiiInteraction.InputFormats.Add(new Ubii.Interactions.IOFormat { InternalName = "inVec3", MessageFormat = "vector3" });
        this.ubiiInteraction.OutputFormats.Add(new Ubii.Interactions.IOFormat { InternalName = "outVec3", MessageFormat = "vector3" });
        this.ubiiInteraction.Authors.Add("Lukas Goll");
        */

        Ubii.Services.ServiceReply interactionReply = await ubiiClient.CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.INTERACTION_DATABASE_GET,
            Interaction = new Ubii.Interactions.Interaction { Id = interactionID}
        });
        if (interactionReply.Interaction != null)
        {
            ubiiInteraction = interactionReply.Interaction;
        }

        CreateUbiiSession();
        Ubii.Services.ServiceReply sessionRequest = await ubiiClient.CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_START,
            Session = ubiiSession
        });
        if (sessionRequest.Session != null)
        {
            ubiiSession = sessionRequest.Session;
        }

        Ubii.Services.ServiceReply subRequest = await ubiiClient.Subscribe(topicTestSubscribe, (Ubii.TopicData.TopicDataRecord record) =>
        {
            //testPosition = new Vector3((float)record.Vector3.X, (float)record.Vector3.Y, (float)record.Vector3.Z);
            //predictions = record.Object2DList;
            x = record.Double;
        });
        testRunning = true;
    }

    private void CreateUbiiDeviceSpecs()
    {
        topicTestSubscribe = "/" + ubiiClient.GetID() + "/test_tensorflow/test_subscribe";
        topicTestPublish = "/" + ubiiClient.GetID() + "/test_tensorflow/test_publish";

        ubiiDevice = new Ubii.Devices.Device { Name = deviceName, ClientId = ubiiClient.GetID(), DeviceType = Ubii.Devices.Device.Types.DeviceType.Participant };
        ubiiDevice.Components.Add(new Ubii.Devices.Component { IoType = Ubii.Devices.Component.Types.IOType.Input, MessageFormat = "double", Topic = topicTestPublish });
        ubiiDevice.Components.Add(new Ubii.Devices.Component { IoType = Ubii.Devices.Component.Types.IOType.Output, MessageFormat = "double", Topic = topicTestSubscribe });
       
    }

    public void CreateUbiiSession()
    {
        ubiiSession = new Ubii.Sessions.Session
        {
            Id = "Session_Test_Unity",
            Name = "TestTensorflowConnection - Session IO",
            //Interactions = other.interactions_.Clone();
            //ioMappings_ = other.ioMappings_.Clone();
            //Tags = "Test";
            Description = "Testing Session",
            ProcessMode = Ubii.Sessions.ProcessMode.IndividualProcessFrequencies
        };
        ubiiSession.Interactions.Add(ubiiInteraction);

        inputMapping = new Ubii.Sessions.InteractionInputMapping { Name = "defaultIn", Topic = topicTestPublish };
        outputMapping = new Ubii.Sessions.InteractionOutputMapping { Name = "defaultOut", Topic = topicTestSubscribe };
        IOMapping = new Ubii.Sessions.IOMapping { InteractionId = this.ubiiInteraction.Id };
        IOMapping.InputMappings.Add(inputMapping);
        IOMapping.OutputMappings.Add(outputMapping);
        ubiiSession.IoMappings.Add(IOMapping);
    }
}
