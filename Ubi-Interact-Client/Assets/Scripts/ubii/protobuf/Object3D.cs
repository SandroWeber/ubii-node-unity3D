// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto/topicData/topicDataRecord/dataStructure/object3d.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ubii.DataStructure {

  /// <summary>Holder for reflection information generated from proto/topicData/topicDataRecord/dataStructure/object3d.proto</summary>
  public static partial class Object3DReflection {

    #region Descriptor
    /// <summary>File descriptor for proto/topicData/topicDataRecord/dataStructure/object3d.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static Object3DReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cjxwcm90by90b3BpY0RhdGEvdG9waWNEYXRhUmVjb3JkL2RhdGFTdHJ1Y3R1",
            "cmUvb2JqZWN0M2QucHJvdG8SEnViaWkuZGF0YVN0cnVjdHVyZRo6cHJvdG8v",
            "dG9waWNEYXRhL3RvcGljRGF0YVJlY29yZC9kYXRhU3RydWN0dXJlL3Bvc2Uz",
            "ZC5wcm90bxo7cHJvdG8vdG9waWNEYXRhL3RvcGljRGF0YVJlY29yZC9kYXRh",
            "U3RydWN0dXJlL3ZlY3RvcjMucHJvdG8igwEKCE9iamVjdDNEEgoKAmlkGAEg",
            "ASgJEigKBHBvc2UYAiABKAsyGi51YmlpLmRhdGFTdHJ1Y3R1cmUuUG9zZTNE",
            "EikKBHNpemUYAyABKAsyGy51YmlpLmRhdGFTdHJ1Y3R1cmUuVmVjdG9yMxIW",
            "Cg51c2VyX2RhdGFfanNvbhgEIAEoCSI+CgxPYmplY3QzRExpc3QSLgoIZWxl",
            "bWVudHMYASADKAsyHC51YmlpLmRhdGFTdHJ1Y3R1cmUuT2JqZWN0M0RiBnBy",
            "b3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Ubii.DataStructure.Pose3DReflection.Descriptor, global::Ubii.DataStructure.Vector3Reflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.DataStructure.Object3D), global::Ubii.DataStructure.Object3D.Parser, new[]{ "Id", "Pose", "Size", "UserDataJson" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.DataStructure.Object3DList), global::Ubii.DataStructure.Object3DList.Parser, new[]{ "Elements" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Object3D : pb::IMessage<Object3D> {
    private static readonly pb::MessageParser<Object3D> _parser = new pb::MessageParser<Object3D>(() => new Object3D());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Object3D> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.DataStructure.Object3DReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Object3D() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Object3D(Object3D other) : this() {
      id_ = other.id_;
      pose_ = other.pose_ != null ? other.pose_.Clone() : null;
      size_ = other.size_ != null ? other.size_.Clone() : null;
      userDataJson_ = other.userDataJson_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Object3D Clone() {
      return new Object3D(this);
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

    /// <summary>Field number for the "pose" field.</summary>
    public const int PoseFieldNumber = 2;
    private global::Ubii.DataStructure.Pose3D pose_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.DataStructure.Pose3D Pose {
      get { return pose_; }
      set {
        pose_ = value;
      }
    }

    /// <summary>Field number for the "size" field.</summary>
    public const int SizeFieldNumber = 3;
    private global::Ubii.DataStructure.Vector3 size_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.DataStructure.Vector3 Size {
      get { return size_; }
      set {
        size_ = value;
      }
    }

    /// <summary>Field number for the "user_data_json" field.</summary>
    public const int UserDataJsonFieldNumber = 4;
    private string userDataJson_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string UserDataJson {
      get { return userDataJson_; }
      set {
        userDataJson_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Object3D);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Object3D other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (!object.Equals(Pose, other.Pose)) return false;
      if (!object.Equals(Size, other.Size)) return false;
      if (UserDataJson != other.UserDataJson) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id.Length != 0) hash ^= Id.GetHashCode();
      if (pose_ != null) hash ^= Pose.GetHashCode();
      if (size_ != null) hash ^= Size.GetHashCode();
      if (UserDataJson.Length != 0) hash ^= UserDataJson.GetHashCode();
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
      if (pose_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Pose);
      }
      if (size_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Size);
      }
      if (UserDataJson.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(UserDataJson);
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
      if (pose_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Pose);
      }
      if (size_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Size);
      }
      if (UserDataJson.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(UserDataJson);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Object3D other) {
      if (other == null) {
        return;
      }
      if (other.Id.Length != 0) {
        Id = other.Id;
      }
      if (other.pose_ != null) {
        if (pose_ == null) {
          pose_ = new global::Ubii.DataStructure.Pose3D();
        }
        Pose.MergeFrom(other.Pose);
      }
      if (other.size_ != null) {
        if (size_ == null) {
          size_ = new global::Ubii.DataStructure.Vector3();
        }
        Size.MergeFrom(other.Size);
      }
      if (other.UserDataJson.Length != 0) {
        UserDataJson = other.UserDataJson;
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
            if (pose_ == null) {
              pose_ = new global::Ubii.DataStructure.Pose3D();
            }
            input.ReadMessage(pose_);
            break;
          }
          case 26: {
            if (size_ == null) {
              size_ = new global::Ubii.DataStructure.Vector3();
            }
            input.ReadMessage(size_);
            break;
          }
          case 34: {
            UserDataJson = input.ReadString();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Object3DList : pb::IMessage<Object3DList> {
    private static readonly pb::MessageParser<Object3DList> _parser = new pb::MessageParser<Object3DList>(() => new Object3DList());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Object3DList> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.DataStructure.Object3DReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Object3DList() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Object3DList(Object3DList other) : this() {
      elements_ = other.elements_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Object3DList Clone() {
      return new Object3DList(this);
    }

    /// <summary>Field number for the "elements" field.</summary>
    public const int ElementsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Ubii.DataStructure.Object3D> _repeated_elements_codec
        = pb::FieldCodec.ForMessage(10, global::Ubii.DataStructure.Object3D.Parser);
    private readonly pbc::RepeatedField<global::Ubii.DataStructure.Object3D> elements_ = new pbc::RepeatedField<global::Ubii.DataStructure.Object3D>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Ubii.DataStructure.Object3D> Elements {
      get { return elements_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Object3DList);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Object3DList other) {
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
    public void MergeFrom(Object3DList other) {
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
