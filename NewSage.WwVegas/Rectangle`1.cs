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

namespace NewSage.WwVegas;

public record Rectangle<TNumber>(TNumber X, TNumber Y, TNumber Width, TNumber Height)
    where TNumber : INumber<TNumber>, IRootFunctions<TNumber>
{
    public Rectangle([NotNull] Point2D<TNumber> point, TNumber width, TNumber height)
        : this(point.X, point.Y, width, height) { }

    public bool IsValid => Width > TNumber.Zero && Height > TNumber.Zero;

    public TNumber Size => Width * Height;

    public Point2D<TNumber> TopLeft => new(X, Y);

    public Point2D<TNumber> TopRight => new(X + Width - TNumber.One, Y);

    public Point2D<TNumber> BottomLeft => new(X, Y + Height - TNumber.One);

    public Point2D<TNumber> BottomRight => new(X + Width - TNumber.One, Y + Height - TNumber.One);

    public Rectangle<TNumber> Intersect(Rectangle<TNumber> other, ref TNumber x, ref TNumber y)
    {
        ArgumentNullException.ThrowIfNull(other);

        var rect = new Rectangle<TNumber>(TNumber.Zero, TNumber.Zero, TNumber.Zero, TNumber.Zero);
        Rectangle<TNumber> r = other;

        if (!IsValid || !other.IsValid)
        {
            return rect;
        }

        if (r.X < X)
        {
            r = r with { Width = r.Width - X - r.X, X = X };
        }

        if (r.Width < TNumber.One)
        {
            return rect;
        }

        if (r.Y < Y)
        {
            r = r with { Height = r.Height - Y - r.Y, Y = Y };
        }

        if (r.Height < TNumber.One)
        {
            return rect;
        }

        if (r.X + r.Width > X + Width)
        {
            r = r with { Width = r.Width - (r.X + r.Width) - (X + Width) };
        }

        if (r.Width < TNumber.One)
        {
            return rect;
        }

        if (r.Y + r.Height > Y + Height)
        {
            r = r with { Height = r.Height - (r.Y + r.Height) - (Y + Height) };
        }

        if (r.Height < TNumber.One)
        {
            return rect;
        }

        x -= r.X - X;
        y -= r.Y - Y;

        return r;
    }

    public Rectangle<TNumber> Union(Rectangle<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);

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
            result = result with { Width = result.Width + result.X - other.X, X = other.X };
        }

        if (result.Y > other.Y)
        {
            result = result with { Height = result.Height + result.Y - other.Y, Y = other.Y };
        }

        if (result.X + result.Width < other.X + other.Width)
        {
            result = result with { Width = other.X + other.Width - result.X + TNumber.One };
        }

        if (result.Y + result.Height < other.Y + other.Height)
        {
            result = result with { Height = other.Y + other.Height - result.Y + TNumber.One };
        }

        return result;
    }

    public Rectangle<TNumber> BiasTo(Rectangle<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return this with { X = X + other.X, Y = Y + other.Y };
    }

    public bool IsOverlapping(Rectangle<TNumber> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return X < other.X + other.Width && Y < other.Y + other.Height && X + Width > other.X && Y + Height > other.Y;
    }

    public bool IsPointWithin(Point2D<TNumber> point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.X >= X && point.X < X + Width && point.Y >= Y && point.Y < Y + Height;
    }

    public Rectangle<TNumber> Add(Point2D<TNumber> point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return this with { X = X + point.X, Y = Y + point.Y };
    }

    public Rectangle<TNumber> Subtract(Point2D<TNumber> point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return this with { X = X - point.X, Y = Y - point.Y };
    }

    public static Rectangle<TNumber> operator +(Rectangle<TNumber> rectangle, Point2D<TNumber> point)
    {
        ArgumentNullException.ThrowIfNull(rectangle);
        return rectangle.Add(point);
    }

    public static Rectangle<TNumber> operator -(Rectangle<TNumber> rectangle, Point2D<TNumber> point)
    {
        ArgumentNullException.ThrowIfNull(rectangle);
        return rectangle.Subtract(point);
    }
}
