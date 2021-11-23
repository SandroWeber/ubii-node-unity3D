using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TestObjectMovement : MonoBehaviour
{
    [SerializeField] private int testStartDelaySeconds = 5;
    [SerializeField] private GameObject targetObject = null;

    private UbiiNode ubiiNode = null;
    private string deviceName = "TestObjectMovement - Device";
    private string topicTestPublishSubscribe = null;
    private Ubii.Devices.Device ubiiDevice = null;

    //private CancellationTokenSource cts = null;
    private bool testRunning = false;
    private float tLastPublish = 0f;
    private SubscriptionToken subToken;

    private Vector3 testPosition = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        ubiiNode = FindObjectOfType<UbiiNode>();
    }

    // Update is called once per frame
    void Update()
    {
        targetObject.transform.position = testPosition;

        float tNow = Time.time;
        if (testRunning && tNow > tLastPublish + 1)
        {
            Vector3 randomPosition = Random.insideUnitSphere;
            ubiiNode.Publish(new Ubii.TopicData.TopicDataRecord
            {
                Topic = topicTestPublishSubscribe,
                Vector3 = new Ubii.DataStructure.Vector3 { X = randomPosition.x, Y = randomPosition.y, Z = randomPosition.z }
            });
            tLastPublish = tNow;
        }
    }

    void OnEnable()
    {
        UbiiNode.OnInitialized += OnClientInitialized;
    }

    private async void OnDisable()
    {
        testRunning = false;
        UbiiNode.OnInitialized -= OnClientInitialized;
        await ubiiNode.Unsubscribe(this.subToken);
    }

    public void OnClientInitialized()
    {
        Invoke("StartTest", testStartDelaySeconds); //StartTest();
        return;
    }

    private async void StartTest()
    {
        if (ubiiNode == null)
        {
            Debug.LogError("UbiiClient not found!");
            return;
        }

        await ubiiNode.WaitForConnection();

        CreateUbiiSpecs();

        Ubii.Services.ServiceReply deviceRegistrationReply = await ubiiNode.RegisterDevice(ubiiDevice);
        if (deviceRegistrationReply.Device != null)
        {
            ubiiDevice = deviceRegistrationReply.Device;
        }

        this.subToken = await ubiiNode.SubscribeTopic(topicTestPublishSubscribe,
            (Ubii.TopicData.TopicDataRecord record) =>
            {
                testPosition.Set((float)record.Vector3.X, (float)record.Vector3.Y, (float)record.Vector3.Z);
            });

        testRunning = true;
    }

    private void CreateUbiiSpecs()
    {
        topicTestPublishSubscribe = "/" + ubiiNode.GetID() + "/test_publish_subscribe/object_movement";

        ubiiDevice = new Ubii.Devices.Device { Name = deviceName, ClientId = ubiiNode.GetID(), DeviceType = Ubii.Devices.Device.Types.DeviceType.Participant };
        ubiiDevice.Components.Add(new Ubii.Devices.Component
        {
            Name = "Test Object Movement - random Vector3",
            IoType = Ubii.Devices.Component.Types.IOType.Publisher,
            MessageFormat = "ubii.dataStructure.Vector3",
            Topic = topicTestPublishSubscribe
        });
        Debug.Log("ubiiDevice = " + ubiiDevice);
    }
}