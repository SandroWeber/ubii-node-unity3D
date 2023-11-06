using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Ubii.TopicData;

interface ITopicDataClient
{
    bool IsConnected();
    void TearDown();
    
    Task<bool> Send(TopicData topicData, CancellationToken ct);
}
