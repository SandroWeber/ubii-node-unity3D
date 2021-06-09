using System;
using System.Collections.Generic;
using Ubii.TopicData;

interface ITopicDataBuffer
{
    void Publish(TopicDataRecord msgTopicDataRecord);
    SubscriptionToken Subscribe(string topic, Action<TopicDataRecord> callback);
    void Unsubscribe(SubscriptionToken token);
    void Remove(string topic);
    TopicDataRecord Pull(string topic);
    List<SubscriptionToken> GetSubscriptionTokens(string topic);
}