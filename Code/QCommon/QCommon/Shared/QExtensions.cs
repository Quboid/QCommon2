﻿using Colossal.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib
{
    internal static class QExtensions
    {
        public static string ToStringNoTrace(this Exception e)
        {
            StringBuilder stringBuilder = new(e.GetType().ToString());
            stringBuilder.Append(": ").Append(e.Message);
            return stringBuilder.ToString();
        }

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        public static quaternion Multiply(this quaternion a, quaternion b)
        {
            return math.normalize(math.mul(a, b));
        }

        public static quaternion Inverse(this quaternion q)
        {
            float num = q.value.x * q.value.x + q.value.y * q.value.y + q.value.z * q.value.z + q.value.w * q.value.w;
            float num2 = 1f / num;
            quaternion result = default;
            result.value.x = (0f - q.value.x) * num2;
            result.value.y = (0f - q.value.y) * num2;
            result.value.z = (0f - q.value.z) * num2;
            result.value.w = q.value.w * num2;
            return result;
        }

        public static string D(this Entity e)
        {
            return $"E{e.Index}.{e.Version}";
        }

        public static Bounds3 Expand(this Bounds3 b, float3 size)
        {
            return new Bounds3(
                b.min - size,
                b.max + size
            );
        }

        public static void Encapsulate(ref this Bounds3 a, Bounds3 b)
        {
            a.min.x = Math.Min(a.min.x, b.min.x);
            a.min.y = Math.Min(a.min.y, b.min.y);
            a.min.z = Math.Min(a.min.z, b.min.z);
            a.max.x = Math.Max(a.max.x, b.max.x);
            a.max.y = Math.Max(a.max.y, b.max.y);
            a.max.z = Math.Max(a.max.z, b.max.z);
        }

        public static float3 Center(this Bounds3 bounds)
        {
            float x = bounds.x.min + (bounds.x.max - bounds.x.min) / 2;
            float y = bounds.y.min + (bounds.y.max - bounds.y.min) / 2;
            float z = bounds.z.min + (bounds.z.max - bounds.z.min) / 2;
            //QLoggerStatic.Debug($"{bounds.x.min},{bounds.y.min},{bounds.z.min} - {bounds.x.max},{bounds.y.max},{bounds.z.max}\nCenter:{x},{y},{z}");
            return new float3(x, y, z);
        }

        //public static void SetInvalid(this float3 f)
        //{
        //    f.x = -9999.69f;
        //    f.y = -9999.69f;
        //    f.z = -9999.69f;
        //}

        //public static bool IsValid(this float3 f)
        //{
        //    if (f.x == -9999.69f && f.y == -9999.69f && f.z == -9999.69f) return false;
        //    return true;
        //}

        public static string D(this Game.Objects.Transform t)
        {
            return $"{t.m_Position.DX()}/{t.m_Rotation.Y():0.##}";
        }

        public static string D(this float3 f)
        {
            return $"{f.x:0.##},{f.z:0.##}";
        }

        public static string DX(this float3 f)
        {
            return $"{f.x:0.##},{f.y:0.##},{f.z:0.##}";
        }

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }


        public static float X(this Game.Objects.Transform transform)
        {
            return transform.m_Rotation.ToEulerDegrees().x;
        }

        public static float Y(this Game.Objects.Transform transform)
        {
            return ((Quaternion)transform.m_Rotation).eulerAngles.y;
            //return transform.m_Rotation.ToEulerDegrees().y;
        }

        public static float Z(this Game.Objects.Transform transform)
        {
            return transform.m_Rotation.ToEulerDegrees().z;
        }


        public static float X(this quaternion quat)
        {
            return quat.ToEulerDegrees().x;
        }

        public static float Y(this quaternion quat)
        {
            return quat.ToEulerDegrees().y;
        }

        public static float Z(this quaternion quat)
        {
            return quat.ToEulerDegrees().z;
        }

        public static float3 ToEulerDegrees(this quaternion quat)
        {
            float4 q1 = quat.value;

            float sqw = q1.w * q1.w;
            float sqx = q1.x * q1.x;
            float sqy = q1.y * q1.y;
            float sqz = q1.z * q1.z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = q1.x * q1.w - q1.y * q1.z;
            float3 v;

            if (test > 0.4995f * unit)
            { // north pole
                v.y = 2f * math.atan2(q1.y, q1.x);
                v.x = math.PI / 2;
                v.z = 0;
                return ClampDegreesAll(math.degrees(v));
            }
            if (test < -0.4995f * unit)
            { // south pole
                v.y = -2f * math.atan2(q1.y, q1.x);
                v.x = -math.PI / 2;
                v.z = 0;
                return ClampDegreesAll(math.degrees(v));
            }

            quaternion q3 = new(q1.w, q1.z, q1.x, q1.y);
            float4 q = q3.value;

            v.y = math.atan2(2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w));
            v.x = math.asin(2f * (q.x * q.z - q.w * q.y));
            v.z = math.atan2(2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z));

            return ClampDegreesAll(math.degrees(v));
        }

        static float3 ClampDegreesAll(float3 angles)
        {
            angles.x = ClampDegrees(angles.x);
            angles.y = ClampDegrees(angles.y);
            angles.z = ClampDegrees(angles.z);
            return angles;
        }

        static float ClampDegrees(float angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }
    }
}
