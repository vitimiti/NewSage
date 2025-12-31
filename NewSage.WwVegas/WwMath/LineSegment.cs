// -----------------------------------------------------------------------
// <copyright file="LineSegment.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwMath;

public class LineSegment
{
    public LineSegment() { }

    public LineSegment(Vector3 p0, Vector3 p1) => Set(p0, p1);

    public LineSegment(LineSegment other, Matrix3D matrix) => Set(other, matrix);

    public Vector3 P0 { get; protected set; }

    public Vector3 P1 { get; protected set; }

    public Vector3 PointDifference { get; protected set; }

    public Vector3 Direction { get; protected set; }

    public float Length { get; protected set; }

    public void Set(Vector3 p0, Vector3 p1)
    {
        P0 = p0;
        P1 = p1;
        Recalculate();
    }

    public void Set(LineSegment other, Matrix3D matrix)
    {
        ArgumentNullException.ThrowIfNull(other);

        P0 = Matrix3D.TransformVector(matrix, other.P0);
        P1 = Matrix3D.TransformVector(matrix, other.P1);

        PointDifference = P1 - P0;

        Direction = Matrix3D.RotateVector(matrix, other.Direction);

        Length = other.Length;
    }

    public void SetRandom(Vector3 min, Vector3 max)
    {
        var frac = VegasMath.RandomFloat();
        var p0X = min.X + (frac * (max.X - min.X));
        frac = VegasMath.RandomFloat();
        var p0Y = min.Y + (frac * (max.Y - min.Y));
        frac = VegasMath.RandomFloat();
        var p0Z = min.Z + (frac * (max.Z - min.Z));
        P0.Set(p0X, p0Y, p0Z);

        frac = VegasMath.RandomFloat();
        var p1X = min.X + (frac * (max.X - min.X));
        frac = VegasMath.RandomFloat();
        var p1Y = min.Y + (frac * (max.Y - min.Y));
        frac = VegasMath.RandomFloat();
        var p1Z = min.Z + (frac * (max.Z - min.Z));
        P1.Set(p1X, p1Y, p1Z);

        PointDifference = P1 - P0;
        Direction = PointDifference;
        Direction.Normalize();
        Length = PointDifference.Length;
    }

    public Vector3 ComputePoint(float t) => P0 + (t * PointDifference);

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Worse readability.")]
    public Vector3 FindPointClosestTo(Vector3 position)
    {
        Vector3 v0Position = position - P0;
        var dotProduct = Vector3.DotProduct(Direction, v0Position);

        if (dotProduct <= 0F)
        {
            return P0;
        }

        if (dotProduct >= Length)
        {
            return P1;
        }

        return P0 + (dotProduct * Direction);
    }

    public bool FindIntersection(
        LineSegment other,
        out (Vector3 P1, float Fraction1, Vector3 P2, float Fraction2) intersection
    )
    {
        ArgumentNullException.ThrowIfNull(other);

        var returnValue = false;
        Vector3 p1Result = default;
        Vector3 p2Result = default;
        var f1Result = 0F;
        var f2Result = 0F;

        var cross1 = Vector3.DotProduct(Direction, other.Direction);
        var cross2 = Vector3.DotProduct(other.P0 - P0, other.Direction);
        var top1 = cross2 * cross1;
        var bottom1 = cross1 * cross1;

        var cross3 = Vector3.DotProduct(other.Direction, Direction);
        var cross4 = Vector3.DotProduct(P0 - other.P0, Direction);
        var top2 = cross4 * cross3;
        var bottom2 = cross3 * cross3;

        if (float.Abs(bottom1) > float.Epsilon && float.Abs(bottom2) > 0)
        {
            var length1 = top1 / bottom1;
            var length2 = top2 / bottom2;

            p1Result = P0 + (Direction * length1);
            p2Result = other.P0 + (other.Direction * length2);

            f1Result = length1;
            f2Result = length2;

            returnValue = true;
        }

        intersection = (p1Result, f1Result, p2Result, f2Result);
        return returnValue;
    }

    protected void Recalculate()
    {
        PointDifference = P1 - P0;
        Direction = PointDifference;
        Direction.Normalize();
        Length = PointDifference.Length;
    }
}
