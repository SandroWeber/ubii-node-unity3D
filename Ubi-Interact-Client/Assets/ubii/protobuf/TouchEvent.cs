// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto/topicData/topicDataRecord/dataStructure/touchEvent.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ubii.DataStructure {

  /// <summary>Holder for reflection information generated from proto/topicData/topicDataRecord/dataStructure/touchEvent.proto</summary>
  public static partial class TouchEventReflection {

    #region Descriptor
    /// <summary>File descriptor for proto/topicData/topicDataRecord/dataStructure/touchEvent.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TouchEventReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cj5wcm90by90b3BpY0RhdGEvdG9waWNEYXRhUmVjb3JkL2RhdGFTdHJ1Y3R1",
            "cmUvdG91Y2hFdmVudC5wcm90bxISdWJpaS5kYXRhU3RydWN0dXJlGkNwcm90",
            "by90b3BpY0RhdGEvdG9waWNEYXRhUmVjb3JkL2RhdGFTdHJ1Y3R1cmUvYnV0",
            "dG9uRXZlbnRUeXBlLnByb3RvGjtwcm90by90b3BpY0RhdGEvdG9waWNEYXRh",
            "UmVjb3JkL2RhdGFTdHJ1Y3R1cmUvdmVjdG9yMi5wcm90byLVAQoKVG91Y2hF",
            "dmVudBI7CgR0eXBlGAEgASgOMi0udWJpaS5kYXRhU3RydWN0dXJlLlRvdWNo",
            "RXZlbnQuVG91Y2hFdmVudFR5cGUSLQoIcG9zaXRpb24YAiABKAsyGy51Ymlp",
            "LmRhdGFTdHJ1Y3R1cmUuVmVjdG9yMhIKCgJpZBgDIAEoCRINCgVmb3JjZRgE",
            "IAEoAiJACg5Ub3VjaEV2ZW50VHlwZRIPCgtUT1VDSF9TVEFSVBAAEg4KClRP",
            "VUNIX01PVkUQARINCglUT1VDSF9FTkQQAiJCCg5Ub3VjaEV2ZW50TGlzdBIw",
            "CghlbGVtZW50cxgBIAMoCzIeLnViaWkuZGF0YVN0cnVjdHVyZS5Ub3VjaEV2",
            "ZW50YgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Ubii.DataStructure.ButtonEventTypeReflection.Descriptor, global::Ubii.DataStructure.Vector2Reflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.DataStructure.TouchEvent), global::Ubii.DataStructure.TouchEvent.Parser, new[]{ "Type", "Position", "Id", "Force" }, null, new[]{ typeof(global::Ubii.DataStructure.TouchEvent.Types.TouchEventType) }, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.DataStructure.TouchEventList), global::Ubii.DataStructure.TouchEventList.Parser, new[]{ "Elements" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class TouchEvent : pb::IMessage<TouchEvent> {
    private static readonly pb::MessageParser<TouchEvent> _parser = new pb::MessageParser<TouchEvent>(() => new TouchEvent());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TouchEvent> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.DataStructure.TouchEventReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TouchEvent() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TouchEvent(TouchEvent other) : this() {
      type_ = other.type_;
      position_ = other.position_ != null ? other.position_.Clone() : null;
      id_ = other.id_;
      force_ = other.force_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TouchEvent Clone() {
      return new TouchEvent(this);
    }

    /// <summary>Field number for the "type" field.</summary>
    public const int TypeFieldNumber = 1;
    private global::Ubii.DataStructure.TouchEvent.Types.TouchEventType type_ = global::Ubii.DataStructure.TouchEvent.Types.TouchEventType.TouchStart;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.DataStructure.TouchEvent.Types.TouchEventType Type {
      get { return type_; }
      set {
        type_ = value;
      }
    }

    /// <summary>Field number for the "position" field.</summary>
    public const int PositionFieldNumber = 2;
    private global::Ubii.DataStructure.Vector2 position_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.DataStructure.Vector2 Position {
      get { return position_; }
      set {
        position_ = value;
      }
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 3;
    private string id_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Id {
      get { return id_; }
      set {
        id_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "force" field.</summary>
    public const int ForceFieldNumber = 4;
    private float force_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Force {
      get { return force_; }
      set {
        force_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TouchEvent);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TouchEvent other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Type != other.Type) return false;
      if (!object.Equals(Position, other.Position)) return false;
      if (Id != other.Id) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Force, other.Force)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Type != global::Ubii.DataStructure.TouchEvent.Types.TouchEventType.TouchStart) hash ^= Type.GetHashCode();
      if (position_ != null) hash ^= Position.GetHashCode();
      if (Id.Length != 0) hash ^= Id.GetHashCode();
      if (Force != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Force);
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
      if (Type != global::Ubii.DataStructure.TouchEvent.Types.TouchEventType.TouchStart) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Type);
      }
      if (position_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Position);
      }
      if (Id.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Id);
      }
      if (Force != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(Force);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Type != global::Ubii.DataStructure.TouchEvent.Types.TouchEventType.TouchStart) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Type);
      }
      if (position_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Position);
      }
      if (Id.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Id);
      }
      if (Force != 0F) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TouchEvent other) {
      if (other == null) {
        return;
      }
      if (other.Type != global::Ubii.DataStructure.TouchEvent.Types.TouchEventType.TouchStart) {
        Type = other.Type;
      }
      if (other.position_ != null) {
        if (position_ == null) {
          Position = new global::Ubii.DataStructure.Vector2();
        }
        Position.MergeFrom(other.Position);
      }
      if (other.Id.Length != 0) {
        Id = other.Id;
      }
      if (other.Force != 0F) {
        Force = other.Force;
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
          case 8: {
            Type = (global::Ubii.DataStructure.TouchEvent.Types.TouchEventType) input.ReadEnum();
            break;
          }
          case 18: {
            if (position_ == null) {
              Position = new global::Ubii.DataStructure.Vector2();
            }
            input.ReadMessage(Position);
            break;
          }
          case 26: {
            Id = input.ReadString();
            break;
          }
          case 37: {
            Force = input.ReadFloat();
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the TouchEvent message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      public enum TouchEventType {
        [pbr::OriginalName("TOUCH_START")] TouchStart = 0,
        [pbr::OriginalName("TOUCH_MOVE")] TouchMove = 1,
        [pbr::OriginalName("TOUCH_END")] TouchEnd = 2,
      }

    }
    #endregion

  }

  public sealed partial class TouchEventList : pb::IMessage<TouchEventList> {
    private static readonly pb::MessageParser<TouchEventList> _parser = new pb::MessageParser<TouchEventList>(() => new TouchEventList());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TouchEventList> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.DataStructure.TouchEventReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TouchEventList() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TouchEventList(TouchEventList other) : this() {
      elements_ = other.elements_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TouchEventList Clone() {
      return new TouchEventList(this);
    }

    /// <summary>Field number for the "elements" field.</summary>
    public const int ElementsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Ubii.DataStructure.TouchEvent> _repeated_elements_codec
        = pb::FieldCodec.ForMessage(10, global::Ubii.DataStructure.TouchEvent.Parser);
    private readonly pbc::RepeatedField<global::Ubii.DataStructure.TouchEvent> elements_ = new pbc::RepeatedField<global::Ubii.DataStructure.TouchEvent>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Ubii.DataStructure.TouchEvent> Elements {
      get { return elements_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TouchEventList);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TouchEventList other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!elements_.Equals(other.elements_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= elements_.GetHashCode();
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
      elements_.WriteTo(output, _repeated_elements_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += elements_.CalculateSize(_repeated_elements_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TouchEventList other) {
      if (other == null) {
        return;
      }
      elements_.Add(other.elements_);
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
            elements_.AddEntriesFrom(input, _repeated_elements_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
