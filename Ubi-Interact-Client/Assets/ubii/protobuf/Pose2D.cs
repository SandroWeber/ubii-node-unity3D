// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto/dataStructure/pose2d.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ubii.DataStructure {

  /// <summary>Holder for reflection information generated from proto/dataStructure/pose2d.proto</summary>
  public static partial class Pose2DReflection {

    #region Descriptor
    /// <summary>File descriptor for proto/dataStructure/pose2d.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static Pose2DReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiBwcm90by9kYXRhU3RydWN0dXJlL3Bvc2UyZC5wcm90bxISdWJpaS5kYXRh",
            "U3RydWN0dXJlGiFwcm90by9kYXRhU3RydWN0dXJlL3ZlY3RvcjIucHJvdG8i",
            "iQEKBlBvc2UyRBItCghwb3NpdGlvbhgBIAEoCzIbLnViaWkuZGF0YVN0cnVj",
            "dHVyZS5WZWN0b3IyEjAKCWRpcmVjdGlvbhgCIAEoCzIbLnViaWkuZGF0YVN0",
            "cnVjdHVyZS5WZWN0b3IySAASDwoFYW5nbGUYAyABKAJIAEINCgtvcmllbnRh",
            "dGlvbmIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Ubii.DataStructure.Vector2Reflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ubii.DataStructure.Pose2D), global::Ubii.DataStructure.Pose2D.Parser, new[]{ "Position", "Direction", "Angle" }, new[]{ "Orientation" }, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Pose2D : pb::IMessage<Pose2D> {
    private static readonly pb::MessageParser<Pose2D> _parser = new pb::MessageParser<Pose2D>(() => new Pose2D());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Pose2D> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ubii.DataStructure.Pose2DReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Pose2D() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Pose2D(Pose2D other) : this() {
      position_ = other.position_ != null ? other.position_.Clone() : null;
      switch (other.OrientationCase) {
        case OrientationOneofCase.Direction:
          Direction = other.Direction.Clone();
          break;
        case OrientationOneofCase.Angle:
          Angle = other.Angle;
          break;
      }

      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Pose2D Clone() {
      return new Pose2D(this);
    }

    /// <summary>Field number for the "position" field.</summary>
    public const int PositionFieldNumber = 1;
    private global::Ubii.DataStructure.Vector2 position_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.DataStructure.Vector2 Position {
      get { return position_; }
      set {
        position_ = value;
      }
    }

    /// <summary>Field number for the "direction" field.</summary>
    public const int DirectionFieldNumber = 2;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Ubii.DataStructure.Vector2 Direction {
      get { return orientationCase_ == OrientationOneofCase.Direction ? (global::Ubii.DataStructure.Vector2) orientation_ : null; }
      set {
        orientation_ = value;
        orientationCase_ = value == null ? OrientationOneofCase.None : OrientationOneofCase.Direction;
      }
    }

    /// <summary>Field number for the "angle" field.</summary>
    public const int AngleFieldNumber = 3;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Angle {
      get { return orientationCase_ == OrientationOneofCase.Angle ? (float) orientation_ : 0F; }
      set {
        orientation_ = value;
        orientationCase_ = OrientationOneofCase.Angle;
      }
    }

    private object orientation_;
    /// <summary>Enum of possible cases for the "orientation" oneof.</summary>
    public enum OrientationOneofCase {
      None = 0,
      Direction = 2,
      Angle = 3,
    }
    private OrientationOneofCase orientationCase_ = OrientationOneofCase.None;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OrientationOneofCase OrientationCase {
      get { return orientationCase_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearOrientation() {
      orientationCase_ = OrientationOneofCase.None;
      orientation_ = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Pose2D);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Pose2D other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Position, other.Position)) return false;
      if (!object.Equals(Direction, other.Direction)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Angle, other.Angle)) return false;
      if (OrientationCase != other.OrientationCase) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (position_ != null) hash ^= Position.GetHashCode();
      if (orientationCase_ == OrientationOneofCase.Direction) hash ^= Direction.GetHashCode();
      if (orientationCase_ == OrientationOneofCase.Angle) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Angle);
      hash ^= (int) orientationCase_;
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
      if (position_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Position);
      }
      if (orientationCase_ == OrientationOneofCase.Direction) {
        output.WriteRawTag(18);
        output.WriteMessage(Direction);
      }
      if (orientationCase_ == OrientationOneofCase.Angle) {
        output.WriteRawTag(29);
        output.WriteFloat(Angle);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (position_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Position);
      }
      if (orientationCase_ == OrientationOneofCase.Direction) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Direction);
      }
      if (orientationCase_ == OrientationOneofCase.Angle) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Pose2D other) {
      if (other == null) {
        return;
      }
      if (other.position_ != null) {
        if (position_ == null) {
          position_ = new global::Ubii.DataStructure.Vector2();
        }
        Position.MergeFrom(other.Position);
      }
      switch (other.OrientationCase) {
        case OrientationOneofCase.Direction:
          if (Direction == null) {
            Direction = new global::Ubii.DataStructure.Vector2();
          }
          Direction.MergeFrom(other.Direction);
          break;
        case OrientationOneofCase.Angle:
          Angle = other.Angle;
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
            if (position_ == null) {
              position_ = new global::Ubii.DataStructure.Vector2();
            }
            input.ReadMessage(position_);
            break;
          }
          case 18: {
            global::Ubii.DataStructure.Vector2 subBuilder = new global::Ubii.DataStructure.Vector2();
            if (orientationCase_ == OrientationOneofCase.Direction) {
              subBuilder.MergeFrom(Direction);
            }
            input.ReadMessage(subBuilder);
            Direction = subBuilder;
            break;
          }
          case 29: {
            Angle = input.ReadFloat();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
