// -----------------------------------------------------------------------
// <copyright file="Point3D`1.cs" company="NewSage">
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
using System.Numerics;
using System.Runtime.InteropServices;

namespace NewSage.WwVegas;

[StructLayout(LayoutKind.Sequential)]
public struct Point3D<TNumber> : IEquatable<Point3D<TNumber>>
    where TNumber : INumber<TNumber>
{
    public Point3D() => (X, Y, Z) = (TNumber.Zero, TNumber.Zero, TNumber.Zero);

    public Point3D(TNumber x, TNumber y, TNumber z) => (X, Y, Z) = (x, y, z);

    public TNumber X;
    public TNumber Y;
    public TNumber Z;

    public readonly Point3D<TNumber> DotProduct(Point3D<TNumber> other) => new(X * other.X, Y * other.Y, Z * other.Z);

    public readonly Point3D<TNumber> CrossProduct(Point3D<TNumber> other) =>
        new((Y * other.Z) - (Z * other.Y), (Z * other.X) - (X * other.Z), (X * other.Y) - (Y * other.X));

    public readonly Point3D<TNumber> Add(Point3D<TNumber> other) => new(X + other.X, Y + other.Y, Z + other.Z);

    public readonly Point3D<TNumber> Add(Point2D<TNumber> other) => new(X + other.X, Y + other.Y, Z);

    public readonly Point3D<TNumber> Subtract(Point3D<TNumber> other) => new(X - other.X, Y - other.Y, Z - other.Z);

    public readonly Point3D<TNumber> Subtract(Point2D<TNumber> other) => new(X - other.X, Y - other.Y, Z);

    public readonly Point3D<TNumber> Multiply(TNumber scalar) => new(X * scalar, Y * scalar, Z * scalar);

    public readonly Point3D<TNumber> Multiply(Point3D<TNumber> other) => new(X * other.X, Y * other.Y, Z * other.Z);

    public readonly Point3D<TNumber> Divide(TNumber scalar) =>
        scalar == TNumber.Zero
            ? new Point3D<TNumber>(TNumber.Zero, TNumber.Zero, TNumber.Zero)
            : new Point3D<TNumber>(X / scalar, Y / scalar, Z / scalar);

    public readonly Point3D<TNumber> Negate() => new(-X, -Y, -Z);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Point3D<TNumber> other && Equals(other);

    public readonly bool Equals(Point3D<TNumber> other) => X == other.X && Y == other.Y && Z == other.Z;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    public static Point3D<TNumber> operator +(Point3D<TNumber> x, Point3D<TNumber> y) => x.Add(y);

    public static Point3D<TNumber> operator +(Point3D<TNumber> x, Point2D<TNumber> y) => x.Add(y);

    public static Point3D<TNumber> operator -(Point3D<TNumber> x, Point3D<TNumber> y) => x.Subtract(y);

    public static Point3D<TNumber> operator -(Point3D<TNumber> x, Point2D<TNumber> y) => x.Subtract(y);

    public static Point3D<TNumber> operator *(Point3D<TNumber> point, TNumber scalar) => point.Multiply(scalar);

    public static Point3D<TNumber> operator *(Point3D<TNumber> point, Point3D<TNumber> other) => point.Multiply(other);

    public static Point3D<TNumber> operator /(Point3D<TNumber> point, TNumber scalar) => point.Divide(scalar);

    public static Point3D<TNumber> operator -(Point3D<TNumber> point) => point.Negate();

    public static bool operator ==(Point3D<TNumber> x, Point3D<TNumber> y) => x.Equals(y);

    public static bool operator !=(Point3D<TNumber> x, Point3D<TNumber> y) => !x.Equals(y);
}
