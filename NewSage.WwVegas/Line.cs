// -----------------------------------------------------------------------
// <copyright file="Line.cs" company="NewSage">
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

public static class Line
{
    public static int Read(FileStream fileStream, Span<byte> buffer, out bool eof)
    {
        using var fs = new FileStraw(fileStream);
        return Read(fs, buffer, out eof);
    }

    public static int Read(Straw straw, Span<byte> buffer, out bool eof)
    {
        ArgumentNullException.ThrowIfNull(straw);

        eof = false;
        if (buffer.Length == 0)
        {
            return 0;
        }

        var count = 0;
        Span<byte> c = stackalloc byte[1];

        while (true)
        {
            if (straw.Get(c) != 1)
            {
                eof = true;
                if (count < buffer.Length)
                {
                    buffer[count] = 0;
                }

                break;
            }

            if (c[0] == (byte)'\x0A')
            {
                break;
            }

            if (c[0] != (byte)'\x0D' && count + 1 < buffer.Length)
            {
                buffer[count++] = c[0];
            }
        }

        if (count < buffer.Length)
        {
            buffer[count] = 0;
        }

        return TrimInPlace(buffer, ref count);
    }

    public static int Read(Straw straw, Span<char> buffer, out bool eof)
    {
        ArgumentNullException.ThrowIfNull(straw);

        eof = false;
        if (buffer.Length == 0)
        {
            return 0;
        }

        var count = 0;
        Span<byte> bytes = stackalloc byte[2];

        while (true)
        {
            if (straw.Get(bytes) != 2)
            {
                eof = true;
                if (count < buffer.Length)
                {
                    buffer[count] = '\0';
                }

                break;
            }

            var c = BitConverter.ToChar(bytes);

            if (c == '\x0A')
            {
                break;
            }

            if (c != '\x0D' && count + 1 < buffer.Length)
            {
                buffer[count++] = c;
            }
        }

        if (count < buffer.Length)
        {
            buffer[count] = '\0';
        }

        return TrimInPlace(buffer, ref count);
    }

    private static int TrimInPlace(Span<byte> buffer, ref int count)
    {
        var str = Encoding.ASCII.GetString(buffer[..count]).Trim();
        var trimmedBytes = Encoding.ASCII.GetBytes(str);
        trimmedBytes.CopyTo(buffer);
        if (trimmedBytes.Length < buffer.Length)
        {
            buffer[trimmedBytes.Length] = 0;
        }

        return trimmedBytes.Length;
    }

    private static int TrimInPlace(Span<char> buffer, ref int count)
    {
        var trimmed = buffer[..count].ToString().Trim();
        trimmed.AsSpan().CopyTo(buffer);
        if (trimmed.Length < buffer.Length)
        {
            buffer[trimmed.Length] = '\0';
        }

        return trimmed.Length;
    }
}
