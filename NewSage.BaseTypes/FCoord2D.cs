// -----------------------------------------------------------------------
// <copyright file="FCoord2D.cs" company="NewSage">
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
public struct FCoord2D : IEquatable<FCoord2D>
{
    public float X;
    public float Y;

    public readonly float Length => float.Sqrt((X * X) + (Y * Y));

    public void Normalize()
    {
        var length = Length;
        X /= length;
        Y /= length;
    }

    public readonly float ToAngle()
    {
        var length = Length;
        if (float.Abs(length) < float.Epsilon)
        {
            return 0F;
        }

        var clamped = float.Clamp(X / length, -1F, 1F);
        return Y < 0F ? -float.Acos(clamped) : float.Acos(clamped);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is FCoord2D other && Equals(other);

    public readonly bool Equals(FCoord2D other) =>
        Math.Abs(X - other.X) < float.Epsilon && Math.Abs(Y - other.Y) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(FCoord2D left, FCoord2D right) => left.Equals(right);

    public static bool operator !=(FCoord2D left, FCoord2D right) => !left.Equals(right);
}
