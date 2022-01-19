using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.TopicData;
using UnityEngine;

/// <summary>
/// Topic data proxy implementation
/// </summary>
public class TopicDataProxy : ITopicDataBuffer
{
    private TopicDataBuffer topicDataBuffer;
    private UbiiNetworkClient networkClient;

    public TopicDataProxy(TopicDataBuffer topicDataBuffer, UbiiNetworkClient networkClient)
    {
        this.topicDataBuffer = topicDataBuffer;
        this.networkClient = networkClient;
    }

    public void Publish(TopicDataRecord topicDataRecord)
    {
        networkClient.Publish(topicDataRecord);
    }

    public void PublishImmediately(TopicDataRecord topicDataRecord)
    {
        networkClient.PublishImmediately(topicDataRecord);
    }

    public void PublishImmediately(TopicDataRecordList topicDataRecordList)
    {
        networkClient.PublishImmediately(topicDataRecordList);
    }

    public TopicDataRecord Pull(string topic)
    {
        return topicDataBuffer.Pull(topic);
    }

    public async Task<SubscriptionToken> SubscribeTopic(string topic, Action<TopicDataRecord> callback)
    {
        List<SubscriptionToken> subscriptions = topicDataBuffer.GetTopicSubscriptionTokens(topic);
        if (subscriptions == null || subscriptions.Count == 0)
        {
            bool success = await networkClient.SubscribeTopic(topic, OnTopicDataRecord);
            if (!success) Debug.LogError("TopicDataProxy.SubscribeTopic() - failed to subscribe to " + topic + " at master node");
        }

        return await topicDataBuffer.SubscribeTopic(topic, callback);
    }

    public async Task<SubscriptionToken> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        List<SubscriptionToken> subscriptions = topicDataBuffer.GetRegexSubscriptionTokens(regex);
        if (subscriptions == null || subscriptions.Count == 0)
        {
            bool success = await networkClient.SubscribeRegex(regex, OnTopicDataRecord);
        }

        return await topicDataBuffer.SubscribeRegex(regex, callback);
    }

    public async Task<bool> Unsubscribe(SubscriptionToken token)
    {
        bool successBufferUnsubscribe = await topicDataBuffer.Unsubscribe(token);
        if (!successBufferUnsubscribe) return false;

        if (token.type == SUBSCRIPTION_TOKEN_TYPE.TOPIC)
        {
            List<SubscriptionToken> subscriptions = topicDataBuffer.GetTopicSubscriptionTokens(token.topic);
            if (subscriptions == null || subscriptions.Count == 0)
            {
                await networkClient.UnsubscribeTopic(token.topic, OnTopicDataRecord);
            }
        }
        else if (token.type == SUBSCRIPTION_TOKEN_TYPE.REGEX)
        {
            List<SubscriptionToken> subscriptions = topicDataBuffer.GetRegexSubscriptionTokens(token.topic);
            if (subscriptions == null || subscriptions.Count == 0)
            {
                await networkClient.UnsubscribeRegex(token.topic, OnTopicDataRecord);
            }
        }

        return true;
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
    private void OnTopicDataRecord(TopicDataRecord record)
    {
        this.topicDataBuffer.Publish(record);
    }
}