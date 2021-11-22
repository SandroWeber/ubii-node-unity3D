using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.Devices;
using Ubii.Services;
using Ubii.TopicData;

interface ITopicDataClient
{
    // status related functions
    bool IsConnected();
    void TearDown();

    // topic data related functions
    bool IsSubscribed(string topicOrRegex);
    bool HasTopicCallbacks(string topic);
    bool HasTopicRegexCallbacks(string regex);
    List<string> GetAllSubscribedTopics();
    List<string> GetAllSubscribedRegex();
    void SetPublishDelay(int millisecs);

    void AddTopicCallback(string topic, Action<TopicDataRecord> callback);
    void AddTopicRegexCallback(string regex, Action<TopicDataRecord> callback);
    void RemoveTopicCallback(string topic, Action<TopicDataRecord> callback);
    void RemoveTopicRegexCallback(string regex, Action<TopicDataRecord> callback);
    void RemoveAllTopicCallbacks(string topic);
    void RemoveAllTopicRegexCallbacks(string regex);
    void SendTopicDataRecord(TopicDataRecord record);
    void SendTopicDataImmediately(TopicData topicData);
}
