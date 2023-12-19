using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Ubii.TopicData;

public class TestPerformancePubSub : UbiiTest
{
    static int TIMEOUT_SECONDS = 60, NUM_TOPICS = 5, MIN_PUBLISH_INTERVAL_MS = 5, NUM_MESSAGES = 100;
    private List<SubscriptionToken> subTokens = new List<SubscriptionToken>();

    private List<string> topics = new List<string>();
    private List<Task> tasks = new List<Task>();
    private Dictionary<string, int> receivedMessages = new Dictionary<string, int>();
    private Dictionary<string, Stopwatch> stopWatches = new Dictionary<string, Stopwatch>();
    private CancellationTokenSource ctsCancelTest;

    public TestPerformancePubSub(UbiiNode node) : base(node) { }

    override public async Task<UbiiTestResult> RunTest()
    {
        await node.WaitForConnection();

        ctsCancelTest = new CancellationTokenSource();
        topics.Clear();
        receivedMessages.Clear();
        stopWatches.Clear();

        int publishIntervalMs = 100;

        for (int i = 0; i < NUM_TOPICS; i++)
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
                receivedMessages[record.Topic] = receivedMessages[record.Topic]++;
                if (receivedMessages[record.Topic] == NUM_MESSAGES)
                {
                    stopWatches[topic].Stop();
                }
            }));
            tasks.Add(Task.Run(async () =>
            {
                int counter = 0;
                while (counter < NUM_MESSAGES)
                {
                    counter++;
                    if (counter == NUM_MESSAGES)
                    {
                        stopWatches[topic].Start();
                    }
                    node.Publish(new TopicDataRecord { Topic = topic, Int32 = counter });
                    Task.Delay(publishIntervalMs).Wait(ctsCancelTest.Token); ;
                }
            }));

        }

        return await WaitForTestToFinish();
    }

    override public Task<bool> CancelTest()
    {
        return Task.FromResult(true);
    }

    private async Task<UbiiTestResult> WaitForTestToFinish()
    {
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        /*Task task = Task.Run(() =>
        {
            while (dictTopicToValue.Count > 0 && !cts.IsCancellationRequested)
            {
                Task.Delay(100).Wait(cts.Token);
            }
        }, cts.Token);*/

        try
        {

            foreach (Task task in tasks)
            {
                await task;
            }
            foreach (var token in subTokens)
            {
                await node.Unsubscribe(token);
            }
            return CreateTestResult(true, "test completed successfully");
        }
        catch (OperationCanceledException e)
        {
            return CreateTestResult(false, "timeout");
        }
        finally
        {
            cts.Dispose();
        }
    }
}