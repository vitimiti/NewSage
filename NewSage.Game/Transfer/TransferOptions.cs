// -----------------------------------------------------------------------
// <copyright file="TransferOptions.cs" company="NewSage">
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

namespace NewSage.Game.Transfer;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public readonly struct TransferOptions : IEquatable<TransferOptions>
{
    private readonly uint _value;

    private TransferOptions(uint value) => _value = value;

    public static TransferOptions None => new(0x0000_0000);

    public static TransferOptions NoPostProcessing => new(0x0000_0001);

    public static TransferOptions All => new(0xFFFF_FFFF);

    public static TransferOptions FromUInt32(uint value) => new(value);

    public uint ToUInt32() => _value;

    public TransferOptions BitwiseAnd(TransferOptions other) => new(_value & other._value);

    public TransferOptions BitwiseOr(TransferOptions other) => new(_value | other._value);

    public TransferOptions Xor(TransferOptions other) => new(_value ^ other._value);

    public TransferOptions OnesComplement() => new(~_value);

    public bool Equals(TransferOptions other) => _value == other._value;

    public override bool Equals(object? obj) => obj is TransferOptions other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => $"0x{_value:X8} => ({_value})";

    public static TransferOptions operator |(TransferOptions left, TransferOptions right) => left.BitwiseOr(right);

    public static TransferOptions operator &(TransferOptions left, TransferOptions right) => left.BitwiseAnd(right);

    public static TransferOptions operator ^(TransferOptions left, TransferOptions right) => left.Xor(right);

    public static TransferOptions operator ~(TransferOptions value) => value.OnesComplement();

    public static bool operator ==(TransferOptions left, TransferOptions right) => left.Equals(right);

    public static bool operator !=(TransferOptions left, TransferOptions right) => !left.Equals(right);

    public static implicit operator TransferOptions(uint value) => FromUInt32(value);

    public static implicit operator uint(TransferOptions value) => value.ToUInt32();
}
