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
    /// 4-component TkVector of the Half type. Occupies 8 Byte total.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct TkVector4h : ISerializable, IEquatable<TkVector4h>
    {
        /// <summary>The X component of the Half4.</summary>
        public Half X;

        /// <summary>The Y component of the Half4.</summary>
        public Half Y;

        /// <summary>The Z component of the Half4.</summary>
        public Half Z;

        /// <summary>The W component of the Half4.</summary>
        public Half W;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="value">The value that will initialize this instance.</param>
        public TkVector4h(Half value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="value">The value that will initialize this instance.</param>
        public TkVector4h(Single value)
        {
            X = new Half(value);
            Y = new Half(value);
            Z = new Half(value);
            W = new Half(value);
        }

        /// <summary>
        /// The new Half4 instance will avoid conversion and copy directly from the Half parameters.
        /// </summary>
        /// <param name="x">An Half instance of a 16-bit half-precision floating-point number.</param>
        /// <param name="y">An Half instance of a 16-bit half-precision floating-point number.</param>
        /// <param name="z">An Half instance of a 16-bit half-precision floating-point number.</param>
        /// <param name="w">An Half instance of a 16-bit half-precision floating-point number.</param>
        public TkVector4h(Half x, Half y, Half z, Half w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// The new Half4 instance will convert the 4 parameters into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="x">32-bit single-precision floating-point number.</param>
        /// <param name="y">32-bit single-precision floating-point number.</param>
        /// <param name="z">32-bit single-precision floating-point number.</param>
        /// <param name="w">32-bit single-precision floating-point number.</param>
        public TkVector4h(Single x, Single y, Single z, Single w)
        {
            X = new Half(x);
            Y = new Half(y);
            Z = new Half(z);
            W = new Half(w);
        }

        /// <summary>
        /// The new Half4 instance will convert the 4 parameters into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="x">32-bit single-precision floating-point number.</param>
        /// <param name="y">32-bit single-precision floating-point number.</param>
        /// <param name="z">32-bit single-precision floating-point number.</param>
        /// <param name="w">32-bit single-precision floating-point number.</param>
        /// <param name="throwOnError">Enable checks that will throw if the conversion result is not meaningful.</param>
        public TkVector4h(Single x, Single y, Single z, Single w, bool throwOnError)
        {
            X = new Half(x, throwOnError);
            Y = new Half(y, throwOnError);
            Z = new Half(z, throwOnError);
            W = new Half(w, throwOnError);
        }

        /// <summary>
        /// The new Half4 instance will convert the TkVector4 into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="v">OpenTK.TkVector4</param>
        [CLSCompliant(false)]
        public TkVector4h(TkVector4 v)
        {
            X = new Half(v.X);
            Y = new Half(v.Y);
            Z = new Half(v.Z);
            W = new Half(v.W);
        }

        /// <summary>
        /// The new Half4 instance will convert the TkVector4 into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="v">OpenTK.TkVector4</param>
        /// <param name="throwOnError">Enable checks that will throw if the conversion result is not meaningful.</param>
        [CLSCompliant(false)]
        public TkVector4h(TkVector4 v, bool throwOnError)
        {
            X = new Half(v.X, throwOnError);
            Y = new Half(v.Y, throwOnError);
            Z = new Half(v.Z, throwOnError);
            W = new Half(v.W, throwOnError);
        }

        /// <summary>
        /// The new Half4 instance will convert the TkVector4 into 16-bit half-precision floating-point.
        /// This is the fastest constructor.
        /// </summary>
        /// <param name="v">OpenTK.TkVector4</param>
        public TkVector4h(ref TkVector4 v)
        {
            X = new Half(v.X);
            Y = new Half(v.Y);
            Z = new Half(v.Z);
            W = new Half(v.W);
        }

        /// <summary>
        /// The new Half4 instance will convert the TkVector4 into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="v">OpenTK.TkVector4</param>
        /// <param name="throwOnError">Enable checks that will throw if the conversion result is not meaningful.</param>
        public TkVector4h(ref TkVector4 v, bool throwOnError)
        {
            X = new Half(v.X, throwOnError);
            Y = new Half(v.Y, throwOnError);
            Z = new Half(v.Z, throwOnError);
            W = new Half(v.W, throwOnError);
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
        /// Gets or sets an OpenTK.TkVector2h with the X and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Xw { get { return new TkVector2h(X, W); } set { X = value.X; W = value.Y; } }

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
        /// Gets or sets an OpenTK.TkVector2h with the Y and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Yw { get { return new TkVector2h(Y, W); } set { Y = value.X; W = value.Y; } }

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
        /// Gets an OpenTK.TkVector2h with the Z and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Zw { get { return new TkVector2h(Z, W); } set { Z = value.X; W = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector2h with the W and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Wx { get { return new TkVector2h(W, X); } set { W = value.X; X = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector2h with the W and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Wy { get { return new TkVector2h(W, Y); } set { W = value.X; Y = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector2h with the W and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector2h Wz { get { return new TkVector2h(W, Z); } set { W = value.X; Z = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the X, Y, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Xyz { get { return new TkVector3h(X, Y, Z); } set { X = value.X; Y = value.Y; Z = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the X, Y, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Xyw { get { return new TkVector3h(X, Y, W); } set { X = value.X; Y = value.Y; W = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the X, Z, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Xzy { get { return new TkVector3h(X, Z, Y); } set { X = value.X; Z = value.Y; Y = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the X, Z, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Xzw { get { return new TkVector3h(X, Z, W); } set { X = value.X; Z = value.Y; W = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the X, W, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Xwy { get { return new TkVector3h(X, W, Y); } set { X = value.X; W = value.Y; Y = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the X, W, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Xwz { get { return new TkVector3h(X, W, Z); } set { X = value.X; W = value.Y; Z = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Y, X, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Yxz { get { return new TkVector3h(Y, X, Z); } set { Y = value.X; X = value.Y; Z = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Y, X, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Yxw { get { return new TkVector3h(Y, X, W); } set { Y = value.X; X = value.Y; W = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Y, Z, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Yzx { get { return new TkVector3h(Y, Z, X); } set { Y = value.X; Z = value.Y; X = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Y, Z, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Yzw { get { return new TkVector3h(Y, Z, W); } set { Y = value.X; Z = value.Y; W = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Y, W, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Ywx { get { return new TkVector3h(Y, W, X); } set { Y = value.X; W = value.Y; X = value.Z; } }

        /// <summary>
        /// Gets an OpenTK.TkVector3h with the Y, W, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Ywz { get { return new TkVector3h(Y, W, Z); } set { Y = value.X; W = value.Y; Z = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Z, X, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Zxy { get { return new TkVector3h(Z, X, Y); } set { Z = value.X; X = value.Y; Y = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Z, X, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Zxw { get { return new TkVector3h(Z, X, W); } set { Z = value.X; X = value.Y; W = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Z, Y, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Zyx { get { return new TkVector3h(Z, Y, X); } set { Z = value.X; Y = value.Y; X = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Z, Y, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Zyw { get { return new TkVector3h(Z, Y, W); } set { Z = value.X; Y = value.Y; W = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Z, W, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Zwx { get { return new TkVector3h(Z, W, X); } set { Z = value.X; W = value.Y; X = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the Z, W, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Zwy { get { return new TkVector3h(Z, W, Y); } set { Z = value.X; W = value.Y; Y = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the W, X, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Wxy { get { return new TkVector3h(W, X, Y); } set { W = value.X; X = value.Y; Y = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the W, X, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Wxz { get { return new TkVector3h(W, X, Z); } set { W = value.X; X = value.Y; Z = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the W, Y, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Wyx { get { return new TkVector3h(W, Y, X); } set { W = value.X; Y = value.Y; X = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the W, Y, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Wyz { get { return new TkVector3h(W, Y, Z); } set { W = value.X; Y = value.Y; Z = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the W, Z, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Wzx { get { return new TkVector3h(W, Z, X); } set { W = value.X; Z = value.Y; X = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector3h with the W, Z, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector3h Wzy { get { return new TkVector3h(W, Z, Y); } set { W = value.X; Z = value.Y; Y = value.Z; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the X, Y, W, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Xywz { get { return new TkVector4h(X, Y, W, Z); } set { X = value.X; Y = value.Y; W = value.Z; Z = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the X, Z, Y, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Xzyw { get { return new TkVector4h(X, Z, Y, W); } set { X = value.X; Z = value.Y; Y = value.Z; W = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the X, Z, W, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Xzwy { get { return new TkVector4h(X, Z, W, Y); } set { X = value.X; Z = value.Y; W = value.Z; Y = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the X, W, Y, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Xwyz { get { return new TkVector4h(X, W, Y, Z); } set { X = value.X; W = value.Y; Y = value.Z; Z = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the X, W, Z, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Xwzy { get { return new TkVector4h(X, W, Z, Y); } set { X = value.X; W = value.Y; Z = value.Z; Y = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Y, X, Z, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Yxzw { get { return new TkVector4h(Y, X, Z, W); } set { Y = value.X; X = value.Y; Z = value.Z; W = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Y, X, W, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Yxwz { get { return new TkVector4h(Y, X, W, Z); } set { Y = value.X; X = value.Y; W = value.Z; Z = value.W; } }

        /// <summary>
        /// Gets an OpenTK.TkVector4h with the Y, Y, Z, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Yyzw { get { return new TkVector4h(Y, Y, Z, W); } set { X = value.X; Y = value.Y; Z = value.Z; W = value.W; } }

        /// <summary>
        /// Gets an OpenTK.TkVector4h with the Y, Y, W, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Yywz { get { return new TkVector4h(Y, Y, W, Z); } set { X = value.X; Y = value.Y; W = value.Z; Z = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Y, Z, X, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Yzxw { get { return new TkVector4h(Y, Z, X, W); } set { Y = value.X; Z = value.Y; X = value.Z; W = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Y, Z, W, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Yzwx { get { return new TkVector4h(Y, Z, W, X); } set { Y = value.X; Z = value.Y; W = value.Z; X = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Y, W, X, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Ywxz { get { return new TkVector4h(Y, W, X, Z); } set { Y = value.X; W = value.Y; X = value.Z; Z = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Y, W, Z, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Ywzx { get { return new TkVector4h(Y, W, Z, X); } set { Y = value.X; W = value.Y; Z = value.Z; X = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Z, X, Y, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Zxyw { get { return new TkVector4h(Z, X, Y, W); } set { Z = value.X; X = value.Y; Y = value.Z; W = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Z, X, W, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Zxwy { get { return new TkVector4h(Z, X, W, Y); } set { Z = value.X; X = value.Y; W = value.Z; Y = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Z, Y, X, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Zyxw { get { return new TkVector4h(Z, Y, X, W); } set { Z = value.X; Y = value.Y; X = value.Z; W = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Z, Y, W, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Zywx { get { return new TkVector4h(Z, Y, W, X); } set { Z = value.X; Y = value.Y; W = value.Z; X = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Z, W, X, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Zwxy { get { return new TkVector4h(Z, W, X, Y); } set { Z = value.X; W = value.Y; X = value.Z; Y = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the Z, W, Y, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Zwyx { get { return new TkVector4h(Z, W, Y, X); } set { Z = value.X; W = value.Y; Y = value.Z; X = value.W; } }

        /// <summary>
        /// Gets an OpenTK.TkVector4h with the Z, W, Z, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Zwzy { get { return new TkVector4h(Z, W, Z, Y); } set { X = value.X; W = value.Y; Z = value.Z; Y = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the W, X, Y, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Wxyz { get { return new TkVector4h(W, X, Y, Z); } set { W = value.X; X = value.Y; Y = value.Z; Z = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the W, X, Z, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Wxzy { get { return new TkVector4h(W, X, Z, Y); } set { W = value.X; X = value.Y; Z = value.Z; Y = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the W, Y, X, and Z components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Wyxz { get { return new TkVector4h(W, Y, X, Z); } set { W = value.X; Y = value.Y; X = value.Z; Z = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the W, Y, Z, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Wyzx { get { return new TkVector4h(W, Y, Z, X); } set { W = value.X; Y = value.Y; Z = value.Z; X = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the W, Z, X, and Y components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Wzxy { get { return new TkVector4h(W, Z, X, Y); } set { W = value.X; Z = value.Y; X = value.Z; Y = value.W; } }

        /// <summary>
        /// Gets or sets an OpenTK.TkVector4h with the W, Z, Y, and X components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Wzyx { get { return new TkVector4h(W, Z, Y, X); } set { W = value.X; Z = value.Y; Y = value.Z; X = value.W; } }

        /// <summary>
        /// Gets an OpenTK.TkVector4h with the W, Z, Y, and W components of this instance.
        /// </summary>
        [XmlIgnore]
        public TkVector4h Wzyw { get { return new TkVector4h(W, Z, Y, W); } set { X = value.X; Z = value.Y; Y = value.Z; W = value.W; } }

        /// <summary>
        /// Returns this Half4 instance's contents as TkVector4.
        /// </summary>
        /// <returns>OpenTK.TkVector4</returns>
        public TkVector4 ToTkVector4()
        {
            return new TkVector4(X, Y, Z, W);
        }

        /// <summary>Converts OpenTK.TkVector4 to OpenTK.Half4.</summary>
        /// <param name="v4f">The TkVector4 to convert.</param>
        /// <returns>The resulting Half TkVector.</returns>
        public static explicit operator TkVector4h(TkVector4 v4f)
        {
            return new TkVector4h(v4f);
        }

        /// <summary>Converts OpenTK.Half4 to OpenTK.TkVector4.</summary>
        /// <param name="h4">The Half4 to convert.</param>
        /// <returns>The resulting TkVector4.</returns>
        public static explicit operator TkVector4(TkVector4h h4)
        {
            return new TkVector4(
                h4.X.ToSingle(),
                h4.Y.ToSingle(),
                h4.Z.ToSingle(),
                h4.W.ToSingle());
        }

        /// <summary>The size in bytes for an instance of the Half4 struct is 8.</summary>
        public static readonly int SizeInBytes = 8;

        /// <summary>Constructor used by ISerializable to deserialize the object.</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public TkVector4h(SerializationInfo info, StreamingContext context)
        {
            this.X = (Half)info.GetValue("X", typeof(Half));
            this.Y = (Half)info.GetValue("Y", typeof(Half));
            this.Z = (Half)info.GetValue("Z", typeof(Half));
            this.W = (Half)info.GetValue("W", typeof(Half));
        }

        /// <summary>Used by ISerialize to serialize the object.</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", this.X);
            info.AddValue("Y", this.Y);
            info.AddValue("Z", this.Z);
            info.AddValue("W", this.W);
        }

        /// <summary>Updates the X,Y,Z and W components of this instance by reading from a Stream.</summary>
        /// <param name="bin">A BinaryReader instance associated with an open Stream.</param>
        public void FromBinaryStream(BinaryReader bin)
        {
            X.FromBinaryStream(bin);
            Y.FromBinaryStream(bin);
            Z.FromBinaryStream(bin);
            W.FromBinaryStream(bin);
        }

        /// <summary>Writes the X,Y,Z and W components of this instance into a Stream.</summary>
        /// <param name="bin">A BinaryWriter instance associated with an open Stream.</param>
        public void ToBinaryStream(BinaryWriter bin)
        {
            X.ToBinaryStream(bin);
            Y.ToBinaryStream(bin);
            Z.ToBinaryStream(bin);
            W.ToBinaryStream(bin);
        }

        /// <summary>Returns a value indicating whether this instance is equal to a specified OpenTK.Half4 TkVector.</summary>
        /// <param name="other">OpenTK.Half4 to compare to this instance..</param>
        /// <returns>True, if other is equal to this instance; false otherwise.</returns>
        public bool Equals(TkVector4h other)
        {
            return (this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Z.Equals(other.Z) && this.W.Equals(other.W));
        }

        private static string listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        /// <summary>Returns a string that contains this Half4's numbers in human-legible form.</summary>
        public override string ToString()
        {
            return String.Format("({0}{4} {1}{4} {2}{4} {3})", X.ToString(), Y.ToString(), Z.ToString(), W.ToString(), listSeparator);
        }

        /// <summary>Returns the Half4 as an array of bytes.</summary>
        /// <param name="h">The Half4 to convert.</param>
        /// <returns>The input as byte array.</returns>
        public static byte[] GetBytes(TkVector4h h)
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
            temp = Half.GetBytes(h.W);
            result[6] = temp[0];
            result[7] = temp[1];

            return result;
        }

        /// <summary>Converts an array of bytes into Half4.</summary>
        /// <param name="value">A Half4 in it's byte[] representation.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A new Half4 instance.</returns>
        public static TkVector4h FromBytes(byte[] value, int startIndex)
        {
            return new TkVector4h(
                Half.FromBytes(value, startIndex),
                Half.FromBytes(value, startIndex + 2),
                Half.FromBytes(value, startIndex + 4),
                Half.FromBytes(value, startIndex + 6));
        }
    }
}
