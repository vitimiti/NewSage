// -----------------------------------------------------------------------
// <copyright file="Region3D.cs" company="NewSage">
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NewSage.BaseTypes;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public struct Region3D : IEquatable<Region3D>
{
    public Coord3D Lo;
    public Coord3D Hi;

    public readonly int Width => Hi.X - Lo.X;

    public readonly int Height => Hi.Y - Lo.Y;

    public readonly int Depth => Hi.Z - Lo.Z;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Region3D other && Equals(other);

    public readonly bool Equals(Region3D other) => Lo == other.Lo && Hi == other.Hi;

    public override readonly int GetHashCode() => HashCode.Combine(Lo, Hi);

    public override readonly string ToString() => $"({Lo}, {Hi})";

    public static bool operator ==(Region3D left, Region3D right) => left.Equals(right);

    public static bool operator !=(Region3D left, Region3D right) => !left.Equals(right);
}
