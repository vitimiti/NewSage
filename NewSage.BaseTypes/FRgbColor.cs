// -----------------------------------------------------------------------
// <copyright file="FRgbColor.cs" company="NewSage">
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
public struct FRgbColor : IEquatable<FRgbColor>
{
    public float R;
    public float G;
    public float B;

    public static FRgbColor FromInt32(int color) =>
        new()
        {
            R = ((color >> 16) & 0xFF) / 255F,
            G = ((color >> 8) & 0xFF) / 255F,
            B = (color & 0xFF) / 255F,
        };

    public readonly int ToInt32() => ((int)(R * 255) << 16) | ((int)(G * 255) << 8) | (int)(B * 255);

    public static explicit operator FRgbColor(int color) => FromInt32(color);

    public static explicit operator int(FRgbColor color) => color.ToInt32();

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is FRgbColor other && Equals(other);

    public readonly bool Equals(FRgbColor other) =>
        Math.Abs(R - other.R) < float.Epsilon
        && Math.Abs(G - other.G) < float.Epsilon
        && Math.Abs(B - other.B) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(R, G, B);

    public override readonly string ToString() => $"0x{ToInt32():X8} => ({R:F2}, {G:F2}, {B:F2})";

    public static bool operator ==(FRgbColor left, FRgbColor right) => left.Equals(right);

    public static bool operator !=(FRgbColor left, FRgbColor right) => !left.Equals(right);
}
