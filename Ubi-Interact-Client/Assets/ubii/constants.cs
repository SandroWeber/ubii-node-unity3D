using UnityEngine;

using System.Collections.Generic;
using System;

[Serializable]
public sealed class UbiiConstants
{
    [Serializable]
    public struct Services
    {
        public string SERVER_CONFIG;

        public string CLIENT_REGISTRATION;
        public string CLIENT_DEREGISTRATION;
        public string CLIENT_GET_LIST;

        public string DEVICE_REGISTRATION;
        public string DEVICE_DEREGISTRATION;
        public string DEVICE_GET;
        public string DEVICE_GET_LIST;

        public string COMPONENT_GET_LIST;

        public string PM_DATABASE_SAVE;
        public string PM_DATABASE_DELETE;
        public string PM_DATABASE_GET;
        public string PM_DATABASE_GET_LIST;
        public string PM_DATABASE_ONLINE_GET_LIST;
        public string PM_DATABASE_LOCAL_GET_LIST;
        public string PM_RUNTIME_ADD;
        public string PM_RUNTIME_REMOVE;
        public string PM_RUNTIME_GET;
        public string PM_RUNTIME_GET_LIST;

        public string SESSION_DATABASE_SAVE;
        public string SESSION_DATABASE_DELETE;
        public string SESSION_DATABASE_GET;
        public string SESSION_DATABASE_GET_LIST;
        public string SESSION_DATABASE_ONLINE_GET_LIST;
        public string SESSION_DATABASE_LOCAL_GET_LIST;
        public string SESSION_RUNTIME_ADD;
        public string SESSION_RUNTIME_REMOVE;
        public string SESSION_RUNTIME_GET;
        public string SESSION_RUNTIME_GET_LIST;
        public string SESSION_RUNTIME_START;
        public string SESSION_RUNTIME_STOP;

        public string TOPIC_DEMUX_DATABASE_SAVE;
        public string TOPIC_DEMUX_DATABASE_DELETE;
        public string TOPIC_DEMUX_DATABASE_GET;
        public string TOPIC_DEMUX_DATABASE_GET_LIST;
        public string TOPIC_DEMUX_RUNTIME_GET;
        public string TOPIC_DEMUX_RUNTIME_GET_LIST;

        public string TOPIC_MUX_DATABASE_SAVE;
        public string TOPIC_MUX_DATABASE_DELETE;
        public string TOPIC_MUX_DATABASE_GET;
        public string TOPIC_MUX_DATABASE_GET_LIST;
        public string TOPIC_MUX_RUNTIME_GET;
        public string TOPIC_MUX_RUNTIME_GET_LIST;
        
        public string SERVICE_LIST;
        public string TOPIC_LIST;
        public string TOPIC_SUBSCRIPTION;
    }

    [Serializable]
    public struct InfoTopics
    {
        public string REGEX_ALL_INFOS;
        public string REGEX_PM_INFOS;
        public string NEW_PM;
        public string DELETE_PM;
        public string CHANGE_PM;
        public string PROCESSED_PM;
        public string REGEX_SESSION_INFOS;
        public string NEW_SESSION;
        public string DELETE_SESSION;
        public string CHANGE_SESSION;
        public string START_SESSION;
        public string STOP_SESSION;
    }

    [Serializable]
    public struct DefaultTopics
    {
        public Services SERVICES;
        public InfoTopics INFO_TOPICS;
    }

    [Serializable]
    public struct MsgTypes
    {
        public string ERROR;
        public string SUCCESS;
        
        public string SERVER;
        public string CLIENT;
        public string CLIENT_LIST;

        public string DEVICE;
        public string DEVICE_LIST;
        public string COMPONENT;
        public string COMPONENT_LIST;

        public string TOPIC_MUX;
        public string TOPIC_MUX_LIST;
        public string TOPIC_DEMUX;
        public string TOPIC_DEMUX_LIST;

        public string SERVICE;
        public string SERVICE_LIST;
        public string SERVICE_REQUEST;
        public string SERVICE_REPLY;
        public string SERVICE_REUEST_TOPIC_SUBSCRIPTION;
        
        public string SESSION;
        public string SESSION_LIST;
        public string SESSION_IO_MAPPING;
        
        public string PM;
        public string PM_LIST;
        public string PM_MODULE_IO;
        public string PM_PROCESSING_MODE;
        
        public string TOPIC_DATA;
        public string TOPIC_DATA_RECORD;
        public string TOPIC_DATA_RECORD_LIST;
        public string TOPIC_DATA_TIMESTAMP;
        
        public string DATASTRUCTURE_BOOL;
        public string DATASTRUCTURE_BOOL_LIST;
        public string DATASTRUCTURE_INT32;
        public string DATASTRUCTURE_INT32_LIST;
        public string DATASTRUCTURE_STRING;
        public string DATASTRUCTURE_STRING_LIST;
        public string DATASTRUCTURE_FLOAT;
        public string DATASTRUCTURE_FLOAT_LIST;
        public string DATASTRUCTURE_DOUBLE;
        public string DATASTRUCTURE_DOUBLE_LIST;
        public string DATASTRUCTURE_COLOR;
        public string DATASTRUCTURE_IMAGE;
        public string DATASTRUCTURE_IMAGE_LIST;
        public string DATASTRUCTURE_KEY_EVENT;
        public string DATASTRUCTURE_MATRIX_3X2;
        public string DATASTRUCTURE_MATRIX_4X4;
        public string DATASTRUCTURE_MOUSE_EVENT;
        public string DATASTRUCTURE_OBJECT2D;
        public string DATASTRUCTURE_OBJECT2D_LIST;
        public string DATASTRUCTURE_OBJECT3D;
        public string DATASTRUCTURE_OBJECT3D_LIST;
        public string DATASTRUCTURE_POSE2D;
        public string DATASTRUCTURE_POSE3D;
        public string DATASTRUCTURE_QUATERNION;
        public string DATASTRUCTURE_TOUCH_EVENT;
        public string DATASTRUCTURE_VECTOR2;
        public string DATASTRUCTURE_VECTOR3;
        public string DATASTRUCTURE_VECTOR4;
        public string DATASTRUCTURE_VECTOR8;
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