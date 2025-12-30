// -----------------------------------------------------------------------
// <copyright file="IoVector3.cs" company="NewSage">
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

namespace NewSage.WwVegas;

[StructLayout(LayoutKind.Sequential)]
public struct IoVector3 : IEquatable<IoVector3>
{
    public float X;
    public float Y;
    public float Z;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is IoVector3 other && Equals(other);

    public readonly bool Equals(IoVector3 other) =>
        float.Abs(X - other.X) < float.Epsilon
        && float.Abs(Y - other.Y) < float.Epsilon
        && float.Abs(Z - other.Z) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    public override readonly string ToString() => $"({X}, {Y}, {Z})";

    public static bool operator ==(IoVector3 x, IoVector3 y) => x.Equals(y);

    public static bool operator !=(IoVector3 x, IoVector3 y) => !x.Equals(y);
}
