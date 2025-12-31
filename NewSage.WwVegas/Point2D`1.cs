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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace NewSage.WwVegas;

[StructLayout(LayoutKind.Sequential)]
public struct Point2D<TNumber> : IEquatable<Point2D<TNumber>>
    where TNumber : INumber<TNumber>
{
    public Point2D() => (X, Y) = (TNumber.Zero, TNumber.Zero);

    public Point2D(TNumber x, TNumber y) => (X, Y) = (x, y);

    public TNumber X;
    public TNumber Y;

    public readonly Point2D<TNumber> BiasTo(Rectangle<TNumber> rectangle) => new(X + rectangle.X, Y + rectangle.Y);

    public readonly Point2D<TNumber> DotProduct(Point2D<TNumber> other) => Multiply(other);

    public readonly Point2D<TNumber> CrossProduct(Point2D<TNumber> other) => new(Y - other.Y, X - other.X);

    public readonly Point2D<TNumber> Add(Point2D<TNumber> other) => new(X + other.X, Y + other.Y);

    public readonly Point2D<TNumber> Subtract(Point2D<TNumber> other) => new(X - other.X, Y - other.Y);

    public readonly Point2D<TNumber> Multiply(TNumber scalar) => new(X * scalar, Y * scalar);

    public readonly Point2D<TNumber> Multiply(Point2D<TNumber> other) => new(X * other.X, Y * other.Y);

    public readonly Point2D<TNumber> Divide(TNumber scalar) =>
        scalar == TNumber.Zero
            ? new Point2D<TNumber>(TNumber.Zero, TNumber.Zero)
            : new Point2D<TNumber>(X / scalar, Y / scalar);

    public readonly Point2D<TNumber> Negate() => new(-X, -Y);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Point2D<TNumber> other && Equals(other);

    public readonly bool Equals(Point2D<TNumber> other) => X == other.X && Y == other.Y;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    public static Point2D<TNumber> operator +(Point2D<TNumber> x, Point2D<TNumber> y) => x.Add(y);

    public static Point2D<TNumber> operator -(Point2D<TNumber> x, Point2D<TNumber> y) => x.Subtract(y);

    public static Point2D<TNumber> operator *(Point2D<TNumber> point, TNumber scalar) => point.Multiply(scalar);

    public static Point2D<TNumber> operator *(Point2D<TNumber> point, Point2D<TNumber> other) => point.Multiply(other);

    public static Point2D<TNumber> operator /(Point2D<TNumber> point, TNumber scalar) => point.Divide(scalar);

    public static Point2D<TNumber> operator -(Point2D<TNumber> point) => point.Negate();

    public static bool operator ==(Point2D<TNumber> x, Point2D<TNumber> y) => x.Equals(y);

    public static bool operator !=(Point2D<TNumber> x, Point2D<TNumber> y) => !x.Equals(y);
}
