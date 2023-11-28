using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Ubii.TopicData;

public class TestPubSubRegex : UbiiTest
{
    static int TIMEOUT_SECONDS = 5, NUM_TOPICS = 5;
    private Dictionary<string, int> dictTopicToValue = new Dictionary<string, int>(NUM_TOPICS);
    private List<SubscriptionToken> subTokens = new List<SubscriptionToken>();
    private bool receivedInvalidTopic = false;

    public TestPubSubRegex(UbiiNode node) : base(node) { }

    override public async Task<UbiiTestResult> RunTest()
    {
        await node.WaitForConnection();

        Random rnd = new Random();
        string topicPrefix = Guid.NewGuid().ToString();
        for (int i = 0; i < NUM_TOPICS; i++)
        {
            string topic = topicPrefix + "/" + i;
            dictTopicToValue.Add(topic, rnd.Next(1, 99));
        }

        string regexInvalid = UbiiUtils.REGEX_UUIDv4 + "/[a-z]+";
        SubscriptionToken subToken2 = await node.SubscribeRegex(regexInvalid, (TopicDataRecord record) =>
            {
                receivedInvalidTopic = true;
            });
        subTokens.Add(subToken2);

        string regexValid = UbiiUtils.REGEX_UUIDv4 + "/[0-9]+";
        SubscriptionToken subToken1 = await node.SubscribeRegex(regexValid, (TopicDataRecord record) =>
            {
                if (record.Int32 == dictTopicToValue[record.Topic])
                {
                    dictTopicToValue.Remove(record.Topic);
                }
            });
        subTokens.Add(subToken1);

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
            foreach (SubscriptionToken token in subTokens)
            {
                await node.Unsubscribe(token);
            }

            if (receivedInvalidTopic)
            {
                return CreateTestResult(false, "callback for invalid regex was called");
            }
            else
            {
                return CreateTestResult(true, "test completed successfully");
            }
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