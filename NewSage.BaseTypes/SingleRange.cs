// -----------------------------------------------------------------------
// <copyright file="SingleRange.cs" company="NewSage">
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
public struct SingleRange : IEquatable<SingleRange>
{
    public float Lo;
    public float Hi;

    public void Combine(SingleRange other)
    {
        Lo = float.Min(Lo, other.Lo);
        Hi = float.Max(Hi, other.Hi);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is SingleRange other && Equals(other);

    public readonly bool Equals(SingleRange other) =>
        Math.Abs(Lo - other.Lo) < float.Epsilon && Math.Abs(Hi - other.Hi) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(Lo, Hi);

    public static bool operator ==(SingleRange left, SingleRange right) => left.Equals(right);

    public static bool operator !=(SingleRange left, SingleRange right) => !left.Equals(right);
}
