// -----------------------------------------------------------------------
// <copyright file="WwDateTime.cs" company="NewSage">
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

namespace NewSage.WwVegas;

public record WwDateTime(uint Date, uint Time)
{
    public uint Year => unchecked(((Date & 0xFE00_0000) >> (9 + 16)) + 1980);

    public uint Month => unchecked((Date & 0x01E0_0000) >> (5 + 16));

    public uint Day => (Date & 0x001F_0000) >> 16;

    public uint Hour => (Time & 0xF800) >> 11;

    public uint Minute => (Time & 0x07E0) >> 5;

    public uint Second => (Time & 0x001F) << 1;
}
