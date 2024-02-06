using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Ubii.TopicData;
using System.Linq;

public class TestPerformancePubSub : UbiiTest
{
    const int TIMEOUT_SECONDS = 30, DEFAULT_NUM_TOPICS = 5, MIN_PUBLISH_INTERVAL_MS = 5, DEFAULT_PUBLISH_INTERVAL_MS = 10, NUM_MESSAGES = 100;
    const bool DEFAULT_PUBLISH_IMMEDIATELY = false;
    private List<SubscriptionToken> subTokens = new List<SubscriptionToken>();

    private List<string> topics = new List<string>();
    private List<Task> tasksPublishing = new List<Task>();
    private Dictionary<string, int> receivedMessages = new Dictionary<string, int>();
    private Dictionary<string, Stopwatch> stopWatches = new Dictionary<string, Stopwatch>();
    private CancellationTokenSource ctsCancelTest;
    private int publishIntervalMs, numTopics;
    private bool publishImmediately;

    public TestPerformancePubSub(UbiiNode node, int publishIntervalMs = DEFAULT_PUBLISH_INTERVAL_MS, int numTopics = DEFAULT_NUM_TOPICS, bool publishImmediately = DEFAULT_PUBLISH_IMMEDIATELY) : base(node)
    {
        this.publishIntervalMs = publishIntervalMs;
        this.numTopics = numTopics;
        this.publishImmediately = publishImmediately;
    }

    override public async Task<UbiiTestResult> RunTest()
    {
        return await RunTestIteration(publishIntervalMs, publishImmediately);
    }

    override public Task<bool> CancelTest()
    {
        ctsCancelTest.Cancel();
        return Task.FromResult(true);
    }

    private async Task<UbiiTestResult> RunTestIteration(int publishIntervalMs, bool publishImmediately)
    {
        await node.WaitForConnection();
        UnityEngine.Debug.Log("TestPerformancePubSub - started - publish interval (ms): " + publishIntervalMs + ", using publish immediately: " + publishImmediately);

        ctsCancelTest = new CancellationTokenSource();
        topics.Clear();
        receivedMessages.Clear();
        stopWatches.Clear();

        for (int i = 0; i < DEFAULT_NUM_TOPICS; i++)
        {
            string topic = Guid.NewGuid().ToString();
            topics.Add(topic);
            receivedMessages[topic] = 0;
            stopWatches[topic] = new Stopwatch();
        }

        foreach (string topic in topics)
        {
            subTokens.Add(await node.SubscribeTopic(topic, (TopicDataRecord record) =>
            {
                receivedMessages[record.Topic] = record.Int32;
                if (record.Int32 == NUM_MESSAGES)
                {
                    stopWatches[record.Topic].Stop();
                }
            }));

            tasksPublishing.Add(Task.Run(() =>
            {
                int counter = 0;
                while (counter < NUM_MESSAGES && !ctsCancelTest.IsCancellationRequested)
                {
                    counter++;
                    if (counter == NUM_MESSAGES)
                    {
                        stopWatches[topic].Start();
                    }

                    TopicDataRecord record = new TopicDataRecord { Topic = topic, Int32 = counter };
                    if (publishImmediately)
                    {
                        node.PublishImmediately(record);
                    }
                    else
                    {
                        node.Publish(record);
                    }

                    Task.Delay(publishIntervalMs).Wait(ctsCancelTest.Token);
                }
            }, ctsCancelTest.Token));
        }

        return await WaitForTestToFinish();
    }

    private async Task<UbiiTestResult> WaitForTestToFinish()
    {
        UbiiTestResult result;
        CancellationTokenSource ctsTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        try
        {
            TimeSpan sumPublishDelay = new TimeSpan();
            await Task.Run(async () =>
            {
                foreach (Task taskPublishing in tasksPublishing)
                {
                    await taskPublishing;
                }

                bool allMessagesReceived = false;
                while (!allMessagesReceived && !ctsTimeout.IsCancellationRequested)
                {
                    Task.Delay(500).Wait(ctsTimeout.Token);
                    allMessagesReceived = receivedMessages.Values.All(messageCount => messageCount == NUM_MESSAGES);
                }

                foreach (var stopwatchEntry in stopWatches)
                {
                    sumPublishDelay += stopwatchEntry.Value.Elapsed;
                }
            }, ctsTimeout.Token);

            result = CreateTestResult(
                true,
                "test completed (pub interval ms: " + publishIntervalMs + ", num topics: " + numTopics + ", publish immediately: " + publishImmediately +
                "), delay from last message sent to last message received: " + sumPublishDelay);
        }
        catch (OperationCanceledException e)
        {
            result = CreateTestResult(
                false,
                "timeout(pub interval ms: " + publishIntervalMs + ", num topics: " + numTopics + ", publish immediately: " + publishImmediately + ")");
        }
        finally
        {
            ctsTimeout.Dispose();
        }

        foreach (var token in subTokens)
        {
            await node.Unsubscribe(token);
        }

        return result;
    }
}