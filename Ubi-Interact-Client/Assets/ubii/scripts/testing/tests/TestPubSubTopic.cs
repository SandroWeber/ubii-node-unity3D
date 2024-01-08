using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Ubii.TopicData;
using System.Linq;

public class TestPubSubTopic : UbiiTest
{
    static bool USE_PUBLISH_IMMEDIATELY = true;
    static int TIMEOUT_SECONDS = 5, NUM_TOPICS = 5;
    private Dictionary<string, int> dictTopicToValue = new Dictionary<string, int>(NUM_TOPICS);
    private SubscriptionToken[] subTokens = new SubscriptionToken[NUM_TOPICS];

    public TestPubSubTopic(UbiiNode node) : base(node) { }

    override public async Task<UbiiTestResult> RunTest()
    {
        await node.WaitForConnection();
        dictTopicToValue.Clear();

        Random rnd = new Random();
        for (int i = 0; i < NUM_TOPICS; i++)
        {
            string topic = Guid.NewGuid().ToString();
            dictTopicToValue.Add(topic, rnd.Next(1, 99));
            subTokens[i] = await node.SubscribeTopic(topic, async (TopicDataRecord record) =>
                {
                    if (record.Int32 == dictTopicToValue[topic])
                    {
                        dictTopicToValue.Remove(topic);
                    }
                });
        }

        string[] topics = dictTopicToValue.Keys.ToArray();
        foreach (string topic in topics)
        {
            if (USE_PUBLISH_IMMEDIATELY)
            {
                await node.PublishImmediately(new TopicDataRecord { Topic = topic, Int32 = dictTopicToValue[topic] });
            }
            else
            {
                node.Publish(new TopicDataRecord { Topic = topic, Int32 = dictTopicToValue[topic] });
            }
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