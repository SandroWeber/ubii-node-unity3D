using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ubii.Devices;
using Ubii.TopicData;
using UnityEngine;

public class TestObjectMovement : MonoBehaviour
{
    private UbiiClient ubiiClient = null;
    private string deviceName = "TestObjectMovement - Device";
    private string topicTestPublishSubscribe = null;
    private Device ubiiDevice = null;

    //private CancellationTokenSource cts = null;
    private bool testRunning = false;
    private float tLastPublish = 0f;

    private Vector3 testPosition = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        ubiiClient = FindObjectOfType<UbiiClient>();
        StartTest();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = testPosition;

        float tNow = Time.time;
        if (testRunning && tNow > tLastPublish + 1)
        {
            Vector3 randomPosition = UnityEngine.Random.insideUnitSphere;
            ubiiClient.Publish(
                new Ubii.TopicData.TopicData
                {
                    TopicDataRecord = new Ubii.TopicData.TopicDataRecord
                    {
                        Topic = topicTestPublishSubscribe,
                        Vector3 = new Ubii.DataStructure.Vector3 { X = randomPosition.x, Y = randomPosition.y, Z = randomPosition.z }
                    }
                });
            tLastPublish = tNow;
        }
    }

    private void OnDisable()
    {
        testRunning = false;
    }

    async private void StartTest()
    {
        if (ubiiClient == null)
        {
            Debug.LogError("UbiiClient not found!");
            return;
        }

        await ubiiClient.WaitForConnection();

        CreateUbiiSpecs();

        Ubii.Services.ServiceReply deviceRegistrationReply = await ubiiClient.RegisterDevice(ubiiDevice);
        if (deviceRegistrationReply.Device != null)
        {
            ubiiDevice = deviceRegistrationReply.Device;
        }
        else
        {
            Debug.LogWarning("Ubii device could not be registered");
        }

        await ubiiClient.Subscribe(new List<string>() { topicTestPublishSubscribe }, new List<Action<TopicDataRecord>>() { (TopicDataRecord record) =>
        {
            testPosition.Set((float)record.Vector3.X, (float)record.Vector3.Y, (float)record.Vector3.Z);
        } 
        });

        testRunning = true;
    }

    private void CreateUbiiSpecs()
    {
        topicTestPublishSubscribe = "/" + ubiiClient.GetID() + "/test_publish_subscribe/object_movement";

        ubiiDevice = new Ubii.Devices.Device { Name = deviceName, ClientId = ubiiClient.GetID(), DeviceType = Ubii.Devices.Device.Types.DeviceType.Participant };
        ubiiDevice.Components.Add(new Ubii.Devices.Component { IoType = Ubii.Devices.Component.Types.IOType.Publisher, MessageFormat = "ubii.dataStructure.Vector3", Topic = topicTestPublishSubscribe });
    }
}
