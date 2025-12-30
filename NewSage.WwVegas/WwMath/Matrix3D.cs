// -----------------------------------------------------------------------
// <copyright file="Matrix3D.cs" company="NewSage">
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
public struct Matrix3D : IEquatable<Matrix3D>
{
    public Vector4 Row0;
    public Vector4 Row1;
    public Vector4 Row2;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Matrix3D other && Equals(other);

    public readonly bool Equals(Matrix3D other) => Row0 == other.Row0 && Row1 == other.Row1 && Row2 == other.Row2;

    public override readonly int GetHashCode() => HashCode.Combine(Row0, Row1, Row2);

    public override readonly string ToString() => $"Matrix3D({Row0}, {Row1}, {Row2})";

    public static bool operator ==(Matrix3D x, Matrix3D y) => x.Equals(y);

    public static bool operator !=(Matrix3D x, Matrix3D y) => !x.Equals(y);

    public Vector4 this[int index]
    {
        readonly get =>
            index switch
            {
                0 => Row0,
                1 => Row1,
                2 => Row2,
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
                    Row0 = value;
                    break;
                case 1:
                    Row1 = value;
                    break;
                case 2:
                    Row2 = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        index,
                        "Index must be between 0 and 2 inclusive."
                    );
            }
        }
    }
}
