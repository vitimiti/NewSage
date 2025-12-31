// -----------------------------------------------------------------------
// <copyright file="BufferStraw.cs" company="NewSage">
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

public sealed class BufferStraw(ReadOnlyMemory<byte> buffer) : Straw
{
    public BufferStraw(ReadOnlySpan<byte> buffer)
        : this(buffer.ToArray().AsMemory()) { }

    public ReadOnlyMemory<byte> Buffer { get; } = buffer;

    public int Index { get; private set; }

    public override int Get(Span<byte> buffer)
    {
        var total = 0;
        var sourceLength = buffer.Length;

        if (Buffer.IsEmpty || sourceLength <= 0)
        {
            return total;
        }

        var theoreticalMax = Buffer.Length - Index;
        var len = (sourceLength < theoreticalMax) ? sourceLength : theoreticalMax;

        if (len <= 0)
        {
            return total;
        }

        Buffer.Span.Slice(Index, len).CopyTo(buffer);
        Index += len;
        total += len;

        return total;
    }
}
