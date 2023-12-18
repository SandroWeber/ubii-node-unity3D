using System.Collections.Generic;
using UnityEngine;

public class TestSuite : MonoBehaviour
{
    public List<UbiiTest> tests = new List<UbiiTest>();

    // Start is called before the first frame update
    async void Start()
    {
        UbiiNode node = GetComponent<UbiiNode>();
        if (node == null)
        {
            Debug.LogError("TestSuite could not find UbiiNode");
            return;
        }

        tests.Add(new TestPubSubTopic(node));
        tests.Add(new TestPubSubRegex(node));
        tests.Add(new TestParallelServiceCalls(node));

        bool allTestsSuccess = true;

        // run tests with ZeroMQ connection
        await node.Initialize(
            UbiiNetworkClient.SERVICE_CONNECTION_MODE.ZEROMQ,
            UbiiNetworkClient.DEFAULT_ADDRESS_SERVICE_ZMQ,
            UbiiNetworkClient.TOPICDATA_CONNECTION_MODE.ZEROMQ,
            UbiiNetworkClient.DEFAULT_ADDRESS_TOPICDATA_ZMQ);

        foreach (UbiiTest test in tests)
        {
            UbiiTestResult result = await test.RunTest();
            Debug.Log(result.ToString());
            if (!result.success) allTestsSuccess = false;
        }

        // run tests with HTTP/WS connection
        await node.Disconnect();
        await node.Initialize(
            UbiiNetworkClient.SERVICE_CONNECTION_MODE.HTTP,
            UbiiNetworkClient.DEFAULT_ADDRESS_SERVICE_HTTP,
            UbiiNetworkClient.TOPICDATA_CONNECTION_MODE.HTTP,
            UbiiNetworkClient.DEFAULT_ADDRESS_TOPICDATA_WS);

        foreach (UbiiTest test in tests)
        {
            UbiiTestResult result = await test.RunTest();
            Debug.Log(result.ToString());
        }

        if (allTestsSuccess)
        {
            Debug.Log("UBII - Test Suite - all tests successful");
        }
        else
        {
            Debug.LogError("UBII - Test Suite - some test(s) failed");
        }
    }
}
