using UnityEngine;

using System.Collections.Generic;
using System;

[Serializable]
public sealed class UbiiConstants
{
    [Serializable]
    public struct Services {
        public string SERVER_CONFIG;
        public string CLIENT_REGISTRATION;
        public string CLIENT_DEREGISTRATION;
        public string CLIENT_GET_LIST;
        public string DEVICE_REGISTRATION;
        public string DEVICE_DEREGISTRATION;
        public string DEVICE_GET_LIST;
        public string INTERACTION_REGISTRATION;
        public string INTERACTION_DATABASE_GET;
        public string INTERACTION_DATABASE_GET_LIST;
        public string INTERACTION_LOCAL_DATABASE_GET_LIST;
        public string INTERACTION_ONLINE_DATABASE_GET_LIST;
        public string INTERACTION_REPLACE;
        public string INTERACTION_DELETE;
        public string SESSION_REGISTRATION;
        public string SESSION_RUNTIME_GET;
        public string SESSION_RUNTIME_GET_LIST;
        public string SESSION_DATABASE_GET;
        public string SESSION_DATABASE_GET_LIST;
        public string SESSION_REPLACE;
        public string SESSION_DELETE;
        public string SESSION_START;
        public string SESSION_STOP;
        public string TOPIC_DEMUX_DATABASE_ADD;
        public string TOPIC_DEMUX_DATABASE_DELETE;
        public string TOPIC_DEMUX_DATABASE_GET;
        public string TOPIC_DEMUX_DATABASE_GET_LIST;
        public string TOPIC_DEMUX_DATABASE_REPLACE;
        public string TOPIC_DEMUX_RUNTIME_GET;
        public string TOPIC_DEMUX_RUNTIME_GET_LIST;
        public string TOPIC_DEMUX_RUNTIME_START;
        public string TOPIC_DEMUX_RUNTIME_STOP;
        public string TOPIC_MUX_DATABASE_ADD;
        public string TOPIC_MUX_DATABASE_DELETE;
        public string TOPIC_MUX_DATABASE_GET;
        public string TOPIC_MUX_DATABASE_GET_LIST;
        public string TOPIC_MUX_DATABASE_REPLACE;
        public string TOPIC_MUX_RUNTIME_GET;
        public string TOPIC_MUX_RUNTIME_GET_LIST;
        public string TOPIC_MUX_RUNTIME_START;
        public string TOPIC_MUX_RUNTIME_STOP;
        public string TOPIC_LIST;
        public string TOPIC_SUBSCRIPTION;
    }
    
    [Serializable]
    public struct InfoTopics {
        public string NEW_INTERACTION;
        public string DELETE_INTERACTION;
        public string CHANGE_INTERACTION;
        public string NEW_SESSION;
        public string DELETE_SESSION;
        public string CHANGE_SESSION;
    }

    [Serializable]
    public struct DefaultTopics {
        public Services SERVICES;
        public InfoTopics INFO_TOPICS;
    }

    [Serializable]
    public struct MsgTypes {
        public string ERROR;
        public string SUCCESS;
        public string CLIENT;
        public string DEVICE;
        public string TOPIC_MUX;
        public string TOPIC_MUX_LIST;
        public string TOPIC_DEMUX;
        public string TOPIC_DEMUX_LIST;
        public string INTERACTION;
        public string SERVICE_REQUEST;
        public string SERVICE_REPLY;
        public string SESSION;
        public string TOPIC_DATA;
    }

    public DefaultTopics DEFAULT_TOPICS;
    public MsgTypes MSG_TYPES;
    
    private static readonly Lazy<UbiiConstants> lazy = new Lazy<UbiiConstants>(() => UbiiConstants.CreateFromJSON());

    public static UbiiConstants Instance { get { return lazy.Value; } }

    private static UbiiConstants CreateFromJSON()
    {
        var jsonTextFile = Resources.Load<TextAsset>("ubii/constants");
        UbiiConstants constants = JsonUtility.FromJson<UbiiConstants>(jsonTextFile.text);
        return constants;
    }
}