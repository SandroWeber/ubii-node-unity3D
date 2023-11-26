using System.Collections.Generic;
using UnityEngine;

public class TestSuite : MonoBehaviour
{
    public List<UbiiTest> tests = new List<UbiiTest>();

    // Start is called before the first frame update
    async void Start()
    {
        UbiiNode node = GetComponent<UbiiNode>();
        if (node == null) {
            Debug.LogError("TestSuite could not find UbiiNode");
            return;
        }

        tests.Add(new TestPubSubTopic(node));
        tests.Add(new TestPubSubRegex(node));
        tests.Add(new TestParallelServiceCalls(node));

        await node.Initialize();

        foreach (UbiiTest test in tests) {
            UbiiTestResult result = await test.RunTest();
            Debug.Log(result.ToString());
        }
    }
}
