// -----------------------------------------------------------------------
// <copyright file="FCoord3D.cs" company="NewSage">
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

namespace NewSage.BaseTypes;

public record FCoord3D(float X, float Y, float Z)
{
    public static FCoord3D Zero => new(0F, 0F, 0F);

    public float Length => float.Sqrt((X * X) + (Y * Y) + (Z * Z));

    public float LengthSqr => (X * X) + (Y * Y) + (Z * Z);

    public FCoord3D Normalized
    {
        get
        {
            var length = Length;
            return float.Abs(length) > float.Epsilon ? new FCoord3D(X / length, Y / length, Z / length) : this;
        }
    }

    public static FCoord3D CrossProduct(FCoord3D x, FCoord3D y) => x * y;

    public FCoord3D Scale(float scale) => Multiply(scale);

    public FCoord3D Add(FCoord3D other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new FCoord3D(X + other.X, Y + other.Y, Z + other.Z);
    }

    public FCoord3D Subtract(FCoord3D other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new FCoord3D(X - other.X, Y - other.Y, Z - other.Z);
    }

    public FCoord3D Multiply(FCoord3D other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new FCoord3D(X * other.X, Y * other.Y, Z * other.Z);
    }

    public FCoord3D Multiply(float scalar) => new(X * scalar, Y * scalar, Z * scalar);

    public static FCoord3D operator +(FCoord3D x, FCoord3D y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Add(y);
    }

    public static FCoord3D operator -(FCoord3D x, FCoord3D y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Subtract(y);
    }

    public static FCoord3D operator *(FCoord3D x, FCoord3D y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Multiply(y);
    }

    public static FCoord3D operator *(FCoord3D coord3D, float scalar)
    {
        ArgumentNullException.ThrowIfNull(coord3D);
        return coord3D.Multiply(scalar);
    }
}
