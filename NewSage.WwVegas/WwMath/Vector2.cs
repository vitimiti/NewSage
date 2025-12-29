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

namespace NewSage.WwVegas.WwMath;

public record Vector2(float X, float Y)
{
    public Vector2(ReadOnlySpan<float> values)
        : this(values[0], values[1]) { }

    public float U => X;

    public float V => Y;

    public float Length2 => (X * X) + (Y * Y);

    public float Length => float.Sqrt(Length2);

    public Vector2 Normalized
    {
        get
        {
            var length2 = Length2;
            return float.Abs(length2) > float.Epsilon ? new Vector2(X / length2, Y / length2) : this;
        }
    }

    public bool IsValid => WwMath.IsValid(X) && WwMath.IsValid(Y);

    public static float DotProduct(Vector2 x, Vector2 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Multiply(y);
    }

    public static float PerpendicularDotProduct(Vector2 x, Vector2 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        return (x.X * -y.Y) + (x.Y * y.X);
    }

    public static float Distance(Vector2 x, Vector2 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        return Distance(x.X, x.Y, y.X, y.Y);
    }

    public static float Distance(float x1, float y1, float x2, float y2) =>
        float.Sqrt(float.Pow(x1 - x2, 2) + float.Pow(y1 - y2, 2));

    public static float QuickDistance(Vector2 x, Vector2 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        return QuickDistance(x.X, x.Y, y.X, y.Y);
    }

    public static float QuickDistance(float x1, float y1, float x2, float y2)
    {
        var xDiff = float.Abs(x1 - x2);
        var yDiff = float.Abs(y1 - y2);

        return xDiff > yDiff ? (yDiff / 2) + xDiff : (xDiff / 2) + yDiff;
    }

    public static Vector2 Lerp(Vector2 x, Vector2 y, float t)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        return new Vector2(x.X + ((y.X - x.X) * t), x.Y + ((y.Y - x.Y) * t));
    }

    public Vector2 Rotate(float theta) => Rotate(float.Sin(theta), float.Cos(theta));

    public Vector2 Rotate(float sin, float cos) => new((X * cos) + (Y * -sin), (X * sin) + (Y * cos));

    public bool RotateTowards(Vector2 target, float maxTheta, out Vector2 result, out bool positiveTurn) =>
        RotateTowards(target, float.Sin(maxTheta), float.Cos(maxTheta), out result, out positiveTurn);

    public bool RotateTowards(Vector2 target, float maxSin, float maxCos, out Vector2 result, out bool positiveTurn)
    {
        var returnValue = false;
        positiveTurn = PerpendicularDotProduct(target, this) > 0F;

        if (DotProduct(this, target) >= maxCos)
        {
            result = target;
            returnValue = true;
        }
        else
        {
            result = positiveTurn ? Rotate(maxSin, maxCos) : Rotate(-maxSin, maxCos);
        }

        return returnValue;
    }

    public Vector2 UpdateMin(Vector2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Vector2(float.Min(X, other.X), float.Min(Y, other.Y));
    }

    public Vector2 UpdateMax(Vector2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Vector2(float.Max(X, other.X), float.Max(Y, other.Y));
    }

    public Vector2 Scale(float x, float y) => new(X * x, Y * y);

    public Vector2 Scale(Vector2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Scale(other.X, other.Y);
    }

    public Vector2 Add(Vector2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Vector2(X + other.X, Y + other.Y);
    }

    public Vector2 Subtract(Vector2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Vector2(X - other.X, Y - other.Y);
    }

    public Vector2 Multiply(float scalar) => new(X * scalar, Y * scalar);

    public float Multiply(Vector2 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return (X * other.X) + (Y * other.Y);
    }

    public Vector2 Divide(float scalar)
    {
        var oneOverScalar = 1F / scalar;
        return Multiply(oneOverScalar);
    }

    public Vector2 Plus() => new(+X, +Y);

    public Vector2 Negate() => new(-X, -Y);

    public static Vector2 operator +(Vector2 x, Vector2 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Add(y);
    }

    public static Vector2 operator -(Vector2 x, Vector2 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Subtract(y);
    }

    public static Vector2 operator *(Vector2 vector, float scalar)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Multiply(scalar);
    }

    public static Vector2 operator *(float scalar, Vector2 vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Multiply(scalar);
    }

    public static float operator *(Vector2 x, Vector2 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Multiply(y);
    }

    public static Vector2 operator /(Vector2 vector, float scalar)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Divide(scalar);
    }

    public static Vector2 operator +(Vector2 vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Plus();
    }

    public static Vector2 operator -(Vector2 vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Negate();
    }

    public float this[int index] =>
        index switch
        {
            0 => X,
            1 => Y,
            _ => throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                $"Index {index} is out of range for {nameof(Vector2)}"
            ),
        };
}
