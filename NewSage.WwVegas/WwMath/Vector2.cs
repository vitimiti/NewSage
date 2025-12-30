// -----------------------------------------------------------------------
// <copyright file="Vector2.cs" company="NewSage">
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

[StructLayout(LayoutKind.Explicit)]
public struct Vector2 : IEquatable<Vector2>
{
    public Vector2() { }

    public Vector2(Vector2 other) => Set(other);

    public Vector2(float x, float y) => Set(x, y);

    public Vector2(ReadOnlySpan<float> values)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(values.Length, 2);
        Set(values[0], values[1]);
    }

    [FieldOffset(0)]
    public float X;

    [FieldOffset(0)]
    public float U;

    [FieldOffset(4)]
    public float Y;

    [FieldOffset(4)]
    public float V;

    public readonly float Length2 => (X * X) + (Y * Y);

    public readonly float Length => float.Sqrt(Length2);

    public readonly bool IsValid => WwMath.IsValid(X) && WwMath.IsValid(Y);

    public static Vector2 Normalize(Vector2 vector)
    {
        var length2 = vector.Length2;
        if (float.Abs(length2) < float.Epsilon)
        {
            return new Vector2(0, 0);
        }

        var oneOverLength = WwMath.InvSqrt(length2);
        return vector / oneOverLength;
    }

    public static float DotProduct(Vector2 x, Vector2 y) => x.Multiply(y);

    public static float PerpendicularDotProduct(Vector2 x, Vector2 y) => (x.X * -y.Y) + (x.Y * y.X);

    public static float Distance(Vector2 x, Vector2 y) => Distance(x.X, x.Y, y.X, y.Y);

    public static float Distance(float x1, float y1, float x2, float y2) =>
        float.Sqrt(float.Pow(x1 - x2, 2) + float.Pow(y1 - y2, 2));

    public static float QuickDistance(Vector2 x, Vector2 y) => QuickDistance(x.X, x.Y, y.X, y.Y);

    public static float QuickDistance(float x1, float y1, float x2, float y2)
    {
        var xDiff = float.Abs(x1 - x2);
        var yDiff = float.Abs(y1 - y2);

        return xDiff > yDiff ? (yDiff / 2) + xDiff : (xDiff / 2) + yDiff;
    }

    public static Vector2 Lerp(Vector2 x, Vector2 y, float t) => new(x.X + ((y.X - x.X) * t), x.Y + ((y.Y - x.Y) * t));

    public void Set(float x, float y) => (X, Y) = (x, y);

    public void Set(Vector2 other) => (X, Y) = (other.X, other.Y);

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
    }

    public void Rotate(float theta) => Rotate(float.Sin(theta), float.Cos(theta));

    public void Rotate(float sin, float cos)
    {
        var newX = (X * cos) + (Y * -sin);
        var newY = (X * sin) + (Y * cos);
        X = newX;
        Y = newY;
    }

    public bool RotateTowards(Vector2 target, float maxTheta, out bool positiveTurn) =>
        RotateTowards(target, float.Sin(maxTheta), float.Cos(maxTheta), out positiveTurn);

    public bool RotateTowards(Vector2 target, float maxSin, float maxCos, out bool positiveTurn)
    {
        var returnValue = false;
        positiveTurn = PerpendicularDotProduct(target, this) > 0;

        if (DotProduct(this, target) >= maxCos)
        {
            Set(target);
            returnValue = true;
        }
        else
        {
            if (positiveTurn)
            {
                Rotate(maxSin, maxCos);
            }
            else
            {
                Rotate(-maxSin, maxCos);
            }
        }

        return returnValue;
    }

    public void UpdateMin(Vector2 other)
    {
        X = float.Min(X, other.X);
        Y = float.Min(Y, other.Y);
    }

    public void UpdateMax(Vector2 other)
    {
        X = float.Max(X, other.X);
        Y = float.Max(Y, other.Y);
    }

    public void Scale(float x, float y)
    {
        X *= x;
        Y *= y;
    }

    public void Scale(Vector2 other)
    {
        X *= other.X;
        Y *= other.Y;
    }

    public readonly Vector2 Add(Vector2 other) => new(X + other.X, Y + other.Y);

    public readonly Vector2 Subtract(Vector2 other) => new(X - other.X, Y - other.Y);

    public readonly Vector2 Multiply(float scalar) => new(X * scalar, Y * scalar);

    public readonly float Multiply(Vector2 other) => (X * other.X) + (Y * other.Y);

    public readonly Vector2 Divide(float scalar)
    {
        var oneOverScalar = 1F / scalar;
        return Multiply(oneOverScalar);
    }

    public readonly Vector2 Plus() => new(+X, +Y);

    public readonly Vector2 Negate() => new(-X, -Y);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Vector2 other && Equals(other);

    public readonly bool Equals(Vector2 other) =>
        float.Abs(X - other.X) < float.Epsilon && float.Abs(Y - other.Y) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    public override readonly string ToString() => $"({X}, {Y})";

    public static Vector2 operator +(Vector2 x, Vector2 y) => x.Add(y);

    public static Vector2 operator -(Vector2 x, Vector2 y) => x.Subtract(y);

    public static Vector2 operator *(Vector2 vector, float scalar) => vector.Multiply(scalar);

    public static Vector2 operator *(float scalar, Vector2 vector) => vector.Multiply(scalar);

    public static float operator *(Vector2 x, Vector2 y) => x.Multiply(y);

    public static Vector2 operator /(Vector2 vector, float scalar) => vector.Divide(scalar);

    public static Vector2 operator +(Vector2 vector) => vector.Plus();

    public static Vector2 operator -(Vector2 vector) => vector.Negate();

    public static bool operator ==(Vector2 x, Vector2 y) => x.Equals(y);

    public static bool operator !=(Vector2 x, Vector2 y) => !x.Equals(y);

    public float this[int index]
    {
        readonly get =>
            index switch
            {
                0 => X,
                1 => Y,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index must be between 0 and 1 inclusive."
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 1 inclusive.");
            }
        }
    }
}
