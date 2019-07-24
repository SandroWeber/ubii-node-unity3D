// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto/interactions/interaction.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ubii.Interactions {

  /// <summary>Holder for reflection information generated from proto/interactions/interaction.proto</summary>
  public static partial class InteractionReflection {

    #region Descriptor
    /// <summary>File descriptor for proto/interactions/interaction.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static InteractionReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiRwcm90by9pbnRlcmFjdGlvbnMvaW50ZXJhY3Rpb24ucHJvdG8SEXViaWku",
            "aW50ZXJhY3Rpb25zGiFwcm90by9pbnRlcmFjdGlvbnMvaW9Gb3JtYXQucHJv",
            "dG8iwQEKC0ludGVyYWN0aW9uEgoKAmlkGAEgASgJEgwKBG5hbWUYAiABKAkS",
            "GwoTcHJvY2Vzc2luZ19jYWxsYmFjaxgDIAEoCRIyCg1pbnB1dF9mb3JtYXRz",
            "GAQgAygLMhsudWJpaS5pbnRlcmFjdGlvbnMuSU9Gb3JtYXQSMwoOb3V0cHV0",
            "X2Zvcm1hdHMYBSADKAsyGy51YmlpLmludGVyYWN0aW9ucy5JT0Zvcm1hdBIS",
            "Cgpvbl9jcmVhdGVkGAYgASgJIkMKD0ludGVyYWN0aW9uTGlzdBIwCghlbGVt",
            "ZW50cxgBIAMoCzIeLnViaWkuaW50ZXJhY3Rpb25zLkludGVyYWN0aW9uYgZw",
            "cm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Ubii.Interactions.IoFormatReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.Interactions.Interaction), global::Ubii.Interactions.Interaction.Parser, new[]{ "Id", "Name", "ProcessingCallback", "InputFormats", "OutputFormats", "OnCreated" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.Interactions.InteractionList), global::Ubii.Interactions.InteractionList.Parser, new[]{ "Elements" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Interaction : pb::IMessage<Interaction> {
    private static readonly pb::MessageParser<Interaction> _parser = new pb::MessageParser<Interaction>(() => new Interaction());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Interaction> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.Interactions.InteractionReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Interaction() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Interaction(Interaction other) : this() {
      id_ = other.id_;
      name_ = other.name_;
      processingCallback_ = other.processingCallback_;
      inputFormats_ = other.inputFormats_.Clone();
      outputFormats_ = other.outputFormats_.Clone();
      onCreated_ = other.onCreated_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Interaction Clone() {
      return new Interaction(this);
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

    /// <summary>Field number for the "processing_callback" field.</summary>
    public const int ProcessingCallbackFieldNumber = 3;
    private string processingCallback_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ProcessingCallback {
      get { return processingCallback_; }
      set {
        processingCallback_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "input_formats" field.</summary>
    public const int InputFormatsFieldNumber = 4;
    private static readonly pb::FieldCodec<global::Ubii.Interactions.IOFormat> _repeated_inputFormats_codec
        = pb::FieldCodec.ForMessage(34, global::Ubii.Interactions.IOFormat.Parser);
    private readonly pbc::RepeatedField<global::Ubii.Interactions.IOFormat> inputFormats_ = new pbc::RepeatedField<global::Ubii.Interactions.IOFormat>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Ubii.Interactions.IOFormat> InputFormats {
      get { return inputFormats_; }
    }

    /// <summary>Field number for the "output_formats" field.</summary>
    public const int OutputFormatsFieldNumber = 5;
    private static readonly pb::FieldCodec<global::Ubii.Interactions.IOFormat> _repeated_outputFormats_codec
        = pb::FieldCodec.ForMessage(42, global::Ubii.Interactions.IOFormat.Parser);
    private readonly pbc::RepeatedField<global::Ubii.Interactions.IOFormat> outputFormats_ = new pbc::RepeatedField<global::Ubii.Interactions.IOFormat>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Ubii.Interactions.IOFormat> OutputFormats {
      get { return outputFormats_; }
    }

    /// <summary>Field number for the "on_created" field.</summary>
    public const int OnCreatedFieldNumber = 6;
    private string onCreated_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string OnCreated {
      get { return onCreated_; }
      set {
        onCreated_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Interaction);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Interaction other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Name != other.Name) return false;
      if (ProcessingCallback != other.ProcessingCallback) return false;
      if(!inputFormats_.Equals(other.inputFormats_)) return false;
      if(!outputFormats_.Equals(other.outputFormats_)) return false;
      if (OnCreated != other.OnCreated) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id.Length != 0) hash ^= Id.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (ProcessingCallback.Length != 0) hash ^= ProcessingCallback.GetHashCode();
      hash ^= inputFormats_.GetHashCode();
      hash ^= outputFormats_.GetHashCode();
      if (OnCreated.Length != 0) hash ^= OnCreated.GetHashCode();
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
      if (ProcessingCallback.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(ProcessingCallback);
      }
      inputFormats_.WriteTo(output, _repeated_inputFormats_codec);
      outputFormats_.WriteTo(output, _repeated_outputFormats_codec);
      if (OnCreated.Length != 0) {
        output.WriteRawTag(50);
        output.WriteString(OnCreated);
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
      if (ProcessingCallback.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ProcessingCallback);
      }
      size += inputFormats_.CalculateSize(_repeated_inputFormats_codec);
      size += outputFormats_.CalculateSize(_repeated_outputFormats_codec);
      if (OnCreated.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(OnCreated);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Interaction other) {
      if (other == null) {
        return;
      }
      if (other.Id.Length != 0) {
        Id = other.Id;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.ProcessingCallback.Length != 0) {
        ProcessingCallback = other.ProcessingCallback;
      }
      inputFormats_.Add(other.inputFormats_);
      outputFormats_.Add(other.outputFormats_);
      if (other.OnCreated.Length != 0) {
        OnCreated = other.OnCreated;
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
            ProcessingCallback = input.ReadString();
            break;
          }
          case 34: {
            inputFormats_.AddEntriesFrom(input, _repeated_inputFormats_codec);
            break;
          }
          case 42: {
            outputFormats_.AddEntriesFrom(input, _repeated_outputFormats_codec);
            break;
          }
          case 50: {
            OnCreated = input.ReadString();
            break;
          }
        }
      }
    }

  }

  public sealed partial class InteractionList : pb::IMessage<InteractionList> {
    private static readonly pb::MessageParser<InteractionList> _parser = new pb::MessageParser<InteractionList>(() => new InteractionList());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<InteractionList> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.Interactions.InteractionReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InteractionList() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InteractionList(InteractionList other) : this() {
      elements_ = other.elements_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InteractionList Clone() {
      return new InteractionList(this);
    }

    /// <summary>Field number for the "elements" field.</summary>
    public const int ElementsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Ubii.Interactions.Interaction> _repeated_elements_codec
        = pb::FieldCodec.ForMessage(10, global::Ubii.Interactions.Interaction.Parser);
    private readonly pbc::RepeatedField<global::Ubii.Interactions.Interaction> elements_ = new pbc::RepeatedField<global::Ubii.Interactions.Interaction>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Ubii.Interactions.Interaction> Elements {
      get { return elements_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as InteractionList);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(InteractionList other) {
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
    public void MergeFrom(InteractionList other) {
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
