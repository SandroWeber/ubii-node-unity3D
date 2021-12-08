using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.Devices;
using Ubii.Services;
using Ubii.TopicData;

interface IUbiiNode
{
    // service related functions
    Task<ServiceReply> CallService(ServiceRequest request);

    // status related functions
    bool IsConnected();

    // topic data related functions
    // TODO Unsub Regex!
    void Publish(TopicDataRecord record);
    Task<SubscriptionToken> SubscribeTopic(string topic, Action<TopicDataRecord> callback);
    Task<SubscriptionToken> SubscribeRegex(string regex, Action<TopicDataRecord> callback);
    Task<bool> Unsubscribe(SubscriptionToken token);
    Task<ServiceReply> RegisterDevice(Device ubiiDevice);
    Task<ServiceReply> DeregisterDevice(Device ubiiDevice);
}
