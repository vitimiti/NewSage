// -----------------------------------------------------------------------
// <copyright file="FRegion3D.cs" company="NewSage">
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

namespace NewSage.BaseTypes;

[StructLayout(LayoutKind.Sequential)]
public struct FRegion3D : IEquatable<FRegion3D>
{
    public FCoord3D Lo;
    public FCoord3D Hi;

    public static FRegion3D Zero => default;

    public readonly float Width => Hi.X - Lo.X;

    public readonly float Height => Hi.Y - Lo.Y;

    public readonly float Depth => Hi.Z - Lo.Z;

    public readonly bool IsInRegionNoZ(FCoord3D query) =>
        Lo.X < query.X && query.X < Hi.X && Lo.Y < query.Y && query.Y < Hi.Y;

    public readonly bool IsInRegion(FCoord3D query) => IsInRegionNoZ(query) && Lo.Z < query.Z && query.Z < Hi.Z;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is FRegion3D other && Equals(other);

    public readonly bool Equals(FRegion3D other) => Lo == other.Lo && Hi == other.Hi;

    public override readonly int GetHashCode() => HashCode.Combine(Lo, Hi);

    public static bool operator ==(FRegion3D left, FRegion3D right) => left.Equals(right);

    public static bool operator !=(FRegion3D left, FRegion3D right) => !left.Equals(right);
}
