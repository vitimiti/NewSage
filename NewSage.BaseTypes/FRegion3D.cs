// -----------------------------------------------------------------------
// <copyright file="FRegion3D.cs" company="NewSage">
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

public record FRegion3D(FCoord3D Low, FCoord3D High)
{
    public static FRegion3D Zero => new(FCoord3D.Zero, FCoord3D.Zero);

    public float Width => High.X - Low.X;

    public float Height => High.Y - Low.Y;

    public float Depth => High.Z - Low.Z;

    public bool IsInRegionNoZ(FCoord3D query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return Low.X < query.X && query.X < High.X && Low.Y < query.Y && query.Y < High.Y;
    }

    public bool IsInRegion(FCoord3D query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return IsInRegionNoZ(query) && Low.Z < query.Z && query.Z < High.Z;
    }
}
