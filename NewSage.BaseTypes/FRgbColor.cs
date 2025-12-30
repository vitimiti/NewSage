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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NewSage.BaseTypes;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage(
    "Performance",
    "CA1815:Override equals and operator equals on value types",
    Justification = "Not used in these types."
)]
public struct FRgbColor
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
}
