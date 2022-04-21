// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto/topicData/topicData.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ubii.TopicData {

  /// <summary>Holder for reflection information generated from proto/topicData/topicData.proto</summary>
  public static partial class TopicDataReflection {

    #region Descriptor
    /// <summary>File descriptor for proto/topicData/topicData.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TopicDataReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Ch9wcm90by90b3BpY0RhdGEvdG9waWNEYXRhLnByb3RvEg51YmlpLnRvcGlj",
            "RGF0YRolcHJvdG8vdG9waWNEYXRhL3RvcGljRGF0YVJlY29yZC5wcm90bxoZ",
            "cHJvdG8vZ2VuZXJhbC9lcnJvci5wcm90byK+AQoJVG9waWNEYXRhEjwKEXRv",
            "cGljX2RhdGFfcmVjb3JkGAIgASgLMh8udWJpaS50b3BpY0RhdGEuVG9waWNE",
            "YXRhUmVjb3JkSAASRQoWdG9waWNfZGF0YV9yZWNvcmRfbGlzdBgDIAEoCzIj",
            "LnViaWkudG9waWNEYXRhLlRvcGljRGF0YVJlY29yZExpc3RIABIkCgVlcnJv",
            "chgEIAEoCzITLnViaWkuZ2VuZXJhbC5FcnJvckgAQgYKBHR5cGViBnByb3Rv",
            "Mw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Ubii.TopicData.TopicDataRecordReflection.Descriptor, global::Ubii.General.ErrorReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.TopicData.TopicData), global::Ubii.TopicData.TopicData.Parser, new[]{ "TopicDataRecord", "TopicDataRecordList", "Error" }, new[]{ "Type" }, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class TopicData : pb::IMessage<TopicData> {
    private static readonly pb::MessageParser<TopicData> _parser = new pb::MessageParser<TopicData>(() => new TopicData());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TopicData> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.TopicData.TopicDataReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TopicData() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TopicData(TopicData other) : this() {
      switch (other.TypeCase) {
        case TypeOneofCase.TopicDataRecord:
          TopicDataRecord = other.TopicDataRecord.Clone();
          break;
        case TypeOneofCase.TopicDataRecordList:
          TopicDataRecordList = other.TopicDataRecordList.Clone();
          break;
        case TypeOneofCase.Error:
          Error = other.Error.Clone();
          break;
      }

      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TopicData Clone() {
      return new TopicData(this);
    }

    /// <summary>Field number for the "topic_data_record" field.</summary>
    public const int TopicDataRecordFieldNumber = 2;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.TopicData.TopicDataRecord TopicDataRecord {
      get { return typeCase_ == TypeOneofCase.TopicDataRecord ? (global::Ubii.TopicData.TopicDataRecord) type_ : null; }
      set {
        type_ = value;
        typeCase_ = value == null ? TypeOneofCase.None : TypeOneofCase.TopicDataRecord;
      }
    }

    /// <summary>Field number for the "topic_data_record_list" field.</summary>
    public const int TopicDataRecordListFieldNumber = 3;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.TopicData.TopicDataRecordList TopicDataRecordList {
      get { return typeCase_ == TypeOneofCase.TopicDataRecordList ? (global::Ubii.TopicData.TopicDataRecordList) type_ : null; }
      set {
        type_ = value;
        typeCase_ = value == null ? TypeOneofCase.None : TypeOneofCase.TopicDataRecordList;
      }
    }

    /// <summary>Field number for the "error" field.</summary>
    public const int ErrorFieldNumber = 4;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.General.Error Error {
      get { return typeCase_ == TypeOneofCase.Error ? (global::Ubii.General.Error) type_ : null; }
      set {
        type_ = value;
        typeCase_ = value == null ? TypeOneofCase.None : TypeOneofCase.Error;
      }
    }

    private object type_;
    /// <summary>Enum of possible cases for the "type" oneof.</summary>
    public enum TypeOneofCase {
      None = 0,
      TopicDataRecord = 2,
      TopicDataRecordList = 3,
      Error = 4,
    }
    private TypeOneofCase typeCase_ = TypeOneofCase.None;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TypeOneofCase TypeCase {
      get { return typeCase_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearType() {
      typeCase_ = TypeOneofCase.None;
      type_ = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TopicData);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TopicData other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(TopicDataRecord, other.TopicDataRecord)) return false;
      if (!object.Equals(TopicDataRecordList, other.TopicDataRecordList)) return false;
      if (!object.Equals(Error, other.Error)) return false;
      if (TypeCase != other.TypeCase) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (typeCase_ == TypeOneofCase.TopicDataRecord) hash ^= TopicDataRecord.GetHashCode();
      if (typeCase_ == TypeOneofCase.TopicDataRecordList) hash ^= TopicDataRecordList.GetHashCode();
      if (typeCase_ == TypeOneofCase.Error) hash ^= Error.GetHashCode();
      hash ^= (int) typeCase_;
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
      if (typeCase_ == TypeOneofCase.TopicDataRecord) {
        output.WriteRawTag(18);
        output.WriteMessage(TopicDataRecord);
      }
      if (typeCase_ == TypeOneofCase.TopicDataRecordList) {
        output.WriteRawTag(26);
        output.WriteMessage(TopicDataRecordList);
      }
      if (typeCase_ == TypeOneofCase.Error) {
        output.WriteRawTag(34);
        output.WriteMessage(Error);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (typeCase_ == TypeOneofCase.TopicDataRecord) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(TopicDataRecord);
      }
      if (typeCase_ == TypeOneofCase.TopicDataRecordList) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(TopicDataRecordList);
      }
      if (typeCase_ == TypeOneofCase.Error) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Error);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TopicData other) {
      if (other == null) {
        return;
      }
      switch (other.TypeCase) {
        case TypeOneofCase.TopicDataRecord:
          if (TopicDataRecord == null) {
            TopicDataRecord = new global::Ubii.TopicData.TopicDataRecord();
          }
          TopicDataRecord.MergeFrom(other.TopicDataRecord);
          break;
        case TypeOneofCase.TopicDataRecordList:
          if (TopicDataRecordList == null) {
            TopicDataRecordList = new global::Ubii.TopicData.TopicDataRecordList();
          }
          TopicDataRecordList.MergeFrom(other.TopicDataRecordList);
          break;
        case TypeOneofCase.Error:
          if (Error == null) {
            Error = new global::Ubii.General.Error();
          }
          Error.MergeFrom(other.Error);
          break;
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
          case 18: {
            global::Ubii.TopicData.TopicDataRecord subBuilder = new global::Ubii.TopicData.TopicDataRecord();
            if (typeCase_ == TypeOneofCase.TopicDataRecord) {
              subBuilder.MergeFrom(TopicDataRecord);
            }
            input.ReadMessage(subBuilder);
            TopicDataRecord = subBuilder;
            break;
          }
          case 26: {
            global::Ubii.TopicData.TopicDataRecordList subBuilder = new global::Ubii.TopicData.TopicDataRecordList();
            if (typeCase_ == TypeOneofCase.TopicDataRecordList) {
              subBuilder.MergeFrom(TopicDataRecordList);
            }
            input.ReadMessage(subBuilder);
            TopicDataRecordList = subBuilder;
            break;
          }
          case 34: {
            global::Ubii.General.Error subBuilder = new global::Ubii.General.Error();
            if (typeCase_ == TypeOneofCase.Error) {
              subBuilder.MergeFrom(Error);
            }
            input.ReadMessage(subBuilder);
            Error = subBuilder;
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
