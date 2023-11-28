using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Ubii.TopicData;

public class TestPubSubTopic : UbiiTest
{
    static int TIMEOUT_SECONDS = 5, NUM_TOPICS = 5;
    private Dictionary<string, int> dictTopicToValue = new Dictionary<string, int>(NUM_TOPICS);
    private SubscriptionToken[] subTokens = new SubscriptionToken[NUM_TOPICS];

    public TestPubSubTopic(UbiiNode node) : base(node) { }

    override public async Task<UbiiTestResult> RunTest()
    {
        await node.WaitForConnection();

        Random rnd = new Random();
        for (int i = 0; i < NUM_TOPICS; i++)
        {
            string topic = Guid.NewGuid().ToString();
            dictTopicToValue.Add(topic, rnd.Next(1, 99));
            subTokens[i] = await node.SubscribeTopic(topic, (TopicDataRecord record) =>
                {
                    if (record.Int32 == dictTopicToValue[topic])
                    {
                        dictTopicToValue.Remove(topic);
                    }
                });
        }

        foreach (string topic in dictTopicToValue.Keys)
        {
            node.Publish(new TopicDataRecord { Topic = topic, Int32 = dictTopicToValue[topic] });
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
        Task task = Task.Run(() =>
        {
            while (dictTopicToValue.Count > 0 && !cts.IsCancellationRequested)
            {
                Task.Delay(100).Wait(cts.Token);
            }
        }, cts.Token);

        try
        {
            await task;
            for (int i = 0; i < NUM_TOPICS; i++)
            {
                await node.Unsubscribe(subTokens[i]);
            }
            return new UbiiTestResult(true, this.testName, "test completed successfully");
        }
        catch (OperationCanceledException e)
        {
            return new UbiiTestResult(false, this.testName, "timeout");
        }
        finally
        {
            cts.Dispose();
        }
    }
}