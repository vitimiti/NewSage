// -----------------------------------------------------------------------
// <copyright file="Point2D`1.cs" company="NewSage">
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

public record Point2D<TNumber>(TNumber X, TNumber Y)
    where TNumber : INumber<TNumber>
{
    public Point2D<TNumber> BiasTo(Rectangle<TNumber> rectangle)
    {
        ArgumentNullException.ThrowIfNull(rectangle);
        return new Point2D<TNumber>(X + rectangle.X, Y + rectangle.Y);
    }

    public Point2D<TNumber> DotProduct(Point2D<TNumber> other) => Multiply(other);

    public Point2D<TNumber> CrossProduct(Point2D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point2D<TNumber>(Y - other.Y, X - other.X);
    }

    public Point2D<TNumber> Add(Point2D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point2D<TNumber>(X + other.X, Y + other.Y);
    }

    public Point2D<TNumber> Subtract(Point2D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point2D<TNumber>(X - other.X, Y - other.Y);
    }

    public Point2D<TNumber> Multiply(TNumber scalar) => new(X * scalar, Y * scalar);

    public Point2D<TNumber> Multiply(Point2D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Point2D<TNumber>(X * other.X, Y * other.Y);
    }

    public Point2D<TNumber> Divide(TNumber scalar) =>
        scalar == TNumber.Zero
            ? new Point2D<TNumber>(TNumber.Zero, TNumber.Zero)
            : new Point2D<TNumber>(X / scalar, Y / scalar);

    public Point2D<TNumber> Negate() => new(-X, -Y);

    public static Point2D<TNumber> operator +(Point2D<TNumber> x, Point2D<TNumber> y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Add(y);
    }

    public static Point2D<TNumber> operator -(Point2D<TNumber> x, Point2D<TNumber> y)
    {
        ArgumentNullException.ThrowIfNull(x);
        return x.Subtract(y);
    }

    public static Point2D<TNumber> operator *(Point2D<TNumber> point, TNumber scalar)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.Multiply(scalar);
    }

    public static Point2D<TNumber> operator *(Point2D<TNumber> point, Point2D<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.Multiply(other);
    }

    public static Point2D<TNumber> operator /(Point2D<TNumber> point, TNumber scalar)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.Divide(scalar);
    }

    public static Point2D<TNumber> operator -(Point2D<TNumber> point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.Negate();
    }
}
