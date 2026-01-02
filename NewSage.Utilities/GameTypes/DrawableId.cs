// -----------------------------------------------------------------------
// <copyright file="DrawableId.cs" company="NewSage">
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

namespace NewSage.Utilities.GameTypes;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public readonly struct DrawableId : IEquatable<DrawableId>
{
    private readonly int _value;

    public static DrawableId Invalid => new(0);

    public bool IsInvalid => _value == 0;

    private DrawableId(int value) => _value = value;

    public static DrawableId FromInt32(int value) => new(value);

    public int ToInt32() => _value;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is DrawableId other && Equals(other);

    public bool Equals(DrawableId other) => _value == other._value;

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => IsInvalid ? "INVALID" : $"{_value}";

    public static bool operator ==(DrawableId left, DrawableId right) => left.Equals(right);

    public static bool operator !=(DrawableId left, DrawableId right) => !left.Equals(right);

    public static implicit operator DrawableId(int value) => new(value);

    public static implicit operator int(DrawableId id) => id._value;
}
