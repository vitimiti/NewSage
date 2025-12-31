// -----------------------------------------------------------------------
// <copyright file="OrientedBoundingBox.cs" company="NewSage">
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
public struct OrientedBoundingBox : IEquatable<OrientedBoundingBox>
{
    public Matrix3X3 Basis;
    public Vector3 Center;
    public Vector3 Extent;

    public OrientedBoundingBox(OrientedBoundingBox other) =>
        (Basis, Center, Extent) = (other.Basis, other.Center, other.Extent);

    public OrientedBoundingBox(Vector3 center, Vector3 extent) =>
        (Basis, Center, Extent) = (new Matrix3X3(identity: true), center, extent);

    public OrientedBoundingBox(Vector3 center, Vector3 extent, Matrix3X3 basis) =>
        (Basis, Center, Extent) = (basis, center, extent);

    public OrientedBoundingBox(ReadOnlySpan<Vector3> points) => InitializeFromBoxPoints(points);

    public readonly float Volume => 2F * Extent.X * 2F * Extent.Y * 2F * Extent.Z;

    public static OrientedBoundingBox Transform(Matrix3D matrix, OrientedBoundingBox box)
    {
        OrientedBoundingBox result = default;
        result.Extent = box.Extent;
        result.Center = Matrix3D.TransformVector(matrix, box.Center);
        result.Basis = matrix * box.Basis;
        return result;
    }

    public static bool BoxesIntersect(OrientedBoundingBox x, OrientedBoundingBox y)
    {
        Span<Vector3> a = stackalloc Vector3[3];
        Span<Vector3> b = stackalloc Vector3[3];

        a[0].Set(x.Basis[0][0], x.Basis[1][0], x.Basis[2][0]);
        a[1].Set(x.Basis[0][1], x.Basis[1][1], x.Basis[2][1]);
        a[2].Set(x.Basis[0][2], x.Basis[1][2], x.Basis[2][2]);

        b[0].Set(y.Basis[0][0], y.Basis[1][0], y.Basis[2][0]);
        b[1].Set(y.Basis[0][1], y.Basis[1][1], y.Basis[2][1]);
        b[2].Set(y.Basis[0][2], y.Basis[1][2], y.Basis[2][2]);

        if (BoxesIntersectOnAxis(x, y, a[0]))
        {
            return false;
        }

        if (BoxesIntersectOnAxis(x, y, a[1]))
        {
            return false;
        }

        if (BoxesIntersectOnAxis(x, y, a[2]))
        {
            return false;
        }

        if (BoxesIntersectOnAxis(x, y, b[0]))
        {
            return false;
        }

        if (BoxesIntersectOnAxis(x, y, b[1]))
        {
            return false;
        }

        if (BoxesIntersectOnAxis(x, y, b[2]))
        {
            return false;
        }

        var axis = Vector3.CrossProduct(a[0], b[0]);
        if (!BoxesIntersectOnAxis(x, y, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[0], b[1]);
        if (!BoxesIntersectOnAxis(x, y, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[0], b[2]);
        if (!BoxesIntersectOnAxis(x, y, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[1], b[0]);
        if (!BoxesIntersectOnAxis(x, y, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[1], b[1]);
        if (!BoxesIntersectOnAxis(x, y, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[1], b[2]);
        if (!BoxesIntersectOnAxis(x, y, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[2], b[0]);
        if (!BoxesIntersectOnAxis(x, y, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[2], b[1]);
        if (!BoxesIntersectOnAxis(x, y, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[2], b[2]);
        return BoxesIntersectOnAxis(x, y, axis);
    }

    public static bool BoxesCollide(OrientedBoundingBox x, Vector3 v0, OrientedBoundingBox y, Vector3 v1, float dt)
    {
        var a0 = new Vector3(x.Basis[0][0], x.Basis[1][0], x.Basis[2][0]);
        var a1 = new Vector3(x.Basis[0][1], x.Basis[1][1], x.Basis[2][1]);
        var a2 = new Vector3(x.Basis[0][2], x.Basis[1][2], x.Basis[2][2]);

        var b0 = new Vector3(y.Basis[0][0], y.Basis[1][0], y.Basis[2][0]);
        var b1 = new Vector3(y.Basis[0][1], y.Basis[1][1], y.Basis[2][1]);
        var b2 = new Vector3(y.Basis[0][2], y.Basis[1][2], y.Basis[2][2]);

        Vector3 sepAxis = a0;
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = a1;
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = a2;
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = b0;
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = b1;
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = b2;
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = Vector3.CrossProduct(a0, b0);
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = Vector3.CrossProduct(a0, b1);
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = Vector3.CrossProduct(a0, b2);
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = Vector3.CrossProduct(a1, b0);
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = Vector3.CrossProduct(a1, b1);
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = Vector3.CrossProduct(a1, b2);
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = Vector3.CrossProduct(a2, b0);
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = Vector3.CrossProduct(a2, b1);
        if (!BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt))
        {
            return false;
        }

        sepAxis = Vector3.CrossProduct(a2, b2);
        return BoxesCollideOnAxis(x, v0, y, v1, sepAxis, dt);
    }

    public static bool BoxIntersectsTriangle(OrientedBoundingBox box, Triangle triangle)
    {
        Span<Vector3> a = stackalloc Vector3[3];
        Span<Vector3> e = stackalloc Vector3[3];
        Vector3 normal = triangle.Normal;
        a[0].Set(box.Basis[0][0], box.Basis[1][0], box.Basis[2][0]);
        a[1].Set(box.Basis[0][1], box.Basis[1][1], box.Basis[2][1]);
        a[2].Set(box.Basis[0][2], box.Basis[1][2], box.Basis[2][2]);
        e[0] = triangle.V1 - triangle.V0;
        e[1] = triangle.V2 - triangle.V1;
        e[2] = triangle.V0 - triangle.V2;

        if (!BoxIntersectsTriangleOnAxis(box, triangle, normal))
        {
            return false;
        }

        if (!BoxIntersectsTriangleOnAxis(box, triangle, a[0]))
        {
            return false;
        }

        if (!BoxIntersectsTriangleOnAxis(box, triangle, a[1]))
        {
            return false;
        }

        if (!BoxIntersectsTriangleOnAxis(box, triangle, a[2]))
        {
            return false;
        }

        var axis = Vector3.CrossProduct(a[0], e[0]);
        if (!BoxIntersectsTriangleOnAxis(box, triangle, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[0], e[1]);
        if (!BoxIntersectsTriangleOnAxis(box, triangle, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[0], e[2]);
        if (!BoxIntersectsTriangleOnAxis(box, triangle, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[1], e[0]);
        if (!BoxIntersectsTriangleOnAxis(box, triangle, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[1], e[1]);
        if (!BoxIntersectsTriangleOnAxis(box, triangle, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[1], e[2]);
        if (!BoxIntersectsTriangleOnAxis(box, triangle, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[2], e[0]);
        if (!BoxIntersectsTriangleOnAxis(box, triangle, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[2], e[1]);
        if (!BoxIntersectsTriangleOnAxis(box, triangle, axis))
        {
            return false;
        }

        axis = Vector3.CrossProduct(a[2], e[2]);
        return BoxIntersectsTriangleOnAxis(box, triangle, axis);
    }

    public void InitializeFromBoxPoints(ReadOnlySpan<Vector3> points)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(points.Length, 8);

        Span<Vector3> dp = stackalloc Vector3[8];
        for (var i = 1; i < points.Length; i++)
        {
            dp[i] = points[i] - points[0];
        }

        for (var i = 1; i < points.Length; i++)
        {
            for (var j = 1; j < points.Length; j++)
            {
                if (dp[j].Length2 < dp[i].Length2)
                {
                    (dp[j], dp[i]) = (dp[i], dp[j]);
                }
            }
        }

        var axis0 = Vector3.Normalize(dp[1]);
        var axis1 = Vector3.Normalize(dp[2]);
        var axis2 = Vector3.CrossProduct(axis0, axis1);

        Basis = new Matrix3X3(axis0, axis1, axis2);
        Center.Set(0, 0, 0);
        foreach (Vector3 point in points)
        {
            Center += point;
        }

        Center /= points.Length;

        Extent.Set(0, 0, 0);
        for (var i = 0; i < points.Length; i++)
        {
            var dx = points[i].X - Center.X;
            var dy = points[i].Y - Center.Y;
            var dz = points[i].Z - Center.Z;

            var xpRj = float.Abs((axis0.X * dx) + (axis0.Y * dy) + (axis0.Z * dz));
            if (xpRj > Extent.X)
            {
                Extent.X = xpRj;
            }

            var ypRj = float.Abs((axis1.X * dx) + (axis1.Y * dy) + (axis1.Z * dz));
            if (ypRj > Extent.Y)
            {
                Extent.Y = ypRj;
            }

            var zpRj = float.Abs((axis2.X * dx) + (axis2.Y * dy) + (axis2.Z * dz));
            if (zpRj > Extent.Z)
            {
                Extent.Z = zpRj;
            }
        }
    }

    public void InitializeRandom(float minExtent = .5F, float maxExtent = 1F)
    {
        Center.Set(0, 0, 0);
        Extent.Set(
            minExtent + (VegasMath.RandomFloat() * (maxExtent - minExtent)),
            minExtent + (VegasMath.RandomFloat() * (maxExtent - minExtent)),
            minExtent + (VegasMath.RandomFloat() * (maxExtent - minExtent))
        );

        var orient = new Quaternion
        {
            X = VegasMath.RandomFloat(),
            Y = VegasMath.RandomFloat(),
            Z = VegasMath.RandomFloat(),
            W = VegasMath.RandomFloat(),
        };

        orient.Normalize();
    }

    public readonly float ProjectToAxis(Vector3 axis)
    {
        var x = Extent[0] * Vector3.DotProduct(axis, new Vector3(Basis[0][0], Basis[1][0], Basis[2][0]));
        var y = Extent[1] * Vector3.DotProduct(axis, new Vector3(Basis[0][1], Basis[1][1], Basis[2][1]));
        var z = Extent[2] * Vector3.DotProduct(axis, new Vector3(Basis[0][2], Basis[1][2], Basis[2][2]));

        return float.Abs(x) + float.Abs(y) + float.Abs(z);
    }

    public readonly Vector3 ComputePoint(ReadOnlySpan<float> values)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(values.Length, 3);

        Vector3 point = Extent;
        point.X *= values[0];
        point.Y *= values[1];
        point.Z *= values[2];

        return Center + Matrix3X3.RotateVector(Basis, point);
    }

    public readonly Vector3 ComputeAxisAlignedExtent()
    {
        var x =
            float.Abs(Extent[0] * Basis[0][0])
            + float.Abs(Extent[1] * Basis[0][1])
            + float.Abs(Extent[2] * Basis[0][2]);

        var y =
            float.Abs(Extent[0] * Basis[1][0])
            + float.Abs(Extent[1] * Basis[1][1])
            + float.Abs(Extent[2] * Basis[1][2]);

        var z =
            float.Abs(Extent[0] * Basis[2][0])
            + float.Abs(Extent[1] * Basis[2][1])
            + float.Abs(Extent[2] * Basis[2][2]);

        return new Vector3(x, y, z);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) =>
        obj is OrientedBoundingBox other && Equals(other);

    public readonly bool Equals(OrientedBoundingBox other) =>
        Basis == other.Basis && Center == other.Center && Extent == other.Extent;

    public override readonly int GetHashCode() => HashCode.Combine(Basis, Center, Extent);

    private static bool BoxesIntersectOnAxis(OrientedBoundingBox x, OrientedBoundingBox y, Vector3 axis)
    {
        if (axis.Length2 < float.Epsilon)
        {
            return true;
        }

        var ra = x.ProjectToAxis(axis);
        var rb = y.ProjectToAxis(axis);
        var rSum = float.Abs(ra) + float.Abs(rb);

        Vector3 c = y.Center - x.Center;
        var cDist = Vector3.DotProduct(axis, c);

        return cDist <= rSum && cDist >= -rSum;
    }

    private static bool BoxesCollideOnAxis(
        OrientedBoundingBox x,
        Vector3 v0,
        OrientedBoundingBox y,
        Vector3 v1,
        Vector3 axis,
        float dt
    )
    {
        if (axis.Length2 < float.Epsilon)
        {
            return true;
        }

        var ra = x.ProjectToAxis(axis);
        var rb = y.ProjectToAxis(axis);
        var rSum = float.Abs(ra) + float.Abs(rb);

        Vector3 c = y.Center - x.Center;
        Vector3 v = v1 - v0;

        var cDist = Vector3.DotProduct(axis, c);
        var vDist = cDist + (dt * Vector3.DotProduct(axis, v));

        return (cDist <= rSum || vDist <= rSum) && (cDist >= -rSum || vDist >= -rSum);
    }

    private static bool BoxIntersectsTriangleOnAxis(OrientedBoundingBox box, Triangle triangle, Vector3 axis)
    {
        if (axis.Length2 < float.Epsilon)
        {
            return true;
        }

        Vector3 d = triangle.V0 - box.Center;
        Vector3 r1 = triangle.V1 - triangle.V0;
        Vector3 r2 = triangle.V2 - triangle.V0;

        Vector3 localAxis = axis;

        var dist = Vector3.DotProduct(d, axis);
        if (dist < 0F)
        {
            dist = -dist;
            localAxis = -axis;
        }

        var leb = box.ProjectToAxis(localAxis);

        var lep = 0F;
        var temp = Vector3.DotProduct(r1, axis);
        if (temp < lep)
        {
            lep = temp;
        }

        temp = Vector3.DotProduct(r2, axis);
        if (temp < lep)
        {
            lep = temp;
        }

        lep += dist;

        return lep < leb;
    }

    public static bool operator ==(OrientedBoundingBox left, OrientedBoundingBox right) => left.Equals(right);

    public static bool operator !=(OrientedBoundingBox left, OrientedBoundingBox right) => !left.Equals(right);
}
