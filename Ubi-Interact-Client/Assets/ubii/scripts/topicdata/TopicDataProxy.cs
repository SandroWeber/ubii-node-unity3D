using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ubii.TopicData;
using UnityEngine;

using Google.Protobuf.Collections;

/// <summary>
/// Topic data proxy implementation
/// </summary>
public class TopicDataProxy : ITopicDataBuffer
{
    private TopicDataBuffer topicDataBuffer;
    private UbiiNetworkClient networkClient;
    private ConcurrentDictionary<string, TopicDataRecord> recordsToPublish = new ConcurrentDictionary<string, TopicDataRecord>();
    private Task taskSendTopicData = null;
    private CancellationTokenSource ctsPublish = null;
    private int msPublishDelay = 30;

    public TopicDataProxy(TopicDataBuffer topicDataBuffer, UbiiNetworkClient networkClient)
    {
        this.topicDataBuffer = topicDataBuffer;
        this.networkClient = networkClient;

        networkClient.SetCbHandleTopicData(OnTopicDataMessage);

        ctsPublish = new CancellationTokenSource();
        taskSendTopicData = Task.Factory.StartNew(async () =>
        {
            while (!ctsPublish.IsCancellationRequested)
            {
                await FlushRecordsToPublish();

                await Task.Delay(msPublishDelay, ctsPublish.Token);
            }
        });
    }

    public async void StopPublishing()
    {
        ctsPublish.Cancel();
        await taskSendTopicData;
        taskSendTopicData.Dispose();
    }

    public void SetPublishDelay(int milliseconds)
    {
        msPublishDelay = milliseconds;
    }

    public void Publish(TopicDataRecord record)
    {
        recordsToPublish.AddOrUpdate(record.Topic, record, (key, oldValue) => record);
    }

    public async Task<bool> PublishImmediately(TopicDataRecord topicDataRecord)
    {
        return await networkClient.Send(topicDataRecord, ctsPublish.Token);
    }

    public async Task<bool> PublishImmediately(TopicDataRecordList topicDataRecordList)
    {
        return await networkClient.Send(topicDataRecordList, ctsPublish.Token);
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
            bool success = await networkClient.SubscribeTopic(topic);
            if (!success) Debug.LogError("TopicDataProxy.SubscribeTopic() - failed to subscribe to " + topic);
        }

        return await topicDataBuffer.SubscribeTopic(topic, callback);
    }

    public async Task<SubscriptionToken> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        List<SubscriptionToken> subscriptions = topicDataBuffer.GetRegexSubscriptionTokens(regex);
        if (subscriptions == null || subscriptions.Count == 0)
        {
            bool success = await networkClient.SubscribeRegex(regex);
            if (!success) Debug.LogError("TopicDataProxy.SubscribeRegex() - failed to subscribe to " + regex);
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
                await networkClient.UnsubscribeTopic(token.topic);
            }
        }
        else if (token.type == SUBSCRIPTION_TOKEN_TYPE.REGEX)
        {
            List<SubscriptionToken> subscriptions = topicDataBuffer.GetRegexSubscriptionTokens(token.topic);
            if (subscriptions == null || subscriptions.Count == 0)
            {
                await networkClient.UnsubscribeRegex(token.topic);
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

    private void OnTopicDataMessage(TopicData topicData)
    {
        if (topicData.Error != null)
        {
            Debug.LogError(topicData.Error.ToString());
        }

        if (topicData.TopicDataRecord != null)
        {
            OnTopicDataRecord(topicData.TopicDataRecord);
        }
        if (topicData.TopicDataRecordList != null)
        {
            foreach (TopicDataRecord record in topicData.TopicDataRecordList.Elements)
            {
                OnTopicDataRecord(record);
            }
        }
    }
    private void OnTopicDataRecord(TopicDataRecord record)
    {
        this.topicDataBuffer.Publish(record);
    }

    private async Task<bool> FlushRecordsToPublish()
    {
        if (recordsToPublish.IsEmpty) return true;

        RepeatedField<TopicDataRecord> repeatedField = new RepeatedField<TopicDataRecord>();
        while (!recordsToPublish.IsEmpty)
        {
            if (recordsToPublish.TryRemove(recordsToPublish.First().Key, out TopicDataRecord record))
            {
                repeatedField.Add(record);
            }
        }
        TopicDataRecordList recordList = new TopicDataRecordList()
        {
            Elements = { repeatedField },
        };

        return await networkClient.Send(new TopicData()
        {
            TopicDataRecordList = recordList
        }, ctsPublish.Token);
    }

    public void SendTopicDataRecord(TopicDataRecord record)
    {
        recordsToPublish.AddOrUpdate(record.Topic, record, (key, oldValue) => record);
    }
}