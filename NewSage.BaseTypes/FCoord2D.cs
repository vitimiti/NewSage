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

namespace NewSage.BaseTypes;

public record FCoord2D(float X, float Y)
{
    public float Length => float.Sqrt((X * X) + (Y * Y));

    public FCoord2D Normalized
    {
        get
        {
            var length = Length;
            return float.Abs(length) >= float.Epsilon ? new FCoord2D(X / length, Y / length) : this;
        }
    }

    public float ToAngle()
    {
        var length = Length;
        if (float.Abs(length) < float.Epsilon)
        {
            return 0F;
        }

        var clamped = float.Clamp(X / length, -1F, 1F);
        return Y < 0F ? -float.Acos(clamped) : float.Acos(clamped);
    }
}
