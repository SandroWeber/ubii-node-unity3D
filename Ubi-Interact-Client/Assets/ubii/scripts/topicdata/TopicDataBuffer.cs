using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.TopicData;
using UnityEngine;

public class TopicDataBuffer : ITopicDataBuffer
{
    private Dictionary<string, TopicDataRecord> localTopics;
    private Dictionary<string, List<SubscriptionToken>> dictTopicSubscriptionTokens;
    private Dictionary<string, List<SubscriptionToken>> dictRegexSubscriptionTokens;
    private Dictionary<string, List<SubscriptionToken>> dictTopic2RegexTokenMatches;

    private int currentTokenId = -1;

    public TopicDataBuffer()
    {
        localTopics = new Dictionary<string, TopicDataRecord>();
        dictTopicSubscriptionTokens = new Dictionary<string, List<SubscriptionToken>>();
        dictRegexSubscriptionTokens = new Dictionary<string, List<SubscriptionToken>>();
    }

    /// <summary>
    /// Publishes data under the specified topic to the topic data
    /// If there is already data associated with this topic, it will be overwritten.
    /// </summary>
    /// <param name="record">The topic data to publish</param>
    public void Publish(TopicDataRecord record)
    {
        if (!localTopics.ContainsKey(record.Topic))
        {
            localTopics.Add(record.Topic, record);
            foreach (var entry in dictRegexSubscriptionTokens)
            {
                regexString = entry.Key;
                if (Regex.Match(record.Topic, regexString))
                {
                    if (!dictTopic2RegexTokenMatches.ContainsKey(record.Topic))
                        dictTopic2RegexTokenMatches.Add(record.Topic, new List<SubscriptionToken>());
                    dictTopic2RegexTokenMatches[record.Topic].Add(entry.Value);
                }
            }
        }
        else
        {
            localTopics[record.Topic] = record;
        }

        NotifySubscribers(record);
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
        if (!dictTopicSubscriptionTokens.ContainsKey(topic))
            dictTopicSubscriptionTokens.Add(topic, new List<SubscriptionToken>());

        SubscriptionToken token = GenerateToken(topic, callback, SUBSCRIPTION_TOKEN_TYPE.TOPIC);
        dictTopicSubscriptionTokens[topic].Add(token);

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
        if (dictTopicSubscriptionTokens.ContainsKey(token.topic) && dictTopicSubscriptionTokens[token.topic].Exists(callback => callback.id == token.id))
            dictTopicSubscriptionTokens[token.topic].RemoveAll(callback => callback.id == token.id); // Alternative: RemoveAt(.Find(lambda)), but there is only 1 element anyway
    }

    public SubscriptionToken SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        SubscriptionToken token = GenerateToken(regex, callback, SUBSCRIPTION_TOKEN_TYPE.REGEX);

        if (!dictRegexSubscriptionTokens.ContainsKey(regex))
            dictRegexSubscriptionTokens.Add(regex, new List<SubscriptionToken>());
        dictRegexSubscriptionTokens[regex].Add(token);

        // check whether existing topics match the regex
        foreach (var entry in localTopics)
        {
            string topic = entry.Key;
            if (Regex.Match(topic, regex))
            {
                if (!dictTopic2RegexTokenMatches.ContainsKey(topic))
                    dictTopic2RegexTokenMatches.Add(topic, new List<SubscriptionToken>());
                dictTopic2RegexTokenMatches[record.Topic].Add(token);
            }
        }

        return token;
    }

    public void Remove(string topic)
    {
        if (!localTopics.ContainsKey(topic))
            return;
        localTopics.Remove(topic);
    }

    public List<SubscriptionToken> GetTopicSubscriptionTokens(string topic)
    {
        return dictTopicSubscriptionTokens[topic];
    }

    public List<SubscriptionToken> GetRegexSubscriptionTokens(string regex)
    {
        return dictRegexSubscriptionTokens[regex];
    }

    //--------------------------------------
    //          Private Methods
    //--------------------------------------

    private void NotifySubscribers(TopicDataRecord record)
    {
        foreach (SubscriptionToken token in dictTopicSubscriptionTokens[record.Topic])
        {
            token.callback.Invoke(record);
        }

        if (dictTopic2RegexTokenMatches.ContainsKey(record.Topic))
        {
            foreach (var entry in dictTopic2RegexTokenMatches[record.Topic])
            {
                entry.callback.Invoke(record);
            }
        }
    }

    /// <summary>
    /// Creates a subscription token, token type not implemented yet
    /// </summary>
    /// <param name="topic">Topic to subscribe to</param>
    /// <param name="callback">Callback from subscriber</param>
    /// <returns></returns>
    private SubscriptionToken GenerateToken(string topic, Action<TopicDataRecord> callback, SUBSCRIPTION_TOKEN_TYPE type)
    {
        return new SubscriptionToken
        {
            topic = topic,
            callback = callback,
            id = ++currentTokenId,
            type = type
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
    public SUBSCRIPTION_TOKEN_TYPE type;
}

public enum SUBSCRIPTION_TOKEN_TYPE
{
    TOPIC,
    REGEX
}
