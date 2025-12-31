// -----------------------------------------------------------------------
// <copyright file="Triangle.cs" company="NewSage">
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
public struct Triangle
{
    public Vector3 Normal;
    public Vector3 V0;
    public Vector3 V1;
    public Vector3 V2;

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "This is a complex algorithm and splitting it may make it more difficult to read."
    )]
    public static bool PointInTriangle2D(
        (Vector3 Point0, Vector3 Point1, Vector3 Point2) triangle,
        Vector3 testPoint,
        (int Axis1, int Axis2) dominantPlane,
        ref TriangleCastOptions flags
    )
    {
        var p0P1 = new Vector2(
            triangle.Point1[dominantPlane.Axis1] - triangle.Point0[dominantPlane.Axis1],
            triangle.Point1[dominantPlane.Axis2] - triangle.Point0[dominantPlane.Axis2]
        );

        var p1P2 = new Vector2(
            triangle.Point2[dominantPlane.Axis1] - triangle.Point1[dominantPlane.Axis1],
            triangle.Point2[dominantPlane.Axis2] - triangle.Point1[dominantPlane.Axis2]
        );

        var p2P0 = new Vector2(
            triangle.Point0[dominantPlane.Axis1] - triangle.Point2[dominantPlane.Axis1],
            triangle.Point0[dominantPlane.Axis2] - triangle.Point2[dominantPlane.Axis2]
        );

        var p0P2 = new Vector2(
            triangle.Point2[dominantPlane.Axis1] - triangle.Point0[dominantPlane.Axis1],
            triangle.Point2[dominantPlane.Axis2] - triangle.Point0[dominantPlane.Axis2]
        );

        var p0P1P2 = Vector2.PerpendicularDotProduct(p0P1, p0P2);
        if (float.Abs(p0P1P2) > float.Epsilon)
        {
            var sideFactor = p0P1P2 > 0F ? 1F : -1F;
            Span<float> factors = stackalloc float[3];

            var p0Pt = new Vector2(
                testPoint[dominantPlane.Axis1] - triangle.Point0[dominantPlane.Axis1],
                testPoint[dominantPlane.Axis2] - triangle.Point0[dominantPlane.Axis2]
            );
            factors[0] = Vector2.PerpendicularDotProduct(p0P1, p0Pt);
            if (factors[0] * sideFactor < 0F)
            {
                return false;
            }

            var p1Pt = new Vector2(
                testPoint[dominantPlane.Axis1] - triangle.Point1[dominantPlane.Axis1],
                testPoint[dominantPlane.Axis2] - triangle.Point1[dominantPlane.Axis2]
            );
            factors[1] = Vector2.PerpendicularDotProduct(p1P2, p1Pt);
            if (factors[1] * sideFactor < 0F)
            {
                return false;
            }

            var p2Pt = new Vector2(
                testPoint[dominantPlane.Axis1] - triangle.Point2[dominantPlane.Axis1],
                testPoint[dominantPlane.Axis2] - triangle.Point2[dominantPlane.Axis2]
            );
            factors[2] = Vector2.PerpendicularDotProduct(p2P0, p2Pt);
            if (factors[2] * sideFactor < 0F)
            {
                return false;
            }

            if (
                float.Abs(factors[0]) < float.Epsilon
                || float.Abs(factors[1]) < float.Epsilon
                || float.Abs(factors[2]) < float.Epsilon
            )
            {
                flags |= TriangleCastOptions.HitEdge;
            }

            return true;
        }

        var p0P1Dist2 = p0P1.Length2;
        var p1P2Dist2 = p1P2.Length2;
        var p2P0Dist2 = p2P0.Length2;
        float maxDist2;
        Vector2 pSpE;
        Vector2 pSpT = default;
        if (p0P1Dist2 > p1P2Dist2)
        {
            if (p0P1Dist2 > p2P0Dist2)
            {
                pSpE = p0P1;
                pSpT.Set(
                    testPoint[dominantPlane.Axis1] - triangle.Point0[dominantPlane.Axis1],
                    testPoint[dominantPlane.Axis2] - triangle.Point0[dominantPlane.Axis2]
                );
                maxDist2 = p0P1Dist2;
            }
            else
            {
                pSpE = p2P0;
                pSpT.Set(
                    testPoint[dominantPlane.Axis1] - triangle.Point2[dominantPlane.Axis1],
                    testPoint[dominantPlane.Axis2] - triangle.Point2[dominantPlane.Axis2]
                );
                maxDist2 = p2P0Dist2;
            }
        }
        else
        {
            if (p1P2Dist2 > p2P0Dist2)
            {
                pSpE = p1P2;
                pSpT.Set(
                    testPoint[dominantPlane.Axis1] - triangle.Point1[dominantPlane.Axis1],
                    testPoint[dominantPlane.Axis2] - triangle.Point1[dominantPlane.Axis2]
                );
                maxDist2 = p1P2Dist2;
            }
            else
            {
                pSpE = p2P0;
                pSpT.Set(
                    testPoint[dominantPlane.Axis1] - triangle.Point2[dominantPlane.Axis1],
                    testPoint[dominantPlane.Axis2] - triangle.Point2[dominantPlane.Axis2]
                );
                maxDist2 = p2P0Dist2;
            }
        }

        if (float.Abs(maxDist2) > float.Epsilon)
        {
            if (float.Abs(Vector2.PerpendicularDotProduct(pSpE, pSpT)) > float.Epsilon)
            {
                return false;
            }

            Vector2 pEpT = pSpT - pSpE;
            if (pSpT.Length2 > maxDist2 || pEpT.Length2 > maxDist2)
            {
                return false;
            }

            flags |= TriangleCastOptions.HitEdge;
            return true;
        }

        if (float.Abs(pSpT.Length2) > float.Epsilon)
        {
            return false;
        }

        flags |= TriangleCastOptions.HitEdge;
        return true;
    }

    public static bool CastSemiInfiniteAxisAlignedRayToTriangle(
        (Vector3 Point0, Vector3 Point1, Vector3 Point2, Vector4 Plane) triangle,
        Vector3 rayStart,
        (int AxisR, int Axis1, int Axis2, int Direction) ray,
        ref TriangleCastOptions flags
    )
    {
        var returnValue = false;
        TriangleCastOptions flags2D = TriangleCastOptions.None;
        if (
            !PointInTriangle2D(
                (triangle.Point0, triangle.Point1, triangle.Point2),
                rayStart,
                (ray.Axis1, ray.Axis2),
                ref flags2D
            )
        )
        {
            return returnValue;
        }

        Span<float> signs = [-1F, 1F];
        var result =
            triangle.Plane[ray.AxisR]
            * signs[ray.Direction]
            * (
                (triangle.Plane.X * rayStart.X)
                + (triangle.Plane.Y * rayStart.Y)
                + (triangle.Plane.Z * rayStart.Z)
                + triangle.Plane.W
            );

        if (result < 0F)
        {
            flags |= flags2D & TriangleCastOptions.HitEdge;
            returnValue = true;
        }
        else
        {
            if (float.Abs(result) > float.Epsilon)
            {
                return returnValue;
            }

            if (float.Abs(triangle.Plane[ray.AxisR]) > float.Epsilon)
            {
                flags |= flags2D & TriangleCastOptions.HitEdge;
                flags |= TriangleCastOptions.StartInTriangle;
                returnValue = true;
            }
            else
            {
                var tri = new Triangle
                {
                    V0 = triangle.Point0,
                    V1 = triangle.Point1,
                    V2 = triangle.Point2,
                    Normal = new Vector3(triangle.Plane.X, triangle.Plane.Y, triangle.Plane.Z),
                };

                if (tri.ContainsPoint(rayStart))
                {
                    flags |= TriangleCastOptions.StartInTriangle;
                }
            }
        }

        return returnValue;
    }

    public void ComputeNormal()
    {
        Normal = Vector3.CrossProduct(V1 - V0, V2 - V0);
        Normal.Normalize();
    }

    public readonly bool ContainsPoint(Vector3 point)
    {
        Vector2 edge = default;
        Vector2 dp = default;
        Span<bool> sides = stackalloc bool[3];

        (var axis1, var axis2) = FindDominantPlane();
        for (var vi = 0; vi < 3; vi++)
        {
            var va = vi;
            var vb = (vi + 1) % 3;

            edge.Set(GetVIndex(vb)[axis1] - GetVIndex(va)[axis1], GetVIndex(vb)[axis2] - GetVIndex(va)[axis2]);

            dp.Set(point[axis1] - GetVIndex(va)[axis1], point[axis2] - GetVIndex(va)[axis2]);

            var cross = (edge.X * dp.Y) - (edge.Y * dp.X);
            sides[vi] = cross >= 0F;
        }

        var myIntersect = sides[0] == sides[1] && sides[1] == sides[2];
        return myIntersect;
    }

    public readonly Vector3 GetVIndex(int index) =>
        index switch
        {
            0 => V0,
            1 => V1,
            2 => V2,
            _ => throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                "Index must be between 0 and 2 inclusive."
            ),
        };

    public readonly (int Axis1, int Axis2) FindDominantPlane()
    {
        var x = float.Abs(Normal.X);
        var y = float.Abs(Normal.Y);
        var z = float.Abs(Normal.Z);
        var value = x;
        var ni = 0;

        if (y > value)
        {
            ni = 1;
            value = y;
        }

        if (z > value)
        {
            ni = 2;
        }

        switch (ni)
        {
            case 0:
                return (1, 2);
            case 1:
                return (0, 2);
            default:
                break;
        }

        return (0, 1);
    }
}
