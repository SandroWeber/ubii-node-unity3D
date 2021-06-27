using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.TopicData;

interface ITopicDataBuffer
{
    void Publish(TopicDataRecord msgTopicDataRecord);
    Task<SubscriptionToken> SubscribeTopic(string topic, Action<TopicDataRecord> callback);
    Task<SubscriptionToken> SubscribeRegex(string regex, Action<TopicDataRecord> callback);
    Task<bool> Unsubscribe(SubscriptionToken token);
    void Remove(string topic);
    TopicDataRecord Pull(string topic);
    List<SubscriptionToken> GetTopicSubscriptionTokens(string topic);
    List<SubscriptionToken> GetRegexSubscriptionTokens(string topic);
}