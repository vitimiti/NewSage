// -----------------------------------------------------------------------
// <copyright file="CrcStraw.cs" company="NewSage">
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

public sealed class CrcStraw : Straw
{
    protected CrcEngine Crc { get; } = new();

    public long Result => Crc.ToInt64();

    public override int Get(Span<byte> buffer)
    {
        if (buffer.Length < 1)
        {
            return 0;
        }

        var counter = base.Get(buffer);
        _ = Crc.ToInt64(buffer[..counter]);
        return counter;
    }
}
