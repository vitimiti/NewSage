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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NewSage.BaseTypes;

[StructLayout(LayoutKind.Sequential)]
public struct FCoord3D : IEquatable<FCoord3D>
{
    public float X;
    public float Y;
    public float Z;

    public static FCoord3D Zero => default;

    public readonly float Length => float.Sqrt((X * X) + (Y * Y) + (Z * Z));

    public readonly float LengthSqr => (X * X) + (Y * Y) + (Z * Z);

    public static FCoord3D CrossProduct(FCoord3D x, FCoord3D y) => x * y;

    public void Normalize()
    {
        var length = Length;
        X /= length;
        Y /= length;
        Z /= length;
    }

    public void Scale(float scale)
    {
        X *= scale;
        Y *= scale;
        Z *= scale;
    }

    public readonly FCoord3D Add(FCoord3D other) =>
        new()
        {
            X = X + other.X,
            Y = Y + other.Y,
            Z = Z + other.Z,
        };

    public readonly FCoord3D Subtract(FCoord3D other) =>
        new()
        {
            X = X - other.X,
            Y = Y - other.Y,
            Z = Z - other.Z,
        };

    public readonly FCoord3D Multiply(FCoord3D other) =>
        new()
        {
            X = X * other.X,
            Y = Y * other.Y,
            Z = Z * other.Z,
        };

    public readonly FCoord3D Multiply(float scalar) =>
        new()
        {
            X = X * scalar,
            Y = Y * scalar,
            Z = Z * scalar,
        };

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is FCoord3D other && Equals(other);

    public readonly bool Equals(FCoord3D other) =>
        Math.Abs(X - other.X) < float.Epsilon
        && Math.Abs(Y - other.Y) < float.Epsilon
        && Math.Abs(Z - other.Z) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    public static FCoord3D operator +(FCoord3D x, FCoord3D y) => x.Add(y);

    public static FCoord3D operator -(FCoord3D x, FCoord3D y) => x.Subtract(y);

    public static FCoord3D operator *(FCoord3D x, FCoord3D y) => x.Multiply(y);

    public static FCoord3D operator *(FCoord3D coord3D, float scalar) => coord3D.Multiply(scalar);

    public static bool operator ==(FCoord3D left, FCoord3D right) => left.Equals(right);

    public static bool operator !=(FCoord3D left, FCoord3D right) => !left.Equals(right);
}
