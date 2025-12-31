// -----------------------------------------------------------------------
// <copyright file="Curve3DKey.cs" company="NewSage">
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
public struct Curve3DKey : IEquatable<Curve3DKey>
{
    public Vector3 Point;
    public float Time;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Curve3DKey other && Equals(other);

    public readonly bool Equals(Curve3DKey other) =>
        Point == other.Point && float.Abs(Time - other.Time) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(Point, Time);

    public static bool operator ==(Curve3DKey left, Curve3DKey right) => left.Equals(right);

    public static bool operator !=(Curve3DKey left, Curve3DKey right) => !left.Equals(right);
}
