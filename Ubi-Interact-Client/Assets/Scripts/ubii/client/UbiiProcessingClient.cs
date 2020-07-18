using System;
using System.Collections;
using System.Collections.Generic;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.Interactions;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using UnityEngine;

public class UbiiProcessingClient : MonoBehaviour
{
    private string host;
    private int port;
    RequestSocket serviceSocket;
    DealerSocket topicdataSocket;
    bool ssConnected = false;
    bool tdsConnected = false;

    private Dictionary<string, Action<TopicDataRecordList>> onProcessingCallbacks;
    private Dictionary<string, Action<TopicDataRecordList>> onCreatedCallbacks;
    private InteractionStatus status;
}
