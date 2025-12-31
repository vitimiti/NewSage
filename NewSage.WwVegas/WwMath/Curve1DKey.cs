// -----------------------------------------------------------------------
// <copyright file="Curve1DKey.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwMath;

[StructLayout(LayoutKind.Sequential)]
public struct Curve1DKey : IEquatable<Curve1DKey>
{
    public float Point;
    public float Time;
    public uint Extra;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Curve1DKey other && Equals(other);

    public readonly bool Equals(Curve1DKey other) =>
        float.Abs(Point - other.Point) < float.Epsilon
        && float.Abs(Time - other.Time) < float.Epsilon
        && Extra == other.Extra;

    public override readonly int GetHashCode() => HashCode.Combine(Point, Time, Extra);

    public static bool operator ==(Curve1DKey left, Curve1DKey right) => left.Equals(right);

    public static bool operator !=(Curve1DKey left, Curve1DKey right) => !left.Equals(right);
}
