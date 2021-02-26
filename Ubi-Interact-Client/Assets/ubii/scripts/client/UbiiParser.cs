using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ubii.UtilityFunctions.Parser
{
	using Ubii.TopicData;
	public static class UbiiParser
	{
		#region Send Functions
        public static TopicData UnityToProto(string topic, bool b)
        {
            TopicData td = new TopicData
            {
                TopicDataRecord = new TopicDataRecord
                {
                    Topic = topic,
                    Bool = b
                },
            };

            return td;
        }

		public static TopicData UnityToProto(string topic, double d)
		{
			TopicData td = new TopicData
			{
				TopicDataRecord = new TopicDataRecord
				{
					Topic = topic,
					Double = d
				},
			};

			return td;
		}

		public static TopicData UnityToProto(string topic, Vector2 v)
		{
			TopicData td = new TopicData
			{
				TopicDataRecord = new TopicDataRecord
				{
					Topic = topic,
					Vector2 = new Ubii.DataStructure.Vector2 { X = v.x, Y = v.y }
				},
			};

			return td;
		}

		public static TopicData UnityToProto(string topic, Vector3 v)
		{
			TopicData td = new TopicData
			{
				TopicDataRecord = new TopicDataRecord
				{
					Topic = topic,
					Vector3 = new Ubii.DataStructure.Vector3 { X = v.x, Y = v.y, Z = v.z }
				},
			};

			return td;
		}

		public static TopicData UnityToProto(string topic, Vector4 v)
		{
			TopicData td = new TopicData
			{
				TopicDataRecord = new TopicDataRecord
				{
					Topic = topic,
					Vector4 = new Ubii.DataStructure.Vector4 { X = v.x, Y = v.y, Z = v.z, W = v.w }
				},
			};

			return td;
		}

		public static TopicData UnityToProto(string topic, Quaternion q)
		{
			TopicData td = new TopicData
			{

				TopicDataRecord = new TopicDataRecord
				{
					Topic = topic,
					Quaternion = new Ubii.DataStructure.Quaternion { X = q.x, Y = q.y, Z = q.z, W = q.w }
				},
			};

			return td;
		}

		public static TopicData UnityToProto(string topic, Color c)
		{
			TopicData td = new TopicData
			{

				TopicDataRecord = new TopicDataRecord
				{
					Topic = topic,
					Color = new Ubii.DataStructure.Color { A = c.a, B = c.b, G = c.g, R = c.r }
				},
			};

			return td;
		}

		public static TopicData UnityToProto(string topic, string s)
		{
			TopicData td = new TopicData
			{
				TopicDataRecord = new TopicDataRecord
				{
					Topic = topic,
					String = s
				},
			};

			return td;
		}

		public static TopicData UnityToProto(string topic, Matrix4x4 m4x4)
		{
			TopicData td = new TopicData
			{
				TopicDataRecord = new TopicDataRecord
				{
					Topic = topic,
					Bool = true,
					Matrix4X4 = new Ubii.DataStructure.Matrix4x4
					{
						M00 = m4x4.m00,
						M01 = m4x4.m01,
						M02 = m4x4.m02,
						M03 = m4x4.m03,
						M10 = m4x4.m10,
						M11 = m4x4.m11,
						M12 = m4x4.m12,
						M13 = m4x4.m13,
						M20 = m4x4.m20,
						M21 = m4x4.m21,
						M22 = m4x4.m21,
						M23 = m4x4.m23,
						M30 = m4x4.m30,
						M31 = m4x4.m31,
						M32 = m4x4.m32,
						M33 = m4x4.m33
					},
				}
			};
			return td;
		}
		#endregion
		#region Receive Functions
        public static float ProtoToUnity(double d)
        {
            return (float)d;
        }

        public static Vector2 ProtoToUnity(DataStructure.Vector2 v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }

        public static float TranslateToUnityFloat(double d)
		{
			return (float)d;
		}

		public static Vector3 ProtoToUnity(DataStructure.Vector3 v)
		{
			return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
		}

		public static Vector4 ProtoToUnity(DataStructure.Vector4 v)
		{
			return new Vector4((float)v.X, (float)v.Y, (float)v.Z, (float)v.W);
		}

		public static Quaternion ProtoToUnity(DataStructure.Quaternion q)
		{
			return new Quaternion((float)q.X, (float)q.Y, (float)q.Z, (float)q.W);
		}

		public static Color ProtoToUnity(DataStructure.Color c)
		{
			return new Color((float)c.R, (float)c.G, (float)c.B, (float)c.A);
		}
		#endregion
	}
}
