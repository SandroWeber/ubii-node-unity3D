using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubii.Devices;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.UtilityFunctions.Parser;

interface IUbiiClient
{
    // service related functions
    Task<ServiceReply> CallService(ServiceRequest request);

    // status related functions
    bool IsConnected();

    // topic data related functions
    // TODO Unsub Regex!
    void Publish(TopicData topicdata);
    Task<bool> Subscribe(string topic, Action<TopicDataRecord> callback);
    Task<bool> SubscribeRegex(string regex, Action<TopicDataRecord> callback);
    Task<bool> Unsubscribe(string topic, Action<TopicDataRecord> callback);
    Task<ServiceReply> RegisterDevice(Device ubiiDevice);
    Task<ServiceReply> DeregisterDevice(Device ubiiDevice);
}
