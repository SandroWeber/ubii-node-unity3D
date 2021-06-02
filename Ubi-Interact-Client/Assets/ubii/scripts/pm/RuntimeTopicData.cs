using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.TopicData;
using UnityEngine;

public class RuntimeTopicData
{
    private Dictionary<string, TopicDataRecord> localTopics;
    private Dictionary<string, List<SubscriptionToken>> subscriberCallbacks;

    private int currentTokenId = -1;

    public RuntimeTopicData()
    {
        localTopics = new Dictionary<string, TopicDataRecord>();
        subscriberCallbacks = new Dictionary<string, List<SubscriptionToken>>();
    }

    /// <summary>
    /// Publishes data under the specified topic to the topic data
    /// If there is already data associated with this topic, it will be overwritten.
    /// </summary>
    /// <param name="msgTopicDataRecord">The topic data to publish</param>
    public void Push(TopicDataRecord msgTopicDataRecord)
    {
        if (!localTopics.ContainsKey(msgTopicDataRecord.Topic))
            localTopics.Add(msgTopicDataRecord.Topic, msgTopicDataRecord);

        localTopics[msgTopicDataRecord.Topic] = msgTopicDataRecord;
        NotifySubscribers(msgTopicDataRecord);
        // TODO: universalSubs?
    }

    /// <summary>
    /// Pulls the data from topic data that is associated with specified topic.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public TopicDataRecord Pull(string topic)
    {
        //                                      Do we need a clone here?
        return localTopics.ContainsKey(topic) ? localTopics[topic] : null;
    }

    /// <summary>
    /// Subscribes the callback function to the specified topic.
    /// The callback function is called with the topic and a data parameter whenever data is published to the specified topic.
    /// Returns a token which can be passed to the unsubscribe method in order to unsubscribe the callback from the topic.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="callback"></param>
    /// <returns>Generated subscription token</returns>
    public SubscriptionToken Subscribe(string topic, Action<TopicDataRecord> callback)
    {
        if (!subscriberCallbacks.ContainsKey(topic))
            subscriberCallbacks.Add(topic, new List<SubscriptionToken>());

        SubscriptionToken token = GenerateToken(topic, callback);
        subscriberCallbacks[topic].Add(token);

        return token;
    }

    public void SubscribeAll()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unsubscribes callback from topic
    /// </summary>
    /// <param name="token">Subscription token identifying subscription and callback</param>
    public void Unsubscribe(SubscriptionToken token)
    {
        if (subscriberCallbacks.ContainsKey(token.topic) && subscriberCallbacks[token.topic].Exists(callback => callback.id == token.id))
            subscriberCallbacks[token.topic].RemoveAll(callback => callback.id == token.id); // Alternative: RemoveAt(.Find(lambda)), but there is only 1 element anyway
    }

    public void Remove(string topic)
    {
        if (!localTopics.ContainsKey(topic))
            return;
        localTopics.Remove(topic);
    }

    public List<SubscriptionToken> GetSubscriptionTokens(string topic)
    {
        return subscriberCallbacks[topic];
    }

    //--------------------------------------
    //          Private Methods
    //--------------------------------------

    private void NotifySubscribers(TopicDataRecord topicData)
    {
        foreach (SubscriptionToken token in subscriberCallbacks[topicData.Topic])
        {
            token.callback.Invoke(topicData);
        }
    }

    /// <summary>
    /// Creates a subscription token, token type not implemented yet
    /// </summary>
    /// <param name="topic">Topic to subscribe to</param>
    /// <param name="callback">Callback from subscriber</param>
    /// <returns></returns>
    private SubscriptionToken GenerateToken(string topic, Action<TopicDataRecord> callback)
    {
        return new SubscriptionToken
        {
            topic = topic,
            callback = callback,
            id = ++currentTokenId,
            type = "single"
        };
    }
}

/// <summary>
/// Subsription token used for PMs to store each subscription
/// </summary>
public struct SubscriptionToken
{
    public int id;
    public string topic;
    public Action<TopicDataRecord> callback;
    public string type; // or enum
}
