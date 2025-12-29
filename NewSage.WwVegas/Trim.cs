// -----------------------------------------------------------------------
// <copyright file="Trim.cs" company="NewSage">
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

using System.Text;

namespace NewSage.WwVegas;

public static class Trim
{
    public static int InPlace(Span<byte> buffer)
    {
        var str = Encoding.ASCII.GetString(buffer).Trim();
        var trimmedBytes = Encoding.ASCII.GetBytes(str);
        trimmedBytes.CopyTo(buffer);
        if (trimmedBytes.Length < buffer.Length)
        {
            buffer[trimmedBytes.Length] = 0;
        }

        return trimmedBytes.Length;
    }

    public static int InPlace(Span<char> buffer)
    {
        var trimmed = buffer.ToString().Trim();
        trimmed.AsSpan().CopyTo(buffer);
        if (trimmed.Length < buffer.Length)
        {
            buffer[trimmed.Length] = '\0';
        }

        return trimmed.Length;
    }
}
