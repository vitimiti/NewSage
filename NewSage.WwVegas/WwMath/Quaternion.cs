// -----------------------------------------------------------------------
// <copyright file="Quaternion.cs" company="NewSage">
// A transliteration and update of the CnC Generals (Zero Hour) engine and games with mod-first support.
// Copyright (C) 2025 NewSage Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see https://www.gnu.org/licenses/.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NewSage.WwVegas.WwMath;

[StructLayout(LayoutKind.Sequential)]
public struct Quaternion : IEquatable<Quaternion>
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Quaternion(bool identity)
    {
        if (!identity)
        {
            return;
        }

        (X, Y, Z, W) = (0F, 0F, 0F, 1F);
    }

    public Quaternion(float a, float b, float c, float d) => Set(a, b, c, d);

    public Quaternion(Vector3 axis, float angle)
    {
        var s = float.Sin(angle / 2F);
        var c = float.Cos(angle / 2F);

        X = s * axis.X;
        Y = s * axis.Y;
        Z = s * axis.Z;
        W = c;
    }

    public readonly float Length2 => (X * X) + (Y * Y) + (Z * Z) + (W * W);

    public readonly float Length => float.Sqrt(Length2);

    public readonly bool IsValid =>
        VegasMath.IsValid(X) && VegasMath.IsValid(Y) && VegasMath.IsValid(Z) && VegasMath.IsValid(W);

    public readonly Quaternion Inverse => new(-X, -Y, -Z, W);

    public readonly Quaternion Conjugate => new(-X, -Y, -Z, W);

    public static Quaternion Slerp(Quaternion x, Quaternion y, float alpha)
    {
        var cosT = (x.X * y.X) + (x.Y * y.Y) + (x.Z * y.Z) + (x.W * y.W);

        bool qFlip;
        if (cosT < 0F)
        {
            cosT = -cosT;
            qFlip = true;
        }
        else
        {
            qFlip = false;
        }

        float beta;
        if (1F - cosT < float.Epsilon * float.Epsilon)
        {
            beta = 1F - alpha;
        }
        else
        {
            var theta = float.Acos(cosT);
            var sinT = float.Sin(theta);
            var oneOverSinT = 1F / sinT;
            beta = float.Sin(theta - (alpha * theta)) * oneOverSinT;
            alpha = float.Sin(alpha * theta) * oneOverSinT;
        }

        if (qFlip)
        {
            alpha = -alpha;
        }

        Quaternion result = default;
        result.X = (beta * x.X) + (alpha * y.X);
        result.Y = (beta * x.Y) + (alpha * y.Y);
        result.Z = (beta * x.Z) + (alpha * y.Z);
        result.W = (beta * x.W) + (alpha * y.W);
        return result;
    }

    public static Quaternion FastSlerp(Quaternion x, Quaternion y, float alpha)
    {
        bool qFlip;
        var cosT = (x.X * y.X) + (x.Y * y.Y) + (x.Z * y.Z) + (x.W * y.W);
        if (cosT < 0F)
        {
            cosT = -cosT;
            qFlip = true;
        }
        else
        {
            qFlip = false;
        }

        float beta;
        if (1F - cosT < float.Epsilon * float.Epsilon)
        {
            beta = 1F - alpha;
        }
        else
        {
            var theta = VegasMath.FastAcos(cosT);
            var sinT = VegasMath.FastSin(theta);
            var oneOverSinT = 1F / sinT;
            beta = VegasMath.FastSin(theta - (alpha * theta)) * oneOverSinT;
            alpha = VegasMath.FastSin(alpha * theta) * oneOverSinT;
        }

        if (qFlip)
        {
            alpha = -alpha;
        }

        return new Quaternion(
            (beta * x.X) + (alpha * y.X),
            (beta * x.Y) + (alpha * y.Y),
            (beta * x.Z) + (alpha * y.Z),
            (beta * x.W) + (alpha * y.W)
        );
    }

    public static SlerpInfo SlerpSetup(Quaternion x, Quaternion y)
    {
        SlerpInfo result = default;
        var cosT = (x.X * y.X) + (x.Y * y.Y) + (x.Z * y.Z) + (x.W * y.W);

        if (cosT < 0F)
        {
            cosT = -cosT;
            result.Flip = true;
        }
        else
        {
            result.Flip = false;
        }

        if (1F - cosT < float.Epsilon)
        {
            result.Linear = true;
            result.Theta = 0F;
            result.SinTheta = 0F;
        }
        else
        {
            result.Linear = false;
            result.Theta = float.Acos(cosT);
            result.SinTheta = float.Sin(result.Theta);
        }

        return result;
    }

    public static Quaternion CachedSlerp(Quaternion x, Quaternion y, float alpha, SlerpInfo slerpInfo)
    {
        float beta;
        float oneOverSinT;
        if (slerpInfo.Linear)
        {
            beta = 1F - alpha;
        }
        else
        {
            oneOverSinT = 1F / slerpInfo.Theta;
            beta = float.Sin(slerpInfo.Theta - (alpha * slerpInfo.Theta)) * oneOverSinT;
            alpha = float.Sin(alpha * slerpInfo.Theta) * oneOverSinT;
        }

        if (slerpInfo.Flip)
        {
            alpha = -alpha;
        }

        return new Quaternion(
            (beta * x.X) + (alpha * y.X),
            (beta * x.Y) + (alpha * y.Y),
            (beta * x.Z) + (alpha * y.Z),
            (beta * x.W) + (alpha * y.W)
        );
    }

    public static Quaternion Build(Matrix3X3 matrix) => FromMatrix3X3(matrix);

    public static Quaternion Build(Matrix3D matrix) => FromMatrix3D(matrix);

    public static Quaternion Build(Matrix4X4 matrix) => FromMatrix4X4(matrix);

    public static Matrix3X3 BuildMatrix3X3(Quaternion quaternion) => quaternion.ToMatrix3X3();

    public static Matrix3D BuildMatrix3D(Quaternion quaternion) => quaternion.ToMatrix3D();

    public static Matrix4X4 BuildMatrix4X4(Quaternion quaternion) => quaternion.ToMatrix4X4();

    public static Quaternion FromMatrix3X3(Matrix3X3 matrix)
    {
        Quaternion result = default;
        var tr = matrix[0][0] + matrix[1][1] + matrix[2][2];
        if (tr > 0F)
        {
            var s = float.Sqrt(tr + 1);
            result[3] = s * .5F;
            s = .5F / s;

            result[0] = (matrix[2][1] - matrix[1][2]) * s;
            result[1] = (matrix[0][2] - matrix[2][0]) * s;
            result[2] = (matrix[1][0] - matrix[0][1]) * s;
        }
        else
        {
            var i = 0;
            if (matrix[1][1] > matrix[0][0])
            {
                i = 1;
            }

            if (matrix[2][2] > matrix[i][i])
            {
                i = 2;
            }

            var j = Next(i);
            var k = Next(j);

            var s = float.Sqrt(matrix[i][i] - (matrix[j][j] + matrix[k][k]) + 1F);

            result[i] = s * .5F;
            if (float.Abs(s) > float.Epsilon)
            {
                s = .5F / s;
            }

            result[3] = (matrix[k][j] - matrix[j][k]) * s;
            result[j] = (matrix[j][i] + matrix[i][j]) * s;
            result[k] = (matrix[k][i] + matrix[i][k]) * s;
        }

        return result;
    }

    public static Quaternion FromMatrix3D(Matrix3D matrix)
    {
        Quaternion result = default;
        var tr = matrix[0][0] + matrix[1][1] + matrix[2][2];
        if (tr > 0F)
        {
            var s = float.Sqrt(tr + 1);
            result[3] = s * .5F;
            s = .5F / s;

            result[0] = (matrix[2][1] - matrix[1][2]) * s;
            result[1] = (matrix[0][2] - matrix[2][0]) * s;
            result[2] = (matrix[1][0] - matrix[0][1]) * s;
        }
        else
        {
            var i = 0;
            if (matrix[1][1] > matrix[0][0])
            {
                i = 1;
            }

            if (matrix[2][2] > matrix[i][i])
            {
                i = 2;
            }

            var j = Next(i);
            var k = Next(j);

            var s = float.Sqrt(matrix[i][i] - (matrix[j][j] + matrix[k][k]) + 1F);

            result[i] = s * .5F;
            if (float.Abs(s) > float.Epsilon)
            {
                s = .5F / s;
            }

            result[3] = (matrix[k][j] - matrix[j][k]) * s;
            result[j] = (matrix[j][i] + matrix[i][j]) * s;
            result[k] = (matrix[k][i] + matrix[i][k]) * s;
        }

        return result;
    }

    public static Quaternion FromMatrix4X4(Matrix4X4 matrix)
    {
        Quaternion result = default;
        var tr = matrix[0][0] + matrix[1][1] + matrix[2][2];
        if (tr > 0F)
        {
            var s = float.Sqrt(tr + 1);
            result[3] = s * .5F;
            s = .5F / s;

            result[0] = (matrix[2][1] - matrix[1][2]) * s;
            result[1] = (matrix[0][2] - matrix[2][0]) * s;
            result[2] = (matrix[1][0] - matrix[0][1]) * s;
        }
        else
        {
            var i = 0;
            if (matrix[1][1] > matrix[0][0])
            {
                i = 1;
            }

            if (matrix[2][2] > matrix[i][i])
            {
                i = 2;
            }

            var j = Next(i);
            var k = Next(j);

            var s = float.Sqrt(matrix[i][i] - (matrix[j][j] + matrix[k][k]) + 1F);

            result[i] = s * .5F;
            if (float.Abs(s) > float.Epsilon)
            {
                s = .5F / s;
            }

            result[3] = (matrix[k][j] - matrix[j][k]) * s;
            result[j] = (matrix[j][i] + matrix[i][j]) * s;
            result[k] = (matrix[k][i] + matrix[i][k]) * s;
        }

        return result;
    }

    public static Quaternion Normalize(Quaternion quaternion)
    {
        var length = quaternion.Length;
        if (float.Abs(length) < float.Epsilon)
        {
            return quaternion;
        }

        var oneOverLength = VegasMath.InvSqrt(length);
        return quaternion * oneOverLength;
    }

    public static Quaternion AxisToQuaternion(Vector3 axis, float phi)
    {
        Quaternion q = default;
        Vector3 temp = axis;

        temp.Normalize();
        q[0] = temp[0];
        q[1] = temp[1];
        q[2] = temp[2];

        q.Scale(float.Sin(phi / 2F));
        q[3] = float.Cos(phi / 2F);

        return q;
    }

    public static Quaternion Trackball(float x0, float y0, float x1, float y1, float sphereSize)
    {
        Vector3 p1 = default;
        Vector3 p2 = default;

        if (float.Abs(x0 - x1) < float.Epsilon && float.Abs(y0 - y1) < float.Epsilon)
        {
            return new Quaternion(0, 0, 0, 1);
        }

        p1[0] = x0;
        p1[1] = y0;
        p1[2] = ProjectToSphere(sphereSize, x0, y0);

        p2[0] = x1;
        p2[1] = y1;
        p2[2] = ProjectToSphere(sphereSize, x1, y1);

        var a = Vector3.CrossProduct(p2, p1);

        Vector3 d = p1 - p2;
        var t = float.Clamp(d.Length / (2F * sphereSize), -1F, 1F);
        var phi = 2F * float.Asin(t);

        return AxisToQuaternion(a, phi);
    }

    public void Set(float a = 0F, float b = 0F, float c = 0F, float d = 1F) => (X, Y, Z, W) = (a, b, c, d);

    public void MakeIdentity() => Set();

    public void Scale(float scale) => Set(X * scale, Y * scale, Z * scale, W * scale);

    public readonly Quaternion MakeClosest(Quaternion other)
    {
        var cosT = (other.X * X) + (other.Y * Y) + (other.Z * Z) + (other.W * W);
        return cosT < 0F ? Negate() : this;
    }

    public void Normalize()
    {
        var length2 = Length2;
        if (float.Abs(length2) < 0F)
        {
            return;
        }

        var invMag = VegasMath.InvSqrt(length2);
        Set(X * invMag, Y * invMag, Z * invMag, W * invMag);
    }

    public void RotateX(float theta)
    {
        Quaternion self = this;
        self *= new Quaternion(new Vector3(1, 0, 0), theta);
        Set(self.X, self.Y, self.Z, self.W);
    }

    public void RotateY(float theta)
    {
        Quaternion self = this;
        self *= new Quaternion(new Vector3(0, 1, 0), theta);
        Set(self.X, self.Y, self.Z, self.W);
    }

    public void RotateZ(float theta)
    {
        Quaternion self = this;
        self *= new Quaternion(new Vector3(0, 0, 1), theta);
        Set(self.X, self.Y, self.Z, self.W);
    }

    public void Randomize()
    {
        const int mask = 0xFFFF;
        const float dividend = 65_536F;
        Set(
            (VegasMath.Rand() & mask) / dividend,
            (VegasMath.Rand() & mask) / dividend,
            (VegasMath.Rand() & mask) / dividend,
            (VegasMath.Rand() & mask) / dividend
        );

        Normalize();
    }

    public readonly Vector3 RotateVector(Vector3 vector)
    {
        var x = (W * vector.X) + ((Y * vector.Z) - (vector.Y * Z));
        var y = (W * vector.Y) - ((X * vector.Z) - (vector.X * Z));
        var z = (W * vector.Z) + ((X * vector.Y) - (vector.X * Y));
        var w = -((X * vector.X) + (Y * vector.Y) + (Z * vector.Z));

        return new Vector3(
            (w * -X) + (W * x) + ((y * -Z) - (-Y * z)),
            (w * -Y) + (W * y) - ((x * -Z) - (-X * z)),
            (w * -Z) + (W * z) + ((x * -Y) - (-X * y))
        );
    }

    public readonly Quaternion Add(Quaternion other) => new(X + other.X, Y + other.Y, Z + other.Z, W + other.W);

    public readonly Quaternion Subtract(Quaternion other) => new(X - other.X, Y - other.Y, Z - other.Z, W - other.W);

    public readonly Quaternion Multiply(float scalar) => new(X * scalar, Y * scalar, Z * scalar, W * scalar);

    public readonly Quaternion Multiply(Quaternion other) =>
        new(
            (W * other.X) + (other.W * X) + ((Y * other.Z) - (other.Y * Z)),
            (W * other.Y) + (other.W * Y) - ((X * other.Z) - (other.X * Z)),
            (W * other.Z) + (other.W * Z) + ((X * other.Y) - (other.X * Y)),
            (W * other.W) - ((X * other.X) + (Y * other.Y) + (Z * other.Z))
        );

    public readonly Quaternion Divide(Quaternion other) => this * other.Inverse;

    public readonly Quaternion Plus() => new(+X, +Y, +Z, +W);

    public readonly Quaternion Negate() => new(-X, -Y, -Z, -W);

    public readonly Matrix3X3 ToMatrix3X3()
    {
        Matrix3X3 result = default;

        result.Row0[0] = 1F - (2F * ((this[1] * this[1]) + (this[2] * this[2])));
        result.Row0[1] = 2F * ((this[0] * this[1]) - (this[2] * this[3]));
        result.Row0[2] = 2F * ((this[2] * this[0]) + (this[1] * this[3]));

        result.Row1[0] = 2F * ((this[0] * this[1]) + (this[2] * this[3]));
        result.Row1[1] = 1F - (2F * ((this[2] * this[2]) + (this[0] * this[0])));
        result.Row1[2] = 2F * ((this[1] * this[2]) - (this[0] * this[3]));

        result.Row2[0] = 2F * ((this[2] * this[0]) - (this[1] * this[3]));
        result.Row2[1] = 2F * ((this[1] * this[2]) + (this[0] * this[3]));
        result.Row2[2] = 1F - (2F * ((this[1] * this[1]) + (this[0] * this[0])));

        return result;
    }

    public readonly Matrix3D ToMatrix3D()
    {
        Matrix3D result = default;

        result.Row0[0] = 1F - (2F * ((this[1] * this[1]) + (this[2] * this[2])));
        result.Row0[1] = 2F * ((this[0] * this[1]) - (this[2] * this[3]));
        result.Row0[2] = 2F * ((this[2] * this[0]) + (this[1] * this[3]));

        result.Row1[0] = 2F * ((this[0] * this[1]) + (this[2] * this[3]));
        result.Row1[1] = 1F - (2F * ((this[2] * this[2]) + (this[0] * this[0])));
        result.Row1[2] = 2F * ((this[1] * this[2]) - (this[0] * this[3]));

        result.Row2[0] = 2F * ((this[2] * this[0]) - (this[1] * this[3]));
        result.Row2[1] = 2F * ((this[1] * this[2]) + (this[0] * this[3]));
        result.Row2[2] = 1F - (2F * ((this[1] * this[1]) + (this[0] * this[0])));

        result.Row0[3] = result.Row1[3] = result.Row2[3] = 0F;

        return result;
    }

    public readonly Matrix4X4 ToMatrix4X4()
    {
        Matrix4X4 result = default;

        result.Row0[0] = 1F - (2F * ((this[1] * this[1]) + (this[2] * this[2])));
        result.Row0[1] = 2F * ((this[0] * this[1]) - (this[2] * this[3]));
        result.Row0[2] = 2F * ((this[2] * this[0]) + (this[1] * this[3]));

        result.Row1[0] = 2F * ((this[0] * this[1]) + (this[2] * this[3]));
        result.Row1[1] = 1F - (2F * ((this[2] * this[2]) + (this[0] * this[0])));
        result.Row1[2] = 2F * ((this[1] * this[2]) - (this[0] * this[3]));

        result.Row2[0] = 2F * ((this[2] * this[0]) - (this[1] * this[3]));
        result.Row2[1] = 2F * ((this[1] * this[2]) + (this[0] * this[3]));
        result.Row2[2] = 1F - (2F * ((this[1] * this[1]) + (this[0] * this[0])));

        result.Row0[3] = result.Row1[3] = result.Row2[3] = 0F;

        result.Row3[0] = result.Row3[1] = result.Row3[2] = 0F;
        result.Row3[3] = 1F;

        return result;
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Quaternion other && Equals(other);

    public readonly bool Equals(Quaternion other) =>
        float.Abs(X - other.X) < float.Epsilon
        && float.Abs(Y - other.Y) < float.Epsilon
        && float.Abs(Z - other.Z) < float.Epsilon
        && float.Abs(W - other.W) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    private static float ProjectToSphere(float r, float x, float y)
    {
        var d = float.Sqrt((x * x) + (y * y));

        if (d < r * (VegasMath.Sqrt2 / 2F))
        {
            return float.Sqrt((r * r) - (d * d));
        }

        var t = r / VegasMath.Sqrt2;
        return t * t / d;
    }

    public static Quaternion operator +(Quaternion x, Quaternion y) => x.Add(y);

    public static Quaternion operator -(Quaternion x, Quaternion y) => x.Subtract(y);

    public static Quaternion operator *(Quaternion quaternion, float scalar) => quaternion.Multiply(scalar);

    public static Quaternion operator *(float scalar, Quaternion quaternion) => quaternion.Multiply(scalar);

    public static Quaternion operator *(Quaternion x, Quaternion y) => x.Multiply(y);

    public static Quaternion operator /(Quaternion x, Quaternion y) => x.Divide(y);

    public static Quaternion operator +(Quaternion quaternion) => quaternion.Plus();

    public static Quaternion operator -(Quaternion quaternion) => quaternion.Negate();

    public static bool operator ==(Quaternion x, Quaternion y) => x.Equals(y);

    public static bool operator !=(Quaternion x, Quaternion y) => !x.Equals(y);

    public static explicit operator Quaternion(Matrix3X3 matrix) => FromMatrix3X3(matrix);

    public static explicit operator Quaternion(Matrix3D matrix) => FromMatrix3D(matrix);

    public static explicit operator Quaternion(Matrix4X4 matrix) => FromMatrix4X4(matrix);

    public static explicit operator Matrix3X3(Quaternion quaternion) => quaternion.ToMatrix3X3();

    public static explicit operator Matrix3D(Quaternion quaternion) => quaternion.ToMatrix3D();

    public float this[int index]
    {
        readonly get =>
            index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index must be between 0 and 3 inclusive."
                ),
            };
        set
        {
            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                case 3:
                    W = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        index,
                        "Index must be between 0 and 3 inclusive."
                    );
            }
        }
    }

    private static int Next(int index) =>
        index switch
        {
            0 => 1,
            1 => 2,
            2 => 0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                "Index must be between 0 and 2 inclusive."
            ),
        };
}
