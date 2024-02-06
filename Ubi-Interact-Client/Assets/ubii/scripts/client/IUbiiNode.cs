using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.Devices;
using Ubii.Services;
using Ubii.TopicData;

interface IUbiiNode
{
    string Id { get; }
    string Name { get; }

    // status
    bool IsConnected();
    Task WaitForConnection();

    // configuration
    void SetPublishInterval(int millisecs);

    // services
    Task<ServiceReply> CallService(ServiceRequest request);
    Task<ServiceReply> RegisterDevice(Ubii.Devices.Device ubiiDevice);
    Task<ServiceReply> DeregisterDevice(Ubii.Devices.Device ubiiDevice);

    // topic data
    void Publish(TopicDataRecord record);
    void Publish(TopicDataRecordList recordList);
    Task<bool> PublishImmediately(TopicDataRecord record);
    Task<bool> PublishImmediately(TopicDataRecordList recordList);
    Task<SubscriptionToken> SubscribeTopic(string topic, Action<TopicDataRecord> callback);
    Task<SubscriptionToken> SubscribeRegex(string regex, Action<TopicDataRecord> callback);
    Task<bool> Unsubscribe(SubscriptionToken token);

    Timestamp GenerateTimeStamp();
}
