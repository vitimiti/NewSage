// -----------------------------------------------------------------------
// <copyright file="GameColor.cs" company="NewSage">
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
using System.Runtime.InteropServices;

namespace NewSage.Utilities;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public readonly struct GameColor : IEquatable<GameColor>
{
    private readonly int _value;

    private GameColor(int value) => _value = value;

    public static GameColor Undefined => new(0x00_FF_FF_FF);

    public static GameColor MakeColor(byte red, byte green, byte blue, byte alpha) =>
        new((alpha << 24) | (red << 16) | (green << 8) | blue);

    public byte Red => (byte)((_value >> 16) & 0xFF);

    public byte Green => (byte)((_value >> 8) & 0xFF);

    public byte Blue => (byte)(_value & 0xFF);

    public byte Alpha => (byte)((_value >> 24) & 0xFF);

    public float FRed => Red / 255F;

    public float FGreen => Green / 255F;

    public float FBlue => Blue / 255F;

    public float FAlpha => Alpha / 255F;

    public static GameColor DarkenColor(GameColor color, int percent = 10)
    {
        if (percent is >= 90 or <= 0)
        {
            return color;
        }

        var r = color.Red;
        var g = color.Green;
        var b = color.Blue;

        r -= unchecked((byte)(r * percent / 100));
        g -= unchecked((byte)(g * percent / 100));
        b -= unchecked((byte)(b * percent / 100));

        return MakeColor(r, g, b, color.Alpha);
    }

    public static GameColor FromInt32(int value) => new(value);

    public int ToInt32() => _value;

    public override bool Equals(object? obj) => obj is GameColor other && Equals(other);

    public bool Equals(GameColor other) => _value == other._value;

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() =>
        $"0x{_value:X8} => (R={Red}({FRed:F2}), G={Green}({FGreen:F2}), B={Blue}({FBlue:F2}), A={Alpha}({FAlpha:F2}))";

    public static bool operator ==(GameColor left, GameColor right) => left.Equals(right);

    public static bool operator !=(GameColor left, GameColor right) => !left.Equals(right);

    public static implicit operator GameColor(int value) => FromInt32(value);

    public static implicit operator int(GameColor color) => color.ToInt32();
}
