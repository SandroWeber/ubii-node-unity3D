using System.Collections.Generic;
using UnityEngine;

public class TestSuite : MonoBehaviour
{
    [SerializeField]
    protected bool testOverZeroMQ = true, testOverHTTP = true, runBasicTests = true, runPerformanceTests = true;
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

        if (runBasicTests)
        {
            tests.Add(new TestPubSubTopic(node));
            tests.Add(new TestPubSubRegex(node));
            tests.Add(new TestParallelServiceCalls(node));
        }

        if (runPerformanceTests)
        {
            tests.Add(new TestPerformancePubSub(node));
        }

        bool allTestsSuccess = true;

        if (testOverZeroMQ)
        {
            await node.Initialize(
                       UbiiNetworkClient.SERVICE_CONNECTION_MODE.ZEROMQ,
                       UbiiNetworkClient.DEFAULT_LOCALHOST_ADDRESS_SERVICE_ZMQ,
                       UbiiNetworkClient.TOPICDATA_CONNECTION_MODE.ZEROMQ,
                       UbiiNetworkClient.DEFAULT_LOCALHOST_ADDRESS_TOPICDATA_ZMQ);

            foreach (UbiiTest test in tests)
            {
                UbiiTestResult result = await test.RunTest();
                Debug.Log(result.ToString());
                if (!result.success) allTestsSuccess = false;
            }

            await node.Disconnect();
        }

        if (testOverHTTP)
        {
            // run tests with HTTP/WS connection
            await node.Initialize(
                UbiiNetworkClient.SERVICE_CONNECTION_MODE.HTTP,
                UbiiNetworkClient.DEFAULT_LOCALHOST_ADDRESS_SERVICE_HTTP,
                UbiiNetworkClient.TOPICDATA_CONNECTION_MODE.HTTP,
                UbiiNetworkClient.DEFAULT_LOCALHOST_ADDRESS_TOPICDATA_WS);

            foreach (UbiiTest test in tests)
            {
                UbiiTestResult result = await test.RunTest();
                Debug.Log(result.ToString());
                if (!result.success) allTestsSuccess = false;
            }

            await node.Disconnect();
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
