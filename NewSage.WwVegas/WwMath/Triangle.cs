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
    public Vector3 N;
    public Vector3 V0;
    public Vector3 V1;
    public Vector3 V2;

    public void ComputeNormal()
    {
        N = Vector3.CrossProduct(V1 - V0, V2 - V0);
        N.Normalize();
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
        var x = float.Abs(N.X);
        var y = float.Abs(N.Y);
        var z = float.Abs(N.Z);
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
