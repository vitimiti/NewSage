// -----------------------------------------------------------------------
// <copyright file="Vector3I16.cs" company="NewSage">
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
public struct Vector3I16 : IEquatable<Vector3I16>
{
    public ushort I;
    public ushort J;
    public ushort K;

    public Vector3I16(ushort i, ushort j, ushort k) => Set(i, j, k);

    public void Set(ushort i, ushort j, ushort k) => (I, J, K) = (i, j, k);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Vector3I16 other && Equals(other);

    public readonly bool Equals(Vector3I16 other) => I == other.I && J == other.J && K == other.K;

    public override readonly int GetHashCode() => HashCode.Combine(I, J, K);

    public override readonly string ToString() => $"({I}, {J}, {K})";

    public static bool operator ==(Vector3I16 x, Vector3I16 y) => x.Equals(y);

    public static bool operator !=(Vector3I16 x, Vector3I16 y) => !x.Equals(y);

    public ushort this[int index]
    {
        readonly get =>
            index switch
            {
                0 => I,
                1 => J,
                2 => K,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index must be between 0 and 2 inclusive."
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
                case 2:
                    K = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 2 inclusive.");
            }
        }
    }
}
