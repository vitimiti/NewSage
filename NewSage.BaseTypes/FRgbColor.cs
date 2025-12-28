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

namespace NewSage.BaseTypes;

public record FRgbColor(float Red, float Green, float Blue)
{
    public static FRgbColor FromInt32(int color) =>
        new(((color >> 16) & 0xFF) / 255F, ((color >> 8) & 0xFF) / 255F, (color & 0xFF) / 255F);

    public int ToInt32() => ((int)(Red * 255) << 16) | ((int)(Green * 255) << 8) | (int)(Blue * 255);

    public static explicit operator FRgbColor(int color) => FromInt32(color);

    public static explicit operator int(FRgbColor color)
    {
        ArgumentNullException.ThrowIfNull(color);
        return color.ToInt32();
    }
}
