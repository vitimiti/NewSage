// -----------------------------------------------------------------------
// <copyright file="NameKeyType.cs" company="NewSage">
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

namespace NewSage.Game.NameKeys;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public readonly struct NameKeyType : IEquatable<NameKeyType>
{
    private const uint MaximumValue = 1 << 23;

    private readonly uint _value;

    private NameKeyType(uint value) => _value = value;

    public static NameKeyType Invalid => new(0);

    public static NameKeyType Maximum => new(MaximumValue);

    public bool IsValid => _value != 0 && _value <= Maximum;

    public static NameKeyType FromUInt32(uint value)
    {
        Debug.Assert(value is <= MaximumValue or 0x7FFF_FFFF, "NameKey exceeds 24-bit limit!");
        return new NameKeyType(value);
    }

    public uint ToUInt32() => _value;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is NameKeyType other && Equals(other);

    public bool Equals(NameKeyType other) => _value == other._value;

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => IsValid ? $"0x{_value:X8} => ({_value})" : "INVALID";

    public static bool operator ==(NameKeyType left, NameKeyType right) => left._value == right._value;

    public static bool operator !=(NameKeyType left, NameKeyType right) => left._value != right._value;

    public static implicit operator NameKeyType(uint value) => FromUInt32(value);

    public static implicit operator uint(NameKeyType keyType) => keyType.ToUInt32();
}
