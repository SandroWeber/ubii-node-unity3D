using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ubii.TopicData;
using UnityEngine;

public class TopicDataBuffer : ITopicDataBuffer
{
    private Dictionary<string, TopicDataRecord> localTopics;
    private Dictionary<string, List<SubscriptionToken>> dictTopicSubscriptionTokens;
    private Dictionary<string, List<SubscriptionToken>> dictRegexSubscriptionTokens;
    private Dictionary<string, List<string>> dictTopic2RegexMatches;

    private int currentTokenId = -1;

    public TopicDataBuffer()
    {
        localTopics = new Dictionary<string, TopicDataRecord>();
        dictTopicSubscriptionTokens = new Dictionary<string, List<SubscriptionToken>>();
        dictRegexSubscriptionTokens = new Dictionary<string, List<SubscriptionToken>>();
        dictTopic2RegexMatches = new Dictionary<string, List<string>>();
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
                string regex = entry.Key;
                Match match = Regex.Match(record.Topic, regex);
                if (match.Success)
                {
                    if (!dictTopic2RegexMatches.ContainsKey(record.Topic))
                        dictTopic2RegexMatches.Add(record.Topic, new List<string>());
                    dictTopic2RegexMatches[record.Topic].Add(regex);
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
    public async Task<SubscriptionToken> SubscribeTopic(string topic, Action<TopicDataRecord> callback)
    {
        if (!dictTopicSubscriptionTokens.ContainsKey(topic))
            dictTopicSubscriptionTokens.Add(topic, new List<SubscriptionToken>());

        SubscriptionToken token = GenerateToken(topic, callback, SUBSCRIPTION_TOKEN_TYPE.TOPIC);
        dictTopicSubscriptionTokens[topic].Add(token);

        return token;
    }

    public async Task<SubscriptionToken> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        SubscriptionToken token = GenerateToken(regex, callback, SUBSCRIPTION_TOKEN_TYPE.REGEX);

        if (!dictRegexSubscriptionTokens.ContainsKey(regex))
            dictRegexSubscriptionTokens.Add(regex, new List<SubscriptionToken>());
        dictRegexSubscriptionTokens[regex].Add(token);

        // check whether existing topics match the regex
        foreach (var entry in localTopics)
        {
            string topic = entry.Key;
            Match match = Regex.Match(topic, regex);
            if (match.Success)
            {
                if (!dictTopic2RegexMatches.ContainsKey(topic))
                    dictTopic2RegexMatches.Add(topic, new List<string>());
                dictTopic2RegexMatches[topic].Add(regex);
            }
        }

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
    public Task<bool> Unsubscribe(SubscriptionToken token)
    {
        bool existingTopic = false;
        if (dictTopicSubscriptionTokens.ContainsKey(token.topic))
        {
            dictTopicSubscriptionTokens[token.topic].RemoveAll(entry => entry.id == token.id);
            existingTopic = true;
        }
        else if (dictRegexSubscriptionTokens.ContainsKey(token.topic))
        {
            dictRegexSubscriptionTokens[token.topic].RemoveAll(entry => entry.id == token.id);
            existingTopic = true;
        }

        return Task.FromResult(existingTopic);
    }

    public void Remove(string topic)
    {
        if (!localTopics.ContainsKey(topic))
            return;
        localTopics.Remove(topic);
    }

    public List<SubscriptionToken> GetTopicSubscriptionTokens(string topic)
    {
        return dictTopicSubscriptionTokens.ContainsKey(topic) ? dictTopicSubscriptionTokens[topic] : null;
    }

    public List<SubscriptionToken> GetRegexSubscriptionTokens(string regex)
    {
        return dictRegexSubscriptionTokens.ContainsKey(regex) ? dictRegexSubscriptionTokens[regex] : null;
    }

    //--------------------------------------
    //          Private Methods
    //--------------------------------------

    private void NotifySubscribers(TopicDataRecord record)
    {
        if (dictTopicSubscriptionTokens.ContainsKey(record.Topic))
        {
            foreach (SubscriptionToken token in dictTopicSubscriptionTokens[record.Topic])
            {
                token.callback?.Invoke(record);
            }
        }

        if (dictTopic2RegexMatches.ContainsKey(record.Topic))
        {
            List<string> matchingRegexes = dictTopic2RegexMatches[record.Topic];
            foreach (var regex in matchingRegexes)
            {
                List<SubscriptionToken> tokens = dictRegexSubscriptionTokens[regex];
                foreach (var token in tokens)
                {
                    token.callback.Invoke(record);
                }

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
