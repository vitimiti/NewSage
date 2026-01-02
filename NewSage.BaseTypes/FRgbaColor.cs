// -----------------------------------------------------------------------
// <copyright file="FRgbaColor.cs" company="NewSage">
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
public struct FRgbaColor : IEquatable<FRgbaColor>
{
    public float R;
    public float G;
    public float B;
    public float A;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is FRgbaColor other && Equals(other);

    public readonly bool Equals(FRgbaColor other) =>
        Math.Abs(R - other.R) < float.Epsilon
        && Math.Abs(G - other.G) < float.Epsilon
        && Math.Abs(B - other.B) < float.Epsilon
        && Math.Abs(A - other.A) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(R, G, B, A);

    public override readonly string ToString() => $"({R:F2}, {G:F2}, {B:F2}, {A:F2})";

    public static bool operator ==(FRgbaColor left, FRgbaColor right) => left.Equals(right);

    public static bool operator !=(FRgbaColor left, FRgbaColor right) => !left.Equals(right);
}
