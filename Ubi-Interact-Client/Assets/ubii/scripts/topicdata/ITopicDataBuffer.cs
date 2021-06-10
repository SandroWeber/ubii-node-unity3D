using System;
using System.Collections.Generic;
using Ubii.TopicData;

interface ITopicDataBuffer
{
    void Publish(TopicDataRecord msgTopicDataRecord);
    SubscriptionToken SubscribeTopic(string topic, Action<TopicDataRecord> callback);
    SubscriptionToken SubscribeRegex(string regex, Action<TopicDataRecord> callback);
    void Unsubscribe(SubscriptionToken token);
    void Remove(string topic);
    TopicDataRecord Pull(string topic);
    List<SubscriptionToken> GetTopicSubscriptionTokens(string topic);
    List<SubscriptionToken> GetRegexSubscriptionTokens(string topic);
}