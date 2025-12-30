// -----------------------------------------------------------------------
// <copyright file="Vector3.cs" company="NewSage">
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
public struct Vector3 : IEquatable<Vector3>
{
    public Vector3() { }

    public Vector3(Vector3 other) => (X, Y, Z) = (other.X, other.Y, other.Z);

    public Vector3(float x, float y, float z) => (X, Y, Z) = (x, y, z);

    public Vector3(ReadOnlySpan<float> values)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(values.Length, 3);
        (X, Y, Z) = (values[0], values[1], values[2]);
    }

    public float X;
    public float Y;
    public float Z;

    public readonly float Length2 => (X * X) + (Y * Y) + (Z * Z);

    public readonly float Length => float.Sqrt(Length2);

    public readonly float QuickLength
    {
        get
        {
            var max = float.Abs(X);
            var mid = float.Abs(Y);
            var min = float.Abs(Z);
            float tmp;

            if (max < mid)
            {
                tmp = max;
                max = mid;
                mid = tmp;
            }

            if (max < min)
            {
                tmp = max;
                max = min;
                min = tmp;
            }

            if (mid < min)
            {
                mid = min;
                min = mid;
            }

            return max + (11F / 32F * mid) + (1F / 4F * min);
        }
    }

    public readonly bool IsValid => WwMath.IsValid(X) && WwMath.IsValid(Y) && WwMath.IsValid(Z);

    public static Vector3 Normalize(Vector3 vector)
    {
        var length2 = vector.Length2;
        if (float.Abs(length2) < float.Epsilon)
        {
            return vector;
        }

        var oneOverLength = WwMath.InvSqrt(length2);
        return vector * oneOverLength;
    }

    public static float DotProduct(Vector3 x, Vector3 y) => x.Multiply(y);

    public static Vector3 CrossProduct(Vector3 x, Vector3 y) =>
        new((x.Y * y.Z) - (x.Z * y.Y), (x.Z * y.X) - (x.X * y.Z), (x.X * y.Y) - (x.Y * y.X));

    public static Vector3 NormalizedCrossProduct(Vector3 x, Vector3 y)
    {
        Vector3 crossProduct = CrossProduct(x, y);
        crossProduct.Normalize();
        return crossProduct;
    }

    public static float CrossProductX(Vector3 x, Vector3 y) => (x.Y * y.Z) - (x.Z * y.Y);

    public static float CrossProductY(Vector3 x, Vector3 y) => (x.Z * y.X) - (x.X * y.Z);

    public static float CrossProductZ(Vector3 x, Vector3 y) => (x.X * y.Y) - (x.Y * y.X);

    public static float FindXAtY(float y, Vector3 p1, Vector3 p2) =>
        p1.X + ((y - p1.Y) * ((p2.X - p1.X) / (p2.Y - p1.Y)));

    public static float FindXAtZ(float z, Vector3 p1, Vector3 p2) =>
        p1.X + ((z - p1.Z) * ((p2.X - p1.X) / (p2.Z - p1.Z)));

    public static float FinxYAtX(float x, Vector3 p1, Vector3 p2) =>
        p1.Y + ((x - p1.X) * ((p2.Y - p1.Y) / (p2.X - p1.X)));

    public static float FindYAtZ(float z, Vector3 p1, Vector3 p2) =>
        p1.Y + ((z - p1.Z) * ((p2.Y - p1.Y) / (p2.Z - p1.Z)));

    public static float FindZAtX(float x, Vector3 p1, Vector3 p2) =>
        p1.Z + ((x - p1.X) * ((p2.Z - p1.Z) / (p2.X - p1.X)));

    public static float FindZAtY(float y, Vector3 p1, Vector3 p2) =>
        p1.Z + ((y - p1.Y) * ((p2.Z - p1.Z) / (p2.Y - p1.Y)));

    public static float QuickDistance(Vector3 p1, Vector3 p2) => (p1 - p2).QuickLength;

    public static float Distance(Vector3 p1, Vector3 p2) => (p1 - p2).Length;

    public static Vector3 Lerp(Vector3 x, Vector3 y, float alpha) =>
        new(x.X + ((y.X - x.X) * alpha), x.Y + ((y.Y - x.Y) * alpha), x.Z + ((y.Z - x.Z) * alpha));

    public void Set(float x, float y, float z) => (X, Y, Z) = (x, y, z);

    public void Set(Vector3 other) => (X, Y, Z) = (other.X, other.Y, other.Z);

    public void Normalize()
    {
        var length2 = Length2;
        if (float.Abs(length2) < float.Epsilon)
        {
            return;
        }

        var oneOverLength = WwMath.InvSqrt(length2);
        X *= oneOverLength;
        Y *= oneOverLength;
        Z *= oneOverLength;
    }

    public void Scale(float scale)
    {
        X *= scale;
        Y *= scale;
        Z *= scale;
    }

    public void RotateX(float angle) => RotateX(float.Sin(angle), float.Cos(angle));

    public void RotateX(float sin, float cos)
    {
        var tempY = Y;
        var tempZ = Z;
        Y = (cos * tempY) - (sin * tempZ);
        Z = (sin * tempY) + (cos * tempZ);
    }

    public void RotateY(float angle) => RotateY(float.Sin(angle), float.Cos(angle));

    public void RotateY(float sin, float cos)
    {
        var tempX = X;
        var tempZ = Z;
        X = (cos * tempX) + (sin * tempZ);
        Z = (-sin * tempX) + (cos * tempZ);
    }

    public void RotateZ(float angle) => RotateZ(float.Sin(angle), float.Cos(angle));

    public void RotateZ(float sin, float cos)
    {
        var tempX = X;
        var tempY = Y;
        X = (cos * tempX) - (sin * tempY);
        Y = (sin * tempX) + (cos * tempY);
    }

    public void UpdateMin(Vector3 other)
    {
        X = float.Min(X, other.X);
        Y = float.Min(Y, other.Y);
        Z = float.Min(Z, other.Z);
    }

    public void UpdateMax(Vector3 other)
    {
        X = float.Max(X, other.X);
        Y = float.Max(Y, other.Y);
        Z = float.Max(Z, other.Z);
    }

    public void CapAbsoluteTo(Vector3 other)
    {
        X = CapValue(X, other.X);
        Y = CapValue(Y, other.Y);
        Z = CapValue(Z, other.Z);

        return;

        static float CapValue(float value, float otherValue)
        {
            if (value > 0)
            {
                if (otherValue < value)
                {
                    return otherValue;
                }
            }
            else
            {
                if (-otherValue > value)
                {
                    return -otherValue;
                }
            }

            return value;
        }
    }

    public readonly ulong ToAbgr() =>
        (ulong)((255 << 24) | ((int)(Z * 255F) << 16) | ((int)(Y * 255F) << 8) | (int)(X * 255F));

    public readonly ulong ToArgb() =>
        (ulong)((255 << 24) | ((int)(X * 255F) << 16) | ((int)(Y * 255F) << 8) | (int)(Z * 255F));

    public readonly ulong ToArgb(float alpha) =>
        (ulong)(((int)(alpha * 255) << 24) | ((int)(X * 255F) << 16) | ((int)(Y * 255F) << 8) | (int)(Z * 255F));

    public readonly Vector3 Add(Vector3 other) => new(X + other.X, Y + other.Y, Z + other.Z);

    public readonly Vector3 Subtract(Vector3 other) => new(X - other.X, Y - other.Y, Z - other.Z);

    public readonly Vector3 Multiply(float scalar) => new(X * scalar, Y * scalar, Z * scalar);

    public readonly float Multiply(Vector3 other) => (X * other.X) + (Y * other.Y) + (Z * other.Z);

    public readonly Vector3 Divide(float scalar)
    {
        var oneOverScalar = 1F / scalar;
        return Multiply(oneOverScalar);
    }

    public readonly Vector3 Plus() => new(+X, +Y, +Z);

    public readonly Vector3 Negate() => new(-X, -Y, -Z);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Vector3 vector && Equals(vector);

    public readonly bool Equals(Vector3 other) =>
        float.Abs(X - other.X) < float.Epsilon
        && float.Abs(Y - other.Y) < float.Epsilon
        && float.Abs(Z - other.Z) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    public override readonly string ToString() => $"({X}, {Y}, {Z})";

    public static Vector3 operator +(Vector3 x, Vector3 y) => x.Add(y);

    public static Vector3 operator -(Vector3 x, Vector3 y) => x.Subtract(y);

    public static Vector3 operator *(Vector3 vector, float scalar) => vector.Multiply(scalar);

    public static Vector3 operator *(float scalar, Vector3 vector) => vector.Multiply(scalar);

    public static float operator *(Vector3 x, Vector3 y) => x.Multiply(y);

    public static Vector3 operator /(Vector3 vector, float scalar) => vector.Divide(scalar);

    public static Vector3 operator +(Vector3 vector) => vector.Plus();

    public static Vector3 operator -(Vector3 vector) => vector.Negate();

    public static bool operator ==(Vector3 x, Vector3 y) => x.Equals(y);

    public static bool operator !=(Vector3 x, Vector3 y) => !x.Equals(y);

    public float this[int index]
    {
        readonly get =>
            index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index must be between 0 and 2 inclusive."
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 2 inclusive.");
            }
        }
    }
}
