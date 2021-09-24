using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.Devices;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.UtilityFunctions.Parser;

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

    void AddTopicDataCallback(string topic, Action<TopicDataRecord> callback);
    void AddTopicDataRegexCallback(string regex, Action<TopicDataRecord> callback);
    void RemoveAllTopicCallbacks(string topic);
    void RemoveTopicCallback(string topic, Action<TopicDataRecord> callback);
    void RemoveAllTopicRegexCallbacks(string regex);
    void RemoveTopicRegexCallback(string regex, Action<TopicDataRecord> callback);
    void SendTopicDataRecord(TopicDataRecord record);
    void SendTopicDataImmediately(TopicData topicData);
}
