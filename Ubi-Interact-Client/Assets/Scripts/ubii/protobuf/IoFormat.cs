// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto/interactions/ioFormat.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ubii.Interactions {

  /// <summary>Holder for reflection information generated from proto/interactions/ioFormat.proto</summary>
  public static partial class IoFormatReflection {

    #region Descriptor
    /// <summary>File descriptor for proto/interactions/ioFormat.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static IoFormatReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiFwcm90by9pbnRlcmFjdGlvbnMvaW9Gb3JtYXQucHJvdG8SEXViaWkuaW50",
            "ZXJhY3Rpb25zIjkKCElPRm9ybWF0EhUKDWludGVybmFsX25hbWUYASABKAkS",
            "FgoObWVzc2FnZV9mb3JtYXQYAiABKAliBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.Interactions.IOFormat), global::Ubii.Interactions.IOFormat.Parser, new[]{ "InternalName", "MessageFormat" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class IOFormat : pb::IMessage<IOFormat> {
    private static readonly pb::MessageParser<IOFormat> _parser = new pb::MessageParser<IOFormat>(() => new IOFormat());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<IOFormat> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.Interactions.IoFormatReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public IOFormat() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public IOFormat(IOFormat other) : this() {
      internalName_ = other.internalName_;
      messageFormat_ = other.messageFormat_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public IOFormat Clone() {
      return new IOFormat(this);
    }

    /// <summary>Field number for the "internal_name" field.</summary>
    public const int InternalNameFieldNumber = 1;
    private string internalName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string InternalName {
      get { return internalName_; }
      set {
        internalName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "message_format" field.</summary>
    public const int MessageFormatFieldNumber = 2;
    private string messageFormat_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string MessageFormat {
      get { return messageFormat_; }
      set {
        messageFormat_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as IOFormat);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(IOFormat other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (InternalName != other.InternalName) return false;
      if (MessageFormat != other.MessageFormat) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (InternalName.Length != 0) hash ^= InternalName.GetHashCode();
      if (MessageFormat.Length != 0) hash ^= MessageFormat.GetHashCode();
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
      if (InternalName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(InternalName);
      }
      if (MessageFormat.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(MessageFormat);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (InternalName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(InternalName);
      }
      if (MessageFormat.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(MessageFormat);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(IOFormat other) {
      if (other == null) {
        return;
      }
      if (other.InternalName.Length != 0) {
        InternalName = other.InternalName;
      }
      if (other.MessageFormat.Length != 0) {
        MessageFormat = other.MessageFormat;
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
            InternalName = input.ReadString();
            break;
          }
          case 18: {
            MessageFormat = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
