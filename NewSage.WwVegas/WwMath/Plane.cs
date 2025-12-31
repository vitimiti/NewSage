// -----------------------------------------------------------------------
// <copyright file="Plane.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwMath;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage(
    "Performance",
    "CA1815:Override equals and operator equals on value types",
    Justification = "Not used in this type."
)]
public struct Plane
{
    public Vector3 Normal;
    public float Distance;

    public Plane(float nx, float ny, float nz, float distance) => Set(nx, ny, nz, distance);

    public Plane(Vector3 normal, float distance) => Set(normal, distance);

    public Plane(Vector3 normal, Vector3 point) => Set(normal, point);

    public Plane(Vector3 point1, Vector3 point2, Vector3 point3) => Set(point1, point2, point3);

    public static (Vector3 LineDirection, Vector3 LinePoint) IntersectPlanes(Plane x, Plane y)
    {
        var lineDirection = Vector3.CrossProduct(x.Normal, y.Normal);
        Vector3 linePoint = default;
        Vector3 absDir = lineDirection;
        absDir.UpdateMax(-absDir);
        if (absDir.X > absDir.Y)
        {
            if (absDir.X > absDir.Z)
            {
                var oneOverLine = 1F / lineDirection.X;
                linePoint.Set(
                    0F,
                    ((y.Normal.Z * x.Distance) - (x.Normal.Z * y.Distance)) * oneOverLine,
                    ((x.Normal.Y * y.Distance) - (y.Normal.Y * x.Distance)) * oneOverLine
                );
            }
            else
            {
                var oneOverLine = 1F / lineDirection.Z;
                linePoint.Set(
                    ((y.Normal.Y * x.Distance) - (x.Normal.Y * y.Distance)) * oneOverLine,
                    ((x.Normal.X * y.Distance) - (y.Normal.X * x.Distance)) * oneOverLine,
                    0F
                );
            }
        }
        else
        {
            if (absDir.Y > absDir.Z)
            {
                var oneOverLine = 1F / lineDirection.Y;
                linePoint.Set(
                    ((x.Normal.Z * y.Distance) - (y.Normal.Z * x.Distance)) * oneOverLine,
                    0F,
                    ((y.Normal.X * x.Distance) - (x.Normal.X * y.Distance)) * oneOverLine
                );
            }
            else
            {
                var oneOverLine = 1F / lineDirection.Z;
                linePoint.Set(
                    ((y.Normal.Y * x.Distance) - (x.Normal.Y * y.Distance)) * oneOverLine,
                    ((x.Normal.X * y.Distance) - (y.Normal.X * x.Distance)) * oneOverLine,
                    0F
                );
            }
        }

        lineDirection.Normalize();
        return (lineDirection, linePoint);
    }

    public void Set(float a, float b, float c, float d) => (Normal, Distance) = (new Vector3(a, b, c), d);

    public void Set(Vector3 normal, float distance) => (Normal, Distance) = (normal, distance);

    public void Set(Vector3 normal, Vector3 point) => (Normal, Distance) = (normal, Vector3.DotProduct(normal, point));

    public void Set(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        Normal = Vector3.CrossProduct(point2 - point1, point3 - point1);
        if (Normal != default)
        {
            Normal.Normalize();
            Distance = Vector3.DotProduct(Normal, point1);
        }
        else
        {
            Normal = default;
            Distance = 0F;
        }
    }

    public readonly bool ComputeIntersection(Vector3 p0, Vector3 p1, out float t)
    {
        t = 0F;
        var den = Vector3.DotProduct(Normal, p1 - p0);
        if (float.Abs(den) < float.Epsilon)
        {
            return false;
        }

        var num = -(Vector3.DotProduct(Normal, p0) - Distance);
        t = num / den;

        return t is >= 0 and <= 1;
    }

    public readonly bool InFront(Vector3 point) => Vector3.DotProduct(point, Normal) > Distance;

    public readonly bool InFront(Sphere sphere) =>
        Vector3.DotProduct(sphere.Center, Normal) - Distance >= sphere.Radius;

    public readonly bool InFrontOrIntersecting(Sphere sphere) =>
        Distance - Vector3.DotProduct(sphere.Center, Normal) < sphere.Radius;
}
