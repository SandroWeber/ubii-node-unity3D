using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Ubii.TopicData;

interface ITopicDataClient
{
    bool IsConnected();
    //Task<bool> TearDown();
    Task<bool> ShutDownGracefully();
    bool ShutDownImmediately();
    
    Task<bool> Send(TopicData topicData, CancellationToken ct);
}
