// -----------------------------------------------------------------------
// <copyright file="Region2D.cs" company="NewSage">
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
public struct Region2D : IEquatable<Region2D>
{
    public Coord2D Lo;
    public Coord2D Hi;

    public readonly int Width => Hi.X - Lo.X;

    public readonly int Height => Hi.Y - Lo.Y;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Region2D other && Equals(other);

    public readonly bool Equals(Region2D other) => Lo == other.Lo && Hi == other.Hi;

    public override readonly int GetHashCode() => HashCode.Combine(Lo, Hi);

    public static bool operator ==(Region2D left, Region2D right) => left.Equals(right);

    public static bool operator !=(Region2D left, Region2D right) => !left.Equals(right);
}
