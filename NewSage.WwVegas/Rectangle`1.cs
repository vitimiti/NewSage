// -----------------------------------------------------------------------
// <copyright file="Rectangle`1.cs" company="NewSage">
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
public struct Rectangle<TNumber> : IEquatable<Rectangle<TNumber>>
    where TNumber : INumber<TNumber>
{
    public Rectangle() => (X, Y, Width, Height) = (TNumber.Zero, TNumber.Zero, TNumber.Zero, TNumber.Zero);

    public Rectangle(TNumber x, TNumber y, TNumber width, TNumber height) =>
        (X, Y, Width, Height) = (x, y, width, height);

    public Rectangle(Point2D<TNumber> point, TNumber width, TNumber height) =>
        (X, Y, Width, Height) = (point.X, point.Y, width, height);

    public TNumber X;
    public TNumber Y;
    public TNumber Width;
    public TNumber Height;

    public readonly bool IsValid => Width > TNumber.Zero && Height > TNumber.Zero;

    public readonly TNumber Size => Width * Height;

    public readonly Point2D<TNumber> TopLeft => new(X, Y);

    public readonly Point2D<TNumber> TopRight => new(X + Width - TNumber.One, Y);

    public readonly Point2D<TNumber> BottomLeft => new(X, Y + Height - TNumber.One);

    public readonly Point2D<TNumber> BottomRight => new(X + Width - TNumber.One, Y + Height - TNumber.One);

    public readonly Rectangle<TNumber> Intersect(Rectangle<TNumber> other, ref TNumber x, ref TNumber y)
    {
        Rectangle<TNumber> rect = new(TNumber.Zero, TNumber.Zero, TNumber.Zero, TNumber.Zero);
        Rectangle<TNumber> r = other;

        if (!IsValid || !r.IsValid)
        {
            return rect;
        }

        if (r.X < X)
        {
            r.Width -= X - r.X;
            r.X = X;
        }

        if (r.Width < TNumber.One)
        {
            return rect;
        }

        if (r.Y < Y)
        {
            r.Height -= Y - r.Y;
            r.Y = y;
        }

        if (r.Height < TNumber.One)
        {
            return rect;
        }

        if (r.X + r.Width > X + Width)
        {
            r.Width -= r.X + r.Width - (X + Width);
        }

        if (r.Width < TNumber.One)
        {
            return rect;
        }

        if (r.Y + r.Height > Y + Height)
        {
            r.Height -= r.Y + r.Height - (Y + Height);
        }

        if (r.Height < TNumber.One)
        {
            return rect;
        }

        x -= r.X - X;
        y -= r.Y - Y;
        return r;
    }

    public readonly Rectangle<TNumber> Union(Rectangle<TNumber> other)
    {
        if (!IsValid)
        {
            return other;
        }

        if (!other.IsValid)
        {
            return this;
        }

        Rectangle<TNumber> result = this;
        if (result.X > other.X)
        {
            result.Width += result.X - other.X;
            result.X = other.X;
        }

        if (result.Y > other.Y)
        {
            result.Height += result.Y - other.Y;
            result.Y = other.Y;
        }

        if (result.X + result.Width < other.X + other.Width)
        {
            result.Width = other.X + other.Width - result.X + TNumber.One;
        }

        if (result.Y + result.Height < other.Y + other.Height)
        {
            result.Height = other.Y + other.Height - result.Y + TNumber.One;
        }

        return result;
    }

    public readonly Rectangle<TNumber> BiasTo(Rectangle<TNumber> other) => new(X + other.X, Y + other.Y, Width, Height);

    public readonly bool IsOverlapping(Rectangle<TNumber> other) =>
        X < other.X + other.Width && Y < other.Y + other.Height && X + Width > other.X && Y + Height > other.Y;

    public readonly bool IsPointWithin(Point2D<TNumber> point) =>
        point.X >= X && point.X < X + Width && point.Y >= Y && point.Y < Y + Height;

    public readonly Rectangle<TNumber> Add(Point2D<TNumber> point) => new(X + point.X, Y + point.Y, Width, Height);

    public readonly Rectangle<TNumber> Subtract(Point2D<TNumber> point) => new(X - point.X, Y - point.Y, Width, Height);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Rectangle<TNumber> other && Equals(other);

    public readonly bool Equals(Rectangle<TNumber> other) =>
        X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    public static Rectangle<TNumber> operator +(Rectangle<TNumber> rectangle, Point2D<TNumber> point) =>
        rectangle.Add(point);

    public static Rectangle<TNumber> operator -(Rectangle<TNumber> rectangle, Point2D<TNumber> point) =>
        rectangle.Subtract(point);

    public static bool operator ==(Rectangle<TNumber> x, Rectangle<TNumber> y) => x.Equals(y);

    public static bool operator !=(Rectangle<TNumber> x, Rectangle<TNumber> y) => !x.Equals(y);
}
