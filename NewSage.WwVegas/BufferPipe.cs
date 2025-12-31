// -----------------------------------------------------------------------
// <copyright file="BufferPipe.cs" company="NewSage">
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

public class BufferPipe(Memory<byte> buffer) : Pipe
{
    public BufferPipe(Span<byte> buffer)
        : this(buffer.ToArray().AsMemory()) { }

    public Memory<byte> Buffer { get; } = buffer;

    public int Index { get; private set; }

    public override int Put(ReadOnlySpan<byte> source)
    {
        var total = 0;
        var sourceLength = source.Length;

        if (Buffer.IsEmpty || sourceLength <= 0)
        {
            return total;
        }

        var len = sourceLength;
        if (Buffer.Length != 0)
        {
            var theoreticalMax = Buffer.Length - Index;
            len = (sourceLength < theoreticalMax) ? sourceLength : theoreticalMax;
        }

        if (len > 0)
        {
            source[..len].CopyTo(Buffer.Span[Index..]);
        }

        Index += len;
        total += len;

        return total;
    }
}
