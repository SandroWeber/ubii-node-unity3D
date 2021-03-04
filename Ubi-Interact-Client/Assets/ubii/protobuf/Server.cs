// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto/servers/server.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ubii.Servers {

  /// <summary>Holder for reflection information generated from proto/servers/server.proto</summary>
  public static partial class ServerReflection {

    #region Descriptor
    /// <summary>File descriptor for proto/servers/server.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ServerReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Chpwcm90by9zZXJ2ZXJzL3NlcnZlci5wcm90bxIMdWJpaS5zZXJ2ZXJzIs4B",
            "CgZTZXJ2ZXISCgoCaWQYASABKAkSDAoEbmFtZRgCIAEoCRITCgtpcF9ldGhl",
            "cm5ldBgDIAEoCRIPCgdpcF93bGFuGAQgASgJEhgKEHBvcnRfc2VydmljZV96",
            "bXEYBSABKAkSGQoRcG9ydF9zZXJ2aWNlX3Jlc3QYBiABKAkSGwoTcG9ydF90",
            "b3BpY19kYXRhX3ptcRgHIAEoCRIaChJwb3J0X3RvcGljX2RhdGFfd3MYCCAB",
            "KAkSFgoOY29uc3RhbnRzX2pzb24YCSABKAliBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.Servers.Server), global::Ubii.Servers.Server.Parser, new[]{ "Id", "Name", "IpEthernet", "IpWlan", "PortServiceZmq", "PortServiceRest", "PortTopicDataZmq", "PortTopicDataWs", "ConstantsJson" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Server : pb::IMessage<Server> {
    private static readonly pb::MessageParser<Server> _parser = new pb::MessageParser<Server>(() => new Server());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Server> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.Servers.ServerReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Server() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Server(Server other) : this() {
      id_ = other.id_;
      name_ = other.name_;
      ipEthernet_ = other.ipEthernet_;
      ipWlan_ = other.ipWlan_;
      portServiceZmq_ = other.portServiceZmq_;
      portServiceRest_ = other.portServiceRest_;
      portTopicDataZmq_ = other.portTopicDataZmq_;
      portTopicDataWs_ = other.portTopicDataWs_;
      constantsJson_ = other.constantsJson_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Server Clone() {
      return new Server(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private string id_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Id {
      get { return id_; }
      set {
        id_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 2;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ip_ethernet" field.</summary>
    public const int IpEthernetFieldNumber = 3;
    private string ipEthernet_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string IpEthernet {
      get { return ipEthernet_; }
      set {
        ipEthernet_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ip_wlan" field.</summary>
    public const int IpWlanFieldNumber = 4;
    private string ipWlan_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string IpWlan {
      get { return ipWlan_; }
      set {
        ipWlan_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "port_service_zmq" field.</summary>
    public const int PortServiceZmqFieldNumber = 5;
    private string portServiceZmq_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string PortServiceZmq {
      get { return portServiceZmq_; }
      set {
        portServiceZmq_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "port_service_rest" field.</summary>
    public const int PortServiceRestFieldNumber = 6;
    private string portServiceRest_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string PortServiceRest {
      get { return portServiceRest_; }
      set {
        portServiceRest_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "port_topic_data_zmq" field.</summary>
    public const int PortTopicDataZmqFieldNumber = 7;
    private string portTopicDataZmq_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string PortTopicDataZmq {
      get { return portTopicDataZmq_; }
      set {
        portTopicDataZmq_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "port_topic_data_ws" field.</summary>
    public const int PortTopicDataWsFieldNumber = 8;
    private string portTopicDataWs_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string PortTopicDataWs {
      get { return portTopicDataWs_; }
      set {
        portTopicDataWs_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "constants_json" field.</summary>
    public const int ConstantsJsonFieldNumber = 9;
    private string constantsJson_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ConstantsJson {
      get { return constantsJson_; }
      set {
        constantsJson_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Server);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Server other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Name != other.Name) return false;
      if (IpEthernet != other.IpEthernet) return false;
      if (IpWlan != other.IpWlan) return false;
      if (PortServiceZmq != other.PortServiceZmq) return false;
      if (PortServiceRest != other.PortServiceRest) return false;
      if (PortTopicDataZmq != other.PortTopicDataZmq) return false;
      if (PortTopicDataWs != other.PortTopicDataWs) return false;
      if (ConstantsJson != other.ConstantsJson) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id.Length != 0) hash ^= Id.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (IpEthernet.Length != 0) hash ^= IpEthernet.GetHashCode();
      if (IpWlan.Length != 0) hash ^= IpWlan.GetHashCode();
      if (PortServiceZmq.Length != 0) hash ^= PortServiceZmq.GetHashCode();
      if (PortServiceRest.Length != 0) hash ^= PortServiceRest.GetHashCode();
      if (PortTopicDataZmq.Length != 0) hash ^= PortTopicDataZmq.GetHashCode();
      if (PortTopicDataWs.Length != 0) hash ^= PortTopicDataWs.GetHashCode();
      if (ConstantsJson.Length != 0) hash ^= ConstantsJson.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      if (IpEthernet.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(IpEthernet);
      }
      if (IpWlan.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(IpWlan);
      }
      if (PortServiceZmq.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(PortServiceZmq);
      }
      if (PortServiceRest.Length != 0) {
        output.WriteRawTag(50);
        output.WriteString(PortServiceRest);
      }
      if (PortTopicDataZmq.Length != 0) {
        output.WriteRawTag(58);
        output.WriteString(PortTopicDataZmq);
      }
      if (PortTopicDataWs.Length != 0) {
        output.WriteRawTag(66);
        output.WriteString(PortTopicDataWs);
      }
      if (ConstantsJson.Length != 0) {
        output.WriteRawTag(74);
        output.WriteString(ConstantsJson);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Id.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Id);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (IpEthernet.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(IpEthernet);
      }
      if (IpWlan.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(IpWlan);
      }
      if (PortServiceZmq.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(PortServiceZmq);
      }
      if (PortServiceRest.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(PortServiceRest);
      }
      if (PortTopicDataZmq.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(PortTopicDataZmq);
      }
      if (PortTopicDataWs.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(PortTopicDataWs);
      }
      if (ConstantsJson.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ConstantsJson);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Server other) {
      if (other == null) {
        return;
      }
      if (other.Id.Length != 0) {
        Id = other.Id;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.IpEthernet.Length != 0) {
        IpEthernet = other.IpEthernet;
      }
      if (other.IpWlan.Length != 0) {
        IpWlan = other.IpWlan;
      }
      if (other.PortServiceZmq.Length != 0) {
        PortServiceZmq = other.PortServiceZmq;
      }
      if (other.PortServiceRest.Length != 0) {
        PortServiceRest = other.PortServiceRest;
      }
      if (other.PortTopicDataZmq.Length != 0) {
        PortTopicDataZmq = other.PortTopicDataZmq;
      }
      if (other.PortTopicDataWs.Length != 0) {
        PortTopicDataWs = other.PortTopicDataWs;
      }
      if (other.ConstantsJson.Length != 0) {
        ConstantsJson = other.ConstantsJson;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Id = input.ReadString();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 26: {
            IpEthernet = input.ReadString();
            break;
          }
          case 34: {
            IpWlan = input.ReadString();
            break;
          }
          case 42: {
            PortServiceZmq = input.ReadString();
            break;
          }
          case 50: {
            PortServiceRest = input.ReadString();
            break;
          }
          case 58: {
            PortTopicDataZmq = input.ReadString();
            break;
          }
          case 66: {
            PortTopicDataWs = input.ReadString();
            break;
          }
          case 74: {
            ConstantsJson = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code