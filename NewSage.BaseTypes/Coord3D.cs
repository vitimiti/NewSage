// -----------------------------------------------------------------------
// <copyright file="Coord3D.cs" company="NewSage">
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
public struct Coord3D : IEquatable<Coord3D>
{
    public int X;
    public int Y;
    public int Z;

    public static Coord3D Zero => default;

    public readonly int Length => (int)float.Sqrt((X * X) + (Y * Y) + (Z * Z));

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Coord3D other && Equals(other);

    public readonly bool Equals(Coord3D other) => X == other.X && Y == other.Y && Z == other.Z;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    public override readonly string ToString() => $"({X}, {Y}, {Z})";

    public static bool operator ==(Coord3D left, Coord3D right) => left.Equals(right);

    public static bool operator !=(Coord3D left, Coord3D right) => !left.Equals(right);
}
