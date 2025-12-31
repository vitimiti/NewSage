// -----------------------------------------------------------------------
// <copyright file="CastResult.cs" company="NewSage">
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
public struct CastResult
{
    public bool StartBad;
    public float Fraction;
    public Vector3 Normal;
    public uint SurfaceType;
    public bool ComputeContactPoint;
    public Vector3 ContactPoint;

    public CastResult() => Reset();

    public void Reset()
    {
        StartBad = false;
        Fraction = 1F;
        Normal.Set(0, 0, 0);
        SurfaceType = 0;
        ComputeContactPoint = false;
        ContactPoint.Set(0, 0, 0);
    }
}
