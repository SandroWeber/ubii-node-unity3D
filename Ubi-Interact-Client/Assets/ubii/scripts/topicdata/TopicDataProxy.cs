using System;
using System.Collections.Generic;
using Ubii.TopicData;

/// <summary>
/// Topic data proxy implementation
/// </summary>
public class TopicDataProxy : ITopicDataBuffer
{
    private TopicDataBuffer topicDataBuffer;

    public TopicDataProxy(TopicDataBuffer topicDataBuffer)
    {
        this.topicDataBuffer = topicDataBuffer;
    }

    public void Publish(TopicDataRecord topicDataRecord)
    {
        // Send to master node instead of pushing it directly to buffer
        topicDataBuffer.Publish(topicDataRecord);
    }

    public TopicDataRecord Pull(string topic)
    {
        return topicDataBuffer.Pull(topic);
    }

    public SubscriptionToken Subscribe(string topic, Action<TopicDataRecord> callback)
    {
        return topicDataBuffer.Subscribe(topic, callback);
    }

    public void Unsubscribe(SubscriptionToken token)
    {
        topicDataBuffer.Unsubscribe(token);
    }

    public void Remove(string topic)
    {
        topicDataBuffer.Remove(topic);
    }

    public List<SubscriptionToken> GetSubscriptionTokens(string topic)
    {
        return topicDataBuffer.GetSubscriptionTokens(topic);
    }
}