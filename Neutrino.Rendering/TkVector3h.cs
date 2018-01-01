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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Neutrino
{
    /// <summary>
    /// 3-component TkVector of the Half type. Occupies 6 Byte total.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct TkVector3h : ISerializable, IEquatable<TkVector3h>
    {
        /// <summary>The X component of the Half3.</summary>
        public Half X;

        /// <summary>The Y component of the Half3.</summary>
        public Half Y;

        /// <summary>The Z component of the Half3.</summary>
        public Half Z;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="value">The value that will initialize this instance.</param>
        public TkVector3h(Half value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="value">The value that will initialize this instance.</param>
        public TkVector3h(Single value)
        {
            X = new Half(value);
            Y = new Half(value);
            Z = new Half(value);
        }

        /// <summary>
        /// The new Half3 instance will avoid conversion and copy directly from the Half parameters.
        /// </summary>
        /// <param name="x">An Half instance of a 16-bit half-precision floating-point number.</param>
        /// <param name="y">An Half instance of a 16-bit half-precision floating-point number.</param>
        /// <param name="z">An Half instance of a 16-bit half-precision floating-point number.</param>
        public TkVector3h(Half x, Half y, Half z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// The new Half3 instance will convert the 3 parameters into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="x">32-bit single-precision floating-point number.</param>
        /// <param name="y">32-bit single-precision floating-point number.</param>
        /// <param name="z">32-bit single-precision floating-point number.</param>
        public TkVector3h(Single x, Single y, Single z)
        {
            X = new Half(x);
            Y = new Half(y);
            Z = new Half(z);
        }

        /// <summary>
        /// The new Half3 instance will convert the 3 parameters into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="x">32-bit single-precision floating-point number.</param>
        /// <param name="y">32-bit single-precision floating-point number.</param>
        /// <param name="z">32-bit single-precision floating-point number.</param>
        /// <param name="throwOnError">Enable checks that will throw if the conversion result is not meaningful.</param>
        public TkVector3h(Single x, Single y, Single z, bool throwOnError)
        {
            X = new Half(x, throwOnError);
            Y = new Half(y, throwOnError);
            Z = new Half(z, throwOnError);
        }

        /// <summary>
        /// The new Half3 instance will convert the TkVector3 into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="v">OpenTK.TkVector3</param>
        [CLSCompliant(false)]
        public TkVector3h(TkVector3 v)
        {
            X = new Half(v.X);
            Y = new Half(v.Y);
            Z = new Half(v.Z);
        }

        /// <summary>
        /// The new Half3 instance will convert the TkVector3 into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="v">OpenTK.TkVector3</param>
        /// <param name="throwOnError">Enable checks that will throw if the conversion result is not meaningful.</param>
        [CLSCompliant(false)]
        public TkVector3h(TkVector3 v, bool throwOnError)
        {
            X = new Half(v.X, throwOnError);
            Y = new Half(v.Y, throwOnError);
            Z = new Half(v.Z, throwOnError);
        }

        /// <summary>
        /// The new Half3 instance will convert the TkVector3 into 16-bit half-precision floating-point.
        /// This is the fastest constructor.
        /// </summary>
        /// <param name="v">OpenTK.TkVector3</param>
        public TkVector3h(ref TkVector3 v)
        {
            X = new Half(v.X);
            Y = new Half(v.Y);
            Z = new Half(v.Z);
        }

        /// <summary>
        /// The new Half3 instance will convert the TkVector3 into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="v">OpenTK.TkVector3</param>
        /// <param name="throwOnError">Enable checks that will throw if the conversion result is not meaningful.</param>
        [CLSCompliant(false)]
        public TkVector3h(ref TkVector3 v, bool throwOnError)
        {
            X = new Half(v.X, throwOnError);
            Y = new Half(v.Y, throwOnError);
            Z = new Half(v.Z, throwOnError);
        }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector2h with the X and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Xy { get { return new TkVector2h(X, Y); } set { X = value.X; Y = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector2h with the X and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Xz { get { return new TkVector2h(X, Z); } set { X = value.X; Z = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector2h with the Y and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Yx { get { return new TkVector2h(Y, X); } set { Y = value.X; X = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector2h with the Y and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Yz { get { return new TkVector2h(Y, Z); } set { Y = value.X; Z = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector2h with the Z and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Zx { get { return new TkVector2h(Z, X); } set { Z = value.X; X = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector2h with the Z and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Zy { get { return new TkVector2h(Z, Y); } set { Z = value.X; Y = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the X, Z, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Xzy { get { return new TkVector3h(X, Z, Y); } set { X = value.X; Z = value.Y; Y = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Y, X, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Yxz { get { return new TkVector3h(Y, X, Z); } set { Y = value.X; X = value.Y; Z = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Y, Z, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Yzx { get { return new TkVector3h(Y, Z, X); } set { Y = value.X; Z = value.Y; X = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Z, X, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Zxy { get { return new TkVector3h(Z, X, Y); } set { Z = value.X; X = value.Y; Y = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Z, Y, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Zyx { get { return new TkVector3h(Z, Y, X); } set { Z = value.X; Y = value.Y; X = value.Z; } }

        /// <summary>
        /// Returns this Half3 instance's contents as TkVector3.
        /// </summary>
        /// <returns>OpenTK.TkVector3</returns>
        public TkVector3 ToTkVector3()
        {
            return new TkVector3(X, Y, Z);
        }

        /// <summary>Converts OpenTK.TkVector3 to OpenTK.Half3.</summary>
        /// <param name="v3f">The TkVector3 to convert.</param>
        /// <returns>The resulting Half TkVector.</returns>
        public static explicit operator TkVector3h(TkVector3 v3f)
        {
            return new TkVector3h(v3f);
        }

        /// <summary>Converts OpenTK.Half3 to OpenTK.TkVector3.</summary>
        /// <param name="h3">The Half3 to convert.</param>
        /// <returns>The resulting TkVector3.</returns>
        public static explicit operator TkVector3(TkVector3h h3)
        {
            return new TkVector3(
                h3.X.ToSingle(),
                h3.Y.ToSingle(),
                h3.Z.ToSingle());
        }

        /// <summary>The size in bytes for an instance of the Half3 struct is 6.</summary>
        public static readonly int SizeInBytes = 6;

        /// <summary>Constructor used by ISerializable to deserialize the object.</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public TkVector3h(SerializationInfo info, StreamingContext context)
        {
            this.X = (Half)info.GetValue("X", typeof(Half));
            this.Y = (Half)info.GetValue("Y", typeof(Half));
            this.Z = (Half)info.GetValue("Z", typeof(Half));
        }

        /// <summary>Used by ISerialize to serialize the object.</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", this.X);
            info.AddValue("Y", this.Y);
            info.AddValue("Z", this.Z);
        }

        /// <summary>Updates the X,Y and Z components of this instance by reading from a Stream.</summary>
        /// <param name="bin">A BinaryReader instance associated with an open Stream.</param>
        public void FromBinaryStream(BinaryReader bin)
        {
            X.FromBinaryStream(bin);
            Y.FromBinaryStream(bin);
            Z.FromBinaryStream(bin);
        }

        /// <summary>Writes the X,Y and Z components of this instance into a Stream.</summary>
        /// <param name="bin">A BinaryWriter instance associated with an open Stream.</param>
        public void ToBinaryStream(BinaryWriter bin)
        {
            X.ToBinaryStream(bin);
            Y.ToBinaryStream(bin);
            Z.ToBinaryStream(bin);
        }

        /// <summary>Returns a value indicating whether this instance is equal to a specified OpenTK.Half3 TkVector.</summary>
        /// <param name="other">OpenTK.Half3 to compare to this instance..</param>
        /// <returns>True, if other is equal to this instance; false otherwise.</returns>
        public bool Equals(TkVector3h other)
        {
            return (this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Z.Equals(other.Z));
        }

        private static string listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        /// <summary>Returns a string that contains this Half3's numbers in human-legible form.</summary>
        public override string ToString()
        {
            return String.Format("({0}{3} {1}{3} {2})", X.ToString(), Y.ToString(), Z.ToString(), listSeparator);
        }

        /// <summary>Returns the Half3 as an array of bytes.</summary>
        /// <param name="h">The Half3 to convert.</param>
        /// <returns>The input as byte array.</returns>
        public static byte[] GetBytes(TkVector3h h)
        {
            byte[] result = new byte[SizeInBytes];

            byte[] temp = Half.GetBytes(h.X);
            result[0] = temp[0];
            result[1] = temp[1];
            temp = Half.GetBytes(h.Y);
            result[2] = temp[0];
            result[3] = temp[1];
            temp = Half.GetBytes(h.Z);
            result[4] = temp[0];
            result[5] = temp[1];

            return result;
        }

        /// <summary>Converts an array of bytes into Half3.</summary>
        /// <param name="value">A Half3 in it's byte[] representation.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A new Half3 instance.</returns>
        public static TkVector3h FromBytes(byte[] value, int startIndex)
        {
            return new TkVector3h(
                Half.FromBytes(value, startIndex),
                Half.FromBytes(value, startIndex + 2),
                Half.FromBytes(value, startIndex + 4));
        }
    }
}
