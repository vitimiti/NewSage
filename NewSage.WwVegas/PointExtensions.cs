// -----------------------------------------------------------------------
// <copyright file="PointExtensions.cs" company="NewSage">
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

public static class PointExtensions
{
    public static TNumber GetLength<TNumber>(this Point2D<TNumber> point)
        where TNumber : INumber<TNumber>, IRootFunctions<TNumber>
    {
        ArgumentNullException.ThrowIfNull(point);
        return TNumber.Sqrt((point.X * point.X) + (point.Y * point.Y));
    }

    public static TNumber GetLength<TNumber>(this Point3D<TNumber> point)
        where TNumber : INumber<TNumber>, IRootFunctions<TNumber>
    {
        ArgumentNullException.ThrowIfNull(point);
        return TNumber.Sqrt((point.X * point.X) + (point.Y * point.Y) + (point.Z * point.Z));
    }

    public static Point2D<TNumber> GetNormalized<TNumber>(this Point2D<TNumber> point)
        where TNumber : INumber<TNumber>, IRootFunctions<TNumber>
    {
        ArgumentNullException.ThrowIfNull(point);

        TNumber length = point.GetLength();
        return length != TNumber.Zero ? new Point2D<TNumber>(point.X / length, point.Y / length) : point;
    }

    public static Point3D<TNumber> GetNormalized<TNumber>(this Point3D<TNumber> point)
        where TNumber : INumber<TNumber>, IRootFunctions<TNumber>
    {
        ArgumentNullException.ThrowIfNull(point);

        TNumber length = point.GetLength();
        return length != TNumber.Zero
            ? new Point3D<TNumber>(point.X / length, point.Y / length, point.Z / length)
            : point;
    }

    public static TNumber DistanceTo<TNumber>(this Point2D<TNumber> point, Point2D<TNumber> other)
        where TNumber : INumber<TNumber>, IRootFunctions<TNumber> => (point - other).GetLength();

    public static TNumber DistanceTo<TNumber>(this Point3D<TNumber> point, Point3D<TNumber> other)
        where TNumber : INumber<TNumber>, IRootFunctions<TNumber> => (point - other).GetLength();

    public static TNumber DistanceTo<TNumber>(this Point3D<TNumber> point, Point2D<TNumber> other)
        where TNumber : INumber<TNumber>, IRootFunctions<TNumber> => (point - other).GetLength();
}
