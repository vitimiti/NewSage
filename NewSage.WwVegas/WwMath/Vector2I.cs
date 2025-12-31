// -----------------------------------------------------------------------
// <copyright file="Vector2I.cs" company="NewSage">
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
public struct Vector2I : IEquatable<Vector2I>
{
    public int I;
    public int J;

    public Vector2I(int i, int j) => Set(i, j);

    public void Set(int i, int j) => (I, J) = (i, j);

    public void Swap(ref Vector2I other)
    {
        I ^= other.I;
        other.I ^= I;
        I ^= other.I;

        J ^= other.J;
        other.J ^= J;
        J ^= other.J;
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Vector2I other && Equals(other);

    public readonly bool Equals(Vector2I other) => I == other.I && J == other.J;

    public override readonly int GetHashCode() => HashCode.Combine(I, J);

    public override readonly string ToString() => $"({I}, {J})";

    public static bool operator ==(Vector2I x, Vector2I y) => x.Equals(y);

    public static bool operator !=(Vector2I x, Vector2I y) => !x.Equals(y);

    public int this[int index]
    {
        readonly get =>
            index switch
            {
                0 => I,
                1 => J,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index must be between 0 and 1 inclusive."
                ),
            };
        set
        {
            switch (index)
            {
                case 0:
                    I = value;
                    break;
                case 1:
                    J = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 1 inclusive.");
            }
        }
    }
}
