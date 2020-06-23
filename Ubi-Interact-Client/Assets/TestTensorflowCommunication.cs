using System.Collections;
using System.Collections.Generic;
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
    Ubii.Sessions.InteractionInputMapping inputMapping = null;
    Ubii.Sessions.InteractionOutputMapping outputMapping = null;

    //debugging
    Ubii.Services.ServiceReply subscriptionReply = null;

    //private CancellationTokenSource cts = null;
    private bool testRunning = false;
    private float tLastPublish = 0f;

    private Vector3 outPublish = new Vector3(1,1,1);
    public Vector3 testPosition = new Vector3(0,0,0);

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
            ubiiClient.Publish(
                new Ubii.TopicData.TopicData
                {
                    TopicDataRecord = new Ubii.TopicData.TopicDataRecord
                    {
                        Topic = ubiiDevice.Components[0].Topic,
                        Vector3 = new Ubii.DataStructure.Vector3 { X = outPublish.x, Y = outPublish.y , Z = outPublish.z }
                    }
                });
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

    async private void StartTest()
    {
        if (ubiiClient == null)
        {
            Debug.LogError("UbiiClient not found!");
            return;
        }

        await ubiiClient.WaitForConnection();

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

        subscriptionReply = await ubiiClient.Subscribe(ubiiDevice.Components[1].Topic, (Ubii.TopicData.TopicDataRecord record) =>
        {
            testPosition.Set((float)record.Vector3.X, (float)record.Vector3.Y, (float)record.Vector3.Z);
        });
        testRunning = true;
    }

    private void CreateUbiiDeviceSpecs()
    {
        topicTestSubscribe = "/" + ubiiClient.GetID() + "/test_tensorflow/test_subscribe";
        topicTestPublish = "/" + ubiiClient.GetID() + "/test_tensorflow/test_publish";

        ubiiDevice = new Ubii.Devices.Device { Name = deviceName, ClientId = ubiiClient.GetID(), DeviceType = Ubii.Devices.Device.Types.DeviceType.Participant };
        ubiiDevice.Components.Add(new Ubii.Devices.Component { IoType = Ubii.Devices.Component.Types.IOType.Input, MessageFormat = "ubii.dataStructure.Vector3", Topic = topicTestPublish });
        ubiiDevice.Components.Add(new Ubii.Devices.Component { IoType = Ubii.Devices.Component.Types.IOType.Output, MessageFormat = "ubii.dataStructure.Vector3", Topic = topicTestSubscribe });
       
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
            //Authors = "Lukas Goll",
            ProcessMode = Ubii.Sessions.ProcessMode.IndividualProcessFrequencies
        };
        ubiiSession.Interactions.Add(ubiiInteraction);

        inputMapping = new Ubii.Sessions.InteractionInputMapping { Name = ubiiInteraction.InputFormats[0].InternalName, Topic = ubiiDevice.Components[0].Topic };
        outputMapping = new Ubii.Sessions.InteractionOutputMapping { Name = ubiiInteraction.OutputFormats[0].InternalName, Topic = ubiiDevice.Components[1].Topic };
        IOMapping = new Ubii.Sessions.IOMapping { InteractionId = interactionID };
        IOMapping.InputMappings.Add(inputMapping);
        IOMapping.OutputMappings.Add(outputMapping);
        ubiiSession.IoMappings.Add(IOMapping);
    }
}
