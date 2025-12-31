// -----------------------------------------------------------------------
// <copyright file="Vector4.cs" company="NewSage">
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
public struct Vector4 : IEquatable<Vector4>
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector4(Vector4 other) => Set(other.X, other.Y, other.Z, other.W);

    public Vector4(float x, float y, float z, float w) => Set(x, y, z, w);

    public Vector4(ReadOnlySpan<float> values)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(values.Length, 4);
        Set(values[0], values[1], values[2], values[3]);
    }

    public readonly float Length2 => (X * X) + (Y * Y) + (Z * Z) + (W * W);

    public readonly float Length => float.Sqrt(Length2);

    public readonly bool IsValid =>
        VegasMath.IsValid(X) && VegasMath.IsValid(Y) && VegasMath.IsValid(Z) && VegasMath.IsValid(W);

    public static Vector4 Normalize(Vector4 vector)
    {
        var length2 = vector.Length2;
        if (float.Abs(length2) < float.Epsilon)
        {
            return new Vector4(0, 0, 0, 0);
        }

        var oneOverLength = VegasMath.InvSqrt(length2);
        return vector * oneOverLength;
    }

    public static float DotProduct(Vector4 x, Vector4 y) => x.Multiply(y);

    public static Vector4 Lerp(Vector4 x, Vector4 y, float alpha) =>
        new(
            x.X + ((y.X - x.X) * alpha),
            x.Y + ((y.Y - x.Y) * alpha),
            x.Z + ((y.Z - x.Z) * alpha),
            x.W + ((y.W - x.W) * alpha)
        );

    public void Set(float x, float y, float z, float w) => (X, Y, Z, W) = (x, y, z, w);

    public void Normalize()
    {
        var length2 = Length2;
        if (float.Abs(length2) < float.Epsilon)
        {
            return;
        }

        var oneOverLength = VegasMath.InvSqrt(length2);
        X *= oneOverLength;
        Y *= oneOverLength;
        Z *= oneOverLength;
        W *= oneOverLength;
    }

    public readonly Vector4 Add(Vector4 other) => new(X + other.X, Y + other.Y, Z + other.Z, W + other.W);

    public readonly Vector4 Subtract(Vector4 other) => new(X - other.X, Y - other.Y, Z - other.Z, W - other.W);

    public readonly Vector4 Multilpy(float scalar) => new(X * scalar, Y * scalar, Z * scalar, W * scalar);

    public readonly float Multiply(Vector4 other) => (X * other.X) + (Y * other.Y) + (Z * other.Z) + (W * other.W);

    public readonly Vector4 Divide(float scalar)
    {
        scalar = 1F / scalar;
        return Multilpy(scalar);
    }

    public readonly Vector4 Plus() => new(+X, +Y, +Z, +W);

    public readonly Vector4 Negate() => new(-X, -Y, -Z, -W);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Vector4 other && Equals(other);

    public readonly bool Equals(Vector4 other) =>
        float.Abs(X - other.X) < float.Epsilon
        && float.Abs(Y - other.Y) < float.Epsilon
        && float.Abs(Z - other.Z) < float.Epsilon
        && float.Abs(W - other.W) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    public static Vector4 operator +(Vector4 x, Vector4 y) => x.Add(y);

    public static Vector4 operator -(Vector4 x, Vector4 y) => x.Subtract(y);

    public static Vector4 operator *(Vector4 vector, float scalar) => vector.Multilpy(scalar);

    public static Vector4 operator *(float scalar, Vector4 vector) => vector.Multilpy(scalar);

    public static float operator *(Vector4 x, Vector4 y) => x.Multiply(y);

    public static Vector4 operator /(Vector4 vector, float scalar) => vector.Divide(scalar);

    public static Vector4 operator +(Vector4 vector) => vector.Plus();

    public static Vector4 operator -(Vector4 vector) => vector.Negate();

    public static bool operator ==(Vector4 x, Vector4 y) => x.Equals(y);

    public static bool operator !=(Vector4 x, Vector4 y) => !x.Equals(y);

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
}
