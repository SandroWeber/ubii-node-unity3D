// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto/sessions/interactionInputMapping.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ubii.Sessions {

  /// <summary>Holder for reflection information generated from proto/sessions/interactionInputMapping.proto</summary>
  public static partial class InteractionInputMappingReflection {

    #region Descriptor
    /// <summary>File descriptor for proto/sessions/interactionInputMapping.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static InteractionInputMappingReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cixwcm90by9zZXNzaW9ucy9pbnRlcmFjdGlvbklucHV0TWFwcGluZy5wcm90",
            "bxINdWJpaS5zZXNzaW9ucxoccHJvdG8vZGV2aWNlcy90b3BpY011eC5wcm90",
            "byJ1ChdJbnRlcmFjdGlvbklucHV0TWFwcGluZxIMCgRuYW1lGAEgASgJEg8K",
            "BXRvcGljGAIgASgJSAASKwoJdG9waWNfbXV4GAMgASgLMhYudWJpaS5kZXZp",
            "Y2VzLlRvcGljTXV4SABCDgoMdG9waWNfc291cmNlIlcKG0ludGVyYWN0aW9u",
            "SW5wdXRNYXBwaW5nTGlzdBI4CghlbGVtZW50cxgBIAMoCzImLnViaWkuc2Vz",
            "c2lvbnMuSW50ZXJhY3Rpb25JbnB1dE1hcHBpbmdiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Ubii.Devices.TopicMuxReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.Sessions.InteractionInputMapping), global::Ubii.Sessions.InteractionInputMapping.Parser, new[]{ "Name", "Topic", "TopicMux" }, new[]{ "TopicSource" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.Sessions.InteractionInputMappingList), global::Ubii.Sessions.InteractionInputMappingList.Parser, new[]{ "Elements" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class InteractionInputMapping : pb::IMessage<InteractionInputMapping> {
    private static readonly pb::MessageParser<InteractionInputMapping> _parser = new pb::MessageParser<InteractionInputMapping>(() => new InteractionInputMapping());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<InteractionInputMapping> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.Sessions.InteractionInputMappingReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InteractionInputMapping() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InteractionInputMapping(InteractionInputMapping other) : this() {
      name_ = other.name_;
      switch (other.TopicSourceCase) {
        case TopicSourceOneofCase.Topic:
          Topic = other.Topic;
          break;
        case TopicSourceOneofCase.TopicMux:
          TopicMux = other.TopicMux.Clone();
          break;
      }

      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InteractionInputMapping Clone() {
      return new InteractionInputMapping(this);
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "topic" field.</summary>
    public const int TopicFieldNumber = 2;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Topic {
      get { return topicSourceCase_ == TopicSourceOneofCase.Topic ? (string) topicSource_ : ""; }
      set {
        topicSource_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
        topicSourceCase_ = TopicSourceOneofCase.Topic;
      }
    }

    /// <summary>Field number for the "topic_mux" field.</summary>
    public const int TopicMuxFieldNumber = 3;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.Devices.TopicMux TopicMux {
      get { return topicSourceCase_ == TopicSourceOneofCase.TopicMux ? (global::Ubii.Devices.TopicMux) topicSource_ : null; }
      set {
        topicSource_ = value;
        topicSourceCase_ = value == null ? TopicSourceOneofCase.None : TopicSourceOneofCase.TopicMux;
      }
    }

    private object topicSource_;
    /// <summary>Enum of possible cases for the "topic_source" oneof.</summary>
    public enum TopicSourceOneofCase {
      None = 0,
      Topic = 2,
      TopicMux = 3,
    }
    private TopicSourceOneofCase topicSourceCase_ = TopicSourceOneofCase.None;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TopicSourceOneofCase TopicSourceCase {
      get { return topicSourceCase_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearTopicSource() {
      topicSourceCase_ = TopicSourceOneofCase.None;
      topicSource_ = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as InteractionInputMapping);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(InteractionInputMapping other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if (Topic != other.Topic) return false;
      if (!object.Equals(TopicMux, other.TopicMux)) return false;
      if (TopicSourceCase != other.TopicSourceCase) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (topicSourceCase_ == TopicSourceOneofCase.Topic) hash ^= Topic.GetHashCode();
      if (topicSourceCase_ == TopicSourceOneofCase.TopicMux) hash ^= TopicMux.GetHashCode();
      hash ^= (int) topicSourceCase_;
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
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (topicSourceCase_ == TopicSourceOneofCase.Topic) {
        output.WriteRawTag(18);
        output.WriteString(Topic);
      }
      if (topicSourceCase_ == TopicSourceOneofCase.TopicMux) {
        output.WriteRawTag(26);
        output.WriteMessage(TopicMux);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (topicSourceCase_ == TopicSourceOneofCase.Topic) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Topic);
      }
      if (topicSourceCase_ == TopicSourceOneofCase.TopicMux) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(TopicMux);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(InteractionInputMapping other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      switch (other.TopicSourceCase) {
        case TopicSourceOneofCase.Topic:
          Topic = other.Topic;
          break;
        case TopicSourceOneofCase.TopicMux:
          if (TopicMux == null) {
            TopicMux = new global::Ubii.Devices.TopicMux();
          }
          TopicMux.MergeFrom(other.TopicMux);
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
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 18: {
            Topic = input.ReadString();
            break;
          }
          case 26: {
            global::Ubii.Devices.TopicMux subBuilder = new global::Ubii.Devices.TopicMux();
            if (topicSourceCase_ == TopicSourceOneofCase.TopicMux) {
              subBuilder.MergeFrom(TopicMux);
            }
            input.ReadMessage(subBuilder);
            TopicMux = subBuilder;
            break;
          }
        }
      }
    }

  }

  public sealed partial class InteractionInputMappingList : pb::IMessage<InteractionInputMappingList> {
    private static readonly pb::MessageParser<InteractionInputMappingList> _parser = new pb::MessageParser<InteractionInputMappingList>(() => new InteractionInputMappingList());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<InteractionInputMappingList> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.Sessions.InteractionInputMappingReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InteractionInputMappingList() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InteractionInputMappingList(InteractionInputMappingList other) : this() {
      elements_ = other.elements_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InteractionInputMappingList Clone() {
      return new InteractionInputMappingList(this);
    }

    /// <summary>Field number for the "elements" field.</summary>
    public const int ElementsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Ubii.Sessions.InteractionInputMapping> _repeated_elements_codec
        = pb::FieldCodec.ForMessage(10, global::Ubii.Sessions.InteractionInputMapping.Parser);
    private readonly pbc::RepeatedField<global::Ubii.Sessions.InteractionInputMapping> elements_ = new pbc::RepeatedField<global::Ubii.Sessions.InteractionInputMapping>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Ubii.Sessions.InteractionInputMapping> Elements {
      get { return elements_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as InteractionInputMappingList);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(InteractionInputMappingList other) {
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
    public void MergeFrom(InteractionInputMappingList other) {
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
