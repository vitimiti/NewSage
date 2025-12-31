// -----------------------------------------------------------------------
// <copyright file="IoQuaternion.cs" company="NewSage">
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
public struct IoQuaternion : IEquatable<IoQuaternion>
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) =>
        obj is IoQuaternion quaternion && Equals(quaternion);

    public readonly bool Equals(IoQuaternion other) =>
        float.Abs(X - other.X) < float.Epsilon
        && float.Abs(Y - other.Y) < float.Epsilon
        && float.Abs(Z - other.Z) < float.Epsilon
        && float.Abs(W - other.W) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    public static bool operator ==(IoQuaternion x, IoQuaternion y) => x.Equals(y);

    public static bool operator !=(IoQuaternion x, IoQuaternion y) => !x.Equals(y);

    public float this[int index]
    {
        readonly get =>
            index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index must be between 0 and 3 inclusive."
                ),
            };
        set
        {
            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                case 3:
                    W = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 3 inclusive.");
            }
        }
    }
}
