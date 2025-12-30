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

using System.Numerics;

namespace NewSage.WwVegas;

public record Point3D<TNumber>(TNumber X, TNumber Y, TNumber Z)
    where TNumber : INumber<TNumber>
{
    public Point3D<TNumber> DotProduct(Point3D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point3D<TNumber>(X * other.X, Y * other.Y, Z * other.Z);
    }

    public Point3D<TNumber> CrossProduct(Point3D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point3D<TNumber>(
            (Y * other.Z) - (Z * other.Y),
            (Z * other.X) - (X * other.Z),
            (X * other.Y) - (Y * other.X)
        );
    }

    public Point3D<TNumber> Add(Point3D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point3D<TNumber>(X + other.X, Y + other.Y, Z + other.Z);
    }

    public Point3D<TNumber> Add(Point2D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point3D<TNumber>(X + other.X, Y + other.Y, Z);
    }

    public Point3D<TNumber> Subtract(Point3D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point3D<TNumber>(X - other.X, Y - other.Y, Z - other.Z);
    }

    public Point3D<TNumber> Subtract(Point2D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point3D<TNumber>(X - other.X, Y - other.Y, Z);
    }

    public Point3D<TNumber> Multiply(TNumber scalar) => new(X * scalar, Y * scalar, Z * scalar);

    public Point3D<TNumber> Multiply(Point3D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point3D<TNumber>(X * other.X, Y * other.Y, Z * other.Z);
    }

    public Point3D<TNumber> Divide(TNumber scalar) =>
        scalar == TNumber.Zero
            ? new Point3D<TNumber>(TNumber.Zero, TNumber.Zero, TNumber.Zero)
            : new Point3D<TNumber>(X / scalar, Y / scalar, Z / scalar);

    public Point3D<TNumber> Negate() => new(-X, -Y, -Z);

    public static Point3D<TNumber> operator +(Point3D<TNumber> x, Point3D<TNumber> y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Add(y);
    }

    public static Point3D<TNumber> operator +(Point3D<TNumber> x, Point2D<TNumber> y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Add(y);
    }

    public static Point3D<TNumber> operator -(Point3D<TNumber> x, Point3D<TNumber> y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Subtract(y);
    }

    public static Point3D<TNumber> operator -(Point3D<TNumber> x, Point2D<TNumber> y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Subtract(y);
    }

    public static Point3D<TNumber> operator *(Point3D<TNumber> point, TNumber scalar)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.Multiply(scalar);
    }

    public static Point3D<TNumber> operator *(Point3D<TNumber> point, Point3D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.Multiply(other);
    }

    public static Point3D<TNumber> operator /(Point3D<TNumber> point, TNumber scalar)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.Divide(scalar);
    }

    public static Point3D<TNumber> operator -(Point3D<TNumber> point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.Negate();
    }
}
