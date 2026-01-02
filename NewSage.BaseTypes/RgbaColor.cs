// -----------------------------------------------------------------------
// <copyright file="RgbaColor.cs" company="NewSage">
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
public struct RgbaColor : IEquatable<RgbaColor>
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is RgbaColor other && Equals(other);

    public readonly bool Equals(RgbaColor other) => R == other.R && G == other.G && B == other.B && A == other.A;

    public override readonly int GetHashCode() => HashCode.Combine(R, G, B, A);

    public override readonly string ToString() => $"({R}, {G}, {B}, {A})";

    public static bool operator ==(RgbaColor left, RgbaColor right) => left.Equals(right);

    public static bool operator !=(RgbaColor left, RgbaColor right) => !left.Equals(right);
}
