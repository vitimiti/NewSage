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

namespace NewSage.WwVegas.WwMath;

public record Vector3(float X, float Y, float Z)
{
    public Vector3(ReadOnlySpan<float> values)
        : this(values[0], values[1], values[2]) { }

    public float Length2 => (X * X) + (Y * Y) + (Z * Z);

    public float Length => float.Sqrt(Length2);

    public float QuickLength
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

    public Vector3 Normalized
    {
        get
        {
            var length = Length2;
            if (float.Abs(length) >= float.Epsilon)
            {
                var oneOverLength = WwMath.InvSqrt(length);
                return new Vector3(X * oneOverLength, Y * oneOverLength, Z * oneOverLength);
            }

            return this;
        }
    }

    public bool IsValid => WwMath.IsValid(X) && WwMath.IsValid(Y) && WwMath.IsValid(Z);

    public static float DotProduct(Vector3 x, Vector3 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Multiply(y);
    }

    public static Vector3 CrossProduct(Vector3 x, Vector3 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        return new Vector3((x.Y * y.Z) - (x.Z * y.Y), (x.Z * y.X) - (x.X * y.Z), (x.X * y.Y) - (x.Y * y.X));
    }

    public static Vector3 NormalizedCrossProduct(Vector3 x, Vector3 y) => CrossProduct(x, y).Normalized;

    public static float CrossProductX(Vector3 x, Vector3 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        return (x.Y * y.Z) - (x.Z * y.Y);
    }

    public static float CrossProductY(Vector3 x, Vector3 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        return (x.Z * y.X) - (x.X * y.Z);
    }

    public static float CrossProductZ(Vector3 x, Vector3 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        return (x.X * y.Y) - (x.Y * y.X);
    }

    public static float FindXAtY(float y, Vector3 p1, Vector3 p2)
    {
        ArgumentNullException.ThrowIfNull(p1);
        ArgumentNullException.ThrowIfNull(p2);

        return p1.X + ((y - p1.Y) * ((p2.X - p1.X) / (p2.Y - p1.Y)));
    }

    public static float FindXAtZ(float z, Vector3 p1, Vector3 p2)
    {
        ArgumentNullException.ThrowIfNull(p1);
        ArgumentNullException.ThrowIfNull(p2);

        return p1.X + ((z - p1.Z) * ((p2.X - p1.X) / (p2.Z - p1.Z)));
    }

    public static float FinxYAtX(float x, Vector3 p1, Vector3 p2)
    {
        ArgumentNullException.ThrowIfNull(p1);
        ArgumentNullException.ThrowIfNull(p2);

        return p1.Y + ((x - p1.X) * ((p2.Y - p1.Y) / (p2.X - p1.X)));
    }

    public static float FindYAtZ(float z, Vector3 p1, Vector3 p2)
    {
        ArgumentNullException.ThrowIfNull(p1);
        ArgumentNullException.ThrowIfNull(p2);

        return p1.Y + ((z - p1.Z) * ((p2.Y - p1.Y) / (p2.Z - p1.Z)));
    }

    public static float FindZAtX(float x, Vector3 p1, Vector3 p2)
    {
        ArgumentNullException.ThrowIfNull(p1);
        ArgumentNullException.ThrowIfNull(p2);

        return p1.Z + ((x - p1.X) * ((p2.Z - p1.Z) / (p2.X - p1.X)));
    }

    public static float FindZAtY(float y, Vector3 p1, Vector3 p2)
    {
        ArgumentNullException.ThrowIfNull(p1);
        ArgumentNullException.ThrowIfNull(p2);

        return p1.Z + ((y - p1.Y) * ((p2.Z - p1.Z) / (p2.Y - p1.Y)));
    }

    public static float QuickDistance(Vector3 p1, Vector3 p2) => (p1 - p2).QuickLength;

    public static float Distance(Vector3 p1, Vector3 p2) => (p1 - p2).Length;

    public static Vector3 Lerp(Vector3 x, Vector3 y, float alpha)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        return new Vector3(x.X + ((y.X - x.X) * alpha), x.Y + ((y.Y - x.Y) * alpha), x.Z + ((y.Z - x.Z) * alpha));
    }

    public Vector3 Scaled(float s) => Multiply(s);

    public Vector3 XRotated(float angle) => XRotated(float.Sin(angle), float.Cos(angle));

    public Vector3 XRotated(float sin, float cos) => new(X, (cos * Y) - (sin * Z), (sin * Y) + (cos * Z));

    public Vector3 YRotated(float angle) => YRotated(float.Sin(angle), float.Cos(angle));

    public Vector3 YRotated(float sin, float cos) => new((cos * X) + (sin * Z), Y, (-sin * X) + (cos * Z));

    public Vector3 ZRotated(float angle) => ZRotated(float.Sin(angle), float.Cos(angle));

    public Vector3 ZRotated(float sin, float cos) => new((cos * X) - (sin * Y), (sin * X) + (cos * Y), Z);

    public Vector3 UpdateMin(Vector3 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Vector3(float.Min(X, other.X), float.Min(Y, other.Y), float.Min(Z, other.Z));
    }

    public Vector3 UpdateMax(Vector3 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Vector3(float.Max(X, other.X), float.Max(Y, other.Y), float.Max(Z, other.Z));
    }

    public Vector3 CapAbsoluteTo(Vector3 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var x = X;
        var y = Y;
        var z = Z;

        x = CapValue(x, other.X);
        y = CapValue(y, other.Y);
        z = CapValue(z, other.Z);

        return new Vector3(x, y, z);

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

    public ulong ToAbgr() => (ulong)((255 << 24) | ((int)(Z * 255F) << 16) | ((int)(Y * 255F) << 8) | (int)(X * 255F));

    public ulong ToArgb() => (ulong)((255 << 24) | ((int)(X * 255F) << 16) | ((int)(Y * 255F) << 8) | (int)(Z * 255F));

    public ulong ToArgb(float alpha) =>
        (ulong)(((int)(alpha * 255) << 24) | ((int)(X * 255F) << 16) | ((int)(Y * 255F) << 8) | (int)(Z * 255F));

    public Vector3 Add(Vector3 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Vector3(X + other.X, Y + other.Y, Z + other.Z);
    }

    public Vector3 Subtract(Vector3 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Vector3(X - other.X, Y - other.Y, Z - other.Z);
    }

    public Vector3 Multiply(float scalar) => new(X * scalar, Y * scalar, Z * scalar);

    public float Multiply(Vector3 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return (X * other.X) + (Y * other.Y) + (Z * other.Z);
    }

    public Vector3 Divide(float scalar)
    {
        var oneOverScalar = 1F / scalar;
        return Multiply(oneOverScalar);
    }

    public Vector3 Plus() => new(X, Y, Z);

    public Vector3 Negate() => new(-X, -Y, -Z);

    public static Vector3 operator +(Vector3 x, Vector3 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Add(y);
    }

    public static Vector3 operator -(Vector3 x, Vector3 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Subtract(y);
    }

    public static Vector3 operator *(Vector3 vector, float scalar)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Multiply(scalar);
    }

    public static Vector3 operator *(float scalar, Vector3 vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Multiply(scalar);
    }

    public static float operator *(Vector3 x, Vector3 y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Multiply(y);
    }

    public static Vector3 operator /(Vector3 vector, float scalar)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Divide(scalar);
    }

    public static Vector3 operator +(Vector3 vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Plus();
    }

    public static Vector3 operator -(Vector3 vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return vector.Negate();
    }

    public float this[int index] =>
        index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException(
                nameof(index),
                $"Index {index} is out of range for {nameof(Vector3)}"
            ),
        };
}
