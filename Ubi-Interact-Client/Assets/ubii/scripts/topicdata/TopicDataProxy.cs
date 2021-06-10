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

    public SubscriptionToken SubscribeTopic(string topic, Action<TopicDataRecord> callback)
    {
        return topicDataBuffer.SubscribeTopic(topic, callback);
    }

    public SubscriptionToken SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        return topicDataBuffer.SubscribeRegex(regex, callback);
    }

    public void Unsubscribe(SubscriptionToken token)
    {
        topicDataBuffer.Unsubscribe(token);
    }

    public void Remove(string topic)
    {
        topicDataBuffer.Remove(topic);
    }

    public List<SubscriptionToken> GetTopicSubscriptionTokens(string topic)
    {
        return topicDataBuffer.GetTopicSubscriptionTokens(topic);
    }

    public List<SubscriptionToken> GetRegexSubscriptionTokens(string regex)
    {
        return topicDataBuffer.GetRegexSubscriptionTokens(regex);
    }
}