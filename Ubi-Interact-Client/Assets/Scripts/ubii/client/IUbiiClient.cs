using System;
using System.Threading.Tasks;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.UtilityFunctions.Parser;

interface IUbiiClient
{
    // service related functions
    Task<ServiceReply> CallService(ServiceRequest request);

    // topic data related functions
    void Publish(TopicData topicdata);
    Task<ServiceReply> Subscribe(string topic, Action<TopicDataRecord> callback);
    Task<ServiceReply> Unsubscribe(string topic);
}
