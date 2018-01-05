/*
Copyright (c) 2006 - 2008 The Open Toolkit library.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Neutrino
{
    /// <summary>
    /// Represents a Quaternion.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TkQuaternion : IEquatable<TkQuaternion>
    {
        /// <summary>
        /// The X, Y and Z components of this instance.
        /// </summary>
        public TkVector3 Xyz;

        /// <summary>
        /// The W component of this instance.
        /// </summary>
        public float W;

        /// <summary>
        /// Construct a new Quaternion from TkVector and w components
        /// </summary>
        /// <param name="v">The TkVector part</param>
        /// <param name="w">The w part</param>
        public TkQuaternion(TkVector3 v, float w)
        {
            Xyz = v;
            W = w;
        }

        /// <summary>
        /// Construct a new Quaternion
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        /// <param name="z">The z component</param>
        /// <param name="w">The w component</param>
        public TkQuaternion(float x, float y, float z, float w)
            : this(new TkVector3(x, y, z), w)
        { }

        /// <summary>
        /// Construct a new Quaternion from given Euler angles
        /// </summary>
        /// <param name="pitch">The pitch (attitude), rotation around X axis</param>
        /// <param name="yaw">The yaw (heading), rotation around Y axis</param>
        /// <param name="roll">The roll (bank), rotation around Z axis</param>
        public TkQuaternion(float pitch, float yaw, float roll)
        {
            yaw *= 0.5f;
            pitch *= 0.5f;
            roll *= 0.5f;

            float c1 = (float)Math.Cos(yaw);
            float c2 = (float)Math.Cos(pitch);
            float c3 = (float)Math.Cos(roll);
            float s1 = (float)Math.Sin(yaw);
            float s2 = (float)Math.Sin(pitch);
            float s3 = (float)Math.Sin(roll);

            W = c1 * c2 * c3 - s1 * s2 * s3;
            Xyz.X = s1 * s2 * c3 + c1 * c2 * s3;
            Xyz.Y = s1 * c2 * c3 + c1 * s2 * s3;
            Xyz.Z = c1 * s2 * c3 - s1 * c2 * s3;
        }

        /// <summary>
        /// Construct a new Quaternion from given Euler angles
        /// </summary>
        /// <param name="eulerAngles">The euler angles as a TkVector3</param>
        public TkQuaternion(TkVector3 eulerAngles)
            : this(eulerAngles.X, eulerAngles.Y, eulerAngles.Z)
        { }

        /// <summary>
        /// Gets or sets the X component of this instance.
        /// </summary>
        [XmlIgnore]
        public float X { get { return Xyz.X; } set { Xyz.X = value; } }

        /// <summary>
        /// Gets or sets the Y component of this instance.
        /// </summary>
        [XmlIgnore]
        public float Y { get { return Xyz.Y; } set { Xyz.Y = value; } }

        /// <summary>
        /// Gets or sets the Z component of this instance.
        /// </summary>
        [XmlIgnore]
        public float Z { get { return Xyz.Z; } set { Xyz.Z = value; } }

        /// <summary>
        /// Convert the current quaternion to axis angle representation
        /// </summary>
        /// <param name="axis">The resultant axis</param>
        /// <param name="angle">The resultant angle</param>
        public void ToAxisAngle(out TkVector3 axis, out float angle)
        {
            TkVector4 result = ToAxisAngle();
            axis = result.Xyz;
            angle = result.W;
        }

        /// <summary>
        /// Convert this instance to an axis-angle representation.
        /// </summary>
        /// <returns>A TkVector4 that is the axis-angle representation of this quaternion.</returns>
        public TkVector4 ToAxisAngle()
        {
            TkQuaternion q = this;
            if (Math.Abs(q.W) > 1.0f)
            {
                q.Normalize();
            }

            TkVector4 result = new TkVector4();

            result.W = 2.0f * (float)System.Math.Acos(q.W); // angle
            float den = (float)System.Math.Sqrt(1.0 - q.W * q.W);
            if (den > 0.0001f)
            {
                result.Xyz = q.Xyz / den;
            }
            else
            {
                // This occurs when the angle is zero.
                // Not a problem: just set an arbitrary normalized axis.
                result.Xyz = TkVector3.UnitX;
            }

            return result;
        }

        /// <summary>
        /// Gets the length (magnitude) of the quaternion.
        /// </summary>
        /// <seealso cref="LengthSquared"/>
        public float Length
        {
            get
            {
                return (float)System.Math.Sqrt(W * W + Xyz.LengthSquared);
            }
        }

        /// <summary>
        /// Gets the square of the quaternion length (magnitude).
        /// </summary>
        public float LengthSquared
        {
            get
            {
                return W * W + Xyz.LengthSquared;
            }
        }

        /// <summary>
        /// Returns a copy of the Quaternion scaled to unit length.
        /// </summary>
        public TkQuaternion Normalized()
        {
            TkQuaternion q = this;
            q.Normalize();
            return q;
        }

        /// <summary>
        /// Reverses the rotation angle of this Quaterniond.
        /// </summary>
        public void Invert()
        {
            W = -W;
        }

        /// <summary>
        /// Returns a copy of this Quaterniond with its rotation angle reversed.
        /// </summary>
        public TkQuaternion Inverted()
        {
            var q = this;
            q.Invert();
            return q;
        }

        /// <summary>
        /// Scales the Quaternion to unit length.
        /// </summary>
        public void Normalize()
        {
            float scale = 1.0f / this.Length;
            Xyz *= scale;
            W *= scale;
        }

        /// <summary>
        /// Inverts the TkVector3 component of this Quaternion.
        /// </summary>
        public void Conjugate()
        {
            Xyz = -Xyz;
        }

        /// <summary>
        /// Defines the identity quaternion.
        /// </summary>
        public static readonly TkQuaternion Identity = new TkQuaternion(0, 0, 0, 1);

        /// <summary>
        /// Add two quaternions
        /// </summary>
        /// <param name="left">The first operand</param>
        /// <param name="right">The second operand</param>
        /// <returns>The result of the addition</returns>
        public static TkQuaternion Add(TkQuaternion left, TkQuaternion right)
        {
            return new TkQuaternion(
                left.Xyz + right.Xyz,
                left.W + right.W);
        }

        /// <summary>
        /// Add two quaternions
        /// </summary>
        /// <param name="left">The first operand</param>
        /// <param name="right">The second operand</param>
        /// <param name="result">The result of the addition</param>
        public static void Add(ref TkQuaternion left, ref TkQuaternion right, out TkQuaternion result)
        {
            result = new TkQuaternion(
                left.Xyz + right.Xyz,
                left.W + right.W);
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>The result of the operation.</returns>
        public static TkQuaternion Sub(TkQuaternion left, TkQuaternion right)
        {
            return  new TkQuaternion(
                left.Xyz - right.Xyz,
                left.W - right.W);
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <param name="result">The result of the operation.</param>
        public static void Sub(ref TkQuaternion left, ref TkQuaternion right, out TkQuaternion result)
        {
            result = new TkQuaternion(
                left.Xyz - right.Xyz,
                left.W - right.W);
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static TkQuaternion Multiply(TkQuaternion left, TkQuaternion right)
        {
            TkQuaternion result;
            Multiply(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <param name="result">A new instance containing the result of the calculation.</param>
        public static void Multiply(ref TkQuaternion left, ref TkQuaternion right, out TkQuaternion result)
        {
            result = new TkQuaternion(
                right.W * left.Xyz + left.W * right.Xyz + TkVector3.Cross(left.Xyz, right.Xyz),
                left.W * right.W - TkVector3.Dot(left.Xyz, right.Xyz));
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <param name="result">A new instance containing the result of the calculation.</param>
        public static void Multiply(ref TkQuaternion quaternion, float scale, out TkQuaternion result)
        {
            result = new TkQuaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static TkQuaternion Multiply(TkQuaternion quaternion, float scale)
        {
            return new TkQuaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }

        /// <summary>
        /// Get the conjugate of the given quaternion
        /// </summary>
        /// <param name="q">The quaternion</param>
        /// <returns>The conjugate of the given quaternion</returns>
        public static TkQuaternion Conjugate(TkQuaternion q)
        {
            return new TkQuaternion(-q.Xyz, q.W);
        }

        /// <summary>
        /// Get the conjugate of the given quaternion
        /// </summary>
        /// <param name="q">The quaternion</param>
        /// <param name="result">The conjugate of the given quaternion</param>
        public static void Conjugate(ref TkQuaternion q, out TkQuaternion result)
        {
            result = new TkQuaternion(-q.Xyz, q.W);
        }

        /// <summary>
        /// Get the inverse of the given quaternion
        /// </summary>
        /// <param name="q">The quaternion to invert</param>
        /// <returns>The inverse of the given quaternion</returns>
        public static TkQuaternion Invert(TkQuaternion q)
        {
            TkQuaternion result;
            Invert(ref q, out result);
            return result;
        }

        /// <summary>
        /// Get the inverse of the given quaternion
        /// </summary>
        /// <param name="q">The quaternion to invert</param>
        /// <param name="result">The inverse of the given quaternion</param>
        public static void Invert(ref TkQuaternion q, out TkQuaternion result)
        {
            float lengthSq = q.LengthSquared;
            if (lengthSq != 0.0)
            {
                float i = 1.0f / lengthSq;
                result = new TkQuaternion(q.Xyz * -i, q.W * i);
            }
            else
            {
                result = q;
            }
        }

        /// <summary>
        /// Scale the given quaternion to unit length
        /// </summary>
        /// <param name="q">The quaternion to normalize</param>
        /// <returns>The normalized quaternion</returns>
        public static TkQuaternion Normalize(TkQuaternion q)
        {
            TkQuaternion result;
            Normalize(ref q, out result);
            return result;
        }

        /// <summary>
        /// Scale the given quaternion to unit length
        /// </summary>
        /// <param name="q">The quaternion to normalize</param>
        /// <param name="result">The normalized quaternion</param>
        public static void Normalize(ref TkQuaternion q, out TkQuaternion result)
        {
            float scale = 1.0f / q.Length;
            result = new TkQuaternion(q.Xyz * scale, q.W * scale);
        }

        /// <summary>
        /// Build a quaternion from the given axis and angle
        /// </summary>
        /// <param name="axis">The axis to rotate about</param>
        /// <param name="angle">The rotation angle in radians</param>
        /// <returns>The equivalent quaternion</returns>
        public static TkQuaternion FromAxisAngle(TkVector3 axis, float angle)
        {
            if (axis.LengthSquared == 0.0f)
            {
                return Identity;
            }

            TkQuaternion result = Identity;

            angle *= 0.5f;
            axis.Normalize();
            result.Xyz = axis * (float)System.Math.Sin(angle);
            result.W = (float)System.Math.Cos(angle);

            return Normalize(result);
        }

        /// <summary>
        /// Builds a Quaternion from the given euler angles
        /// </summary>
        /// <param name="pitch">The pitch (attitude), rotation around X axis</param>
        /// <param name="yaw">The yaw (heading), rotation around Y axis</param>
        /// <param name="roll">The roll (bank), rotation around Z axis</param>
        /// <returns></returns>
        public static TkQuaternion FromEulerAngles(float pitch, float yaw, float roll)
        {
            return new TkQuaternion(pitch, yaw, roll);
        }

        /// <summary>
        /// Builds a Quaternion from the given euler angles
        /// </summary>
        /// <param name="eulerAngles">The euler angles as a TkVector</param>
        /// <returns>The equivalent Quaternion</returns>
        public static TkQuaternion FromEulerAngles(TkVector3 eulerAngles)
        {
            return new TkQuaternion(eulerAngles);
        }

        /// <summary>
        /// Builds a Quaternion from the given euler angles
        /// </summary>
        /// <param name="eulerAngles">The euler angles a TkVector</param>
        /// <param name="result">The equivalent Quaternion</param>
        public static void FromEulerAngles(ref TkVector3 eulerAngles, out TkQuaternion result)
        {
            float c1 = (float)Math.Cos(eulerAngles.Y * 0.5f);
            float c2 = (float)Math.Cos(eulerAngles.X * 0.5f);
            float c3 = (float)Math.Cos(eulerAngles.Z * 0.5f);
            float s1 = (float)Math.Sin(eulerAngles.Y * 0.5f);
            float s2 = (float)Math.Sin(eulerAngles.X * 0.5f);
            float s3 = (float)Math.Sin(eulerAngles.Z * 0.5f);

            result.W = c1 * c2 * c3 - s1 * s2 * s3;
            result.Xyz.X = s1 * s2 * c3 + c1 * c2 * s3;
            result.Xyz.Y = s1 * c2 * c3 + c1 * s2 * s3;
            result.Xyz.Z = c1 * s2 * c3 - s1 * c2 * s3;
        }

        ///// <summary>
        ///// Builds a quaternion from the given rotation matrix
        ///// </summary>
        ///// <param name="matrix">A rotation matrix</param>
        ///// <returns>The equivalent quaternion</returns>
        //public static TkQuaternion FromMatrix(Matrix3 matrix)
        //{
        //    TkQuaternion result;
        //    FromMatrix(ref matrix, out result);
        //    return result;
        //}

        ///// <summary>
        ///// Builds a quaternion from the given rotation matrix
        ///// </summary>
        ///// <param name="matrix">A rotation matrix</param>
        ///// <param name="result">The equivalent quaternion</param>
        //public static void FromMatrix(ref Matrix3 matrix, out TkQuaternion result)
        //{
        //    float trace = matrix.Trace;

        //    if (trace > 0)
        //    {
        //        float s = (float)Math.Sqrt(trace + 1) * 2;
        //        float invS = 1f / s;

        //        result.W = s * 0.25f;
        //        result.Xyz.X = (matrix.Row2.Y - matrix.Row1.Z) * invS;
        //        result.Xyz.Y = (matrix.Row0.Z - matrix.Row2.X) * invS;
        //        result.Xyz.Z = (matrix.Row1.X - matrix.Row0.Y) * invS;
        //    }
        //    else
        //    {
        //        float m00 = matrix.Row0.X, m11 = matrix.Row1.Y, m22 = matrix.Row2.Z;

        //        if (m00 > m11 && m00 > m22)
        //        {
        //            float s = (float)Math.Sqrt(1 + m00 - m11 - m22) * 2;
        //            float invS = 1f / s;

        //            result.W = (matrix.Row2.Y - matrix.Row1.Z) * invS;
        //            result.Xyz.X = s * 0.25f;
        //            result.Xyz.Y = (matrix.Row0.Y + matrix.Row1.X) * invS;
        //            result.Xyz.Z = (matrix.Row0.Z + matrix.Row2.X) * invS;
        //        }
        //        else if (m11 > m22)
        //        {
        //            float s = (float)Math.Sqrt(1 + m11 - m00 - m22) * 2;
        //            float invS = 1f / s;

        //            result.W = (matrix.Row0.Z - matrix.Row2.X) * invS;
        //            result.Xyz.X = (matrix.Row0.Y + matrix.Row1.X) * invS;
        //            result.Xyz.Y = s * 0.25f;
        //            result.Xyz.Z = (matrix.Row1.Z + matrix.Row2.Y) * invS;
        //        }
        //        else
        //        {
        //            float s = (float)Math.Sqrt(1 + m22 - m00 - m11) * 2;
        //            float invS = 1f / s;

        //            result.W = (matrix.Row1.X - matrix.Row0.Y) * invS;
        //            result.Xyz.X = (matrix.Row0.Z + matrix.Row2.X) * invS;
        //            result.Xyz.Y = (matrix.Row1.Z + matrix.Row2.Y) * invS;
        //            result.Xyz.Z = s * 0.25f;
        //        }
        //    }
        //}

        /// <summary>
        /// Do Spherical linear interpolation between two quaternions
        /// </summary>
        /// <param name="q1">The first quaternion</param>
        /// <param name="q2">The second quaternion</param>
        /// <param name="blend">The blend factor</param>
        /// <returns>A smooth blend between the given quaternions</returns>
        public static TkQuaternion Slerp(TkQuaternion q1, TkQuaternion q2, float blend)
        {
            // if either input is zero, return the other.
            if (q1.LengthSquared == 0.0f)
            {
                if (q2.LengthSquared == 0.0f)
                {
                    return Identity;
                }
                return q2;
            }
            else if (q2.LengthSquared == 0.0f)
            {
                return q1;
            }


            float cosHalfAngle = q1.W * q2.W + TkVector3.Dot(q1.Xyz, q2.Xyz);

            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
            {
                // angle = 0.0f, so just return one input.
                return q1;
            }
            else if (cosHalfAngle < 0.0f)
            {
                q2.Xyz = -q2.Xyz;
                q2.W = -q2.W;
                cosHalfAngle = -cosHalfAngle;
            }

            float blendA;
            float blendB;
            if (cosHalfAngle < 0.99f)
            {
                // do proper slerp for big angles
                float halfAngle = (float)System.Math.Acos(cosHalfAngle);
                float sinHalfAngle = (float)System.Math.Sin(halfAngle);
                float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = (float)System.Math.Sin(halfAngle * (1.0f - blend)) * oneOverSinHalfAngle;
                blendB = (float)System.Math.Sin(halfAngle * blend) * oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1.0f - blend;
                blendB = blend;
            }

            TkQuaternion result = new TkQuaternion(blendA * q1.Xyz + blendB * q2.Xyz, blendA * q1.W + blendB * q2.W);
            if (result.LengthSquared > 0.0f)
            {
                return Normalize(result);
            }
            else
            {
                return Identity;
            }
        }

        /// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static TkQuaternion operator +(TkQuaternion left, TkQuaternion right)
        {
            left.Xyz += right.Xyz;
            left.W += right.W;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static TkQuaternion operator -(TkQuaternion left, TkQuaternion right)
        {
            left.Xyz -= right.Xyz;
            left.W -= right.W;
            return left;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static TkQuaternion operator *(TkQuaternion left, TkQuaternion right)
        {
            Multiply(ref left, ref right, out left);
            return left;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static TkQuaternion operator *(TkQuaternion quaternion, float scale)
        {
            Multiply(ref quaternion, scale, out quaternion);
            return quaternion;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static TkQuaternion operator *(float scale, TkQuaternion quaternion)
        {
            return new TkQuaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(TkQuaternion left, TkQuaternion right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(TkQuaternion left, TkQuaternion right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a System.String that represents the current Quaternion.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("V: {0}, W: {1}", Xyz, W);
        }

        /// <summary>
        /// Compares this object instance to another object for equality.
        /// </summary>
        /// <param name="other">The other object to be used in the comparison.</param>
        /// <returns>True if both objects are Quaternions of equal value. Otherwise it returns false.</returns>
        public override bool Equals(object other)
        {
            if (other is TkQuaternion == false)
            {
                return false;
            }
            return this == (TkQuaternion)other;
        }

        /// <summary>
        /// Provides the hash code for this object.
        /// </summary>
        /// <returns>A hash code formed from the bitwise XOR of this objects members.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Xyz.GetHashCode() * 397) ^ this.W.GetHashCode();
            }
        }

        /// <summary>
        /// Compares this Quaternion instance to another Quaternion for equality.
        /// </summary>
        /// <param name="other">The other Quaternion to be used in the comparison.</param>
        /// <returns>True if both instances are equal; false otherwise.</returns>
        public bool Equals(TkQuaternion other)
        {
            return Xyz == other.Xyz && W == other.W;
        }
    }
}
