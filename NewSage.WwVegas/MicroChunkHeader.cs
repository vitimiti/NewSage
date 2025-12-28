// -----------------------------------------------------------------------
// <copyright file="MicroChunkHeader.cs" company="NewSage">
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

public class MicroChunkHeader
{
    public MicroChunkHeader() { }

    public MicroChunkHeader(byte chunkType, byte chunkSize)
    {
        ChunkType = chunkType;
        ChunkSize = chunkSize;
    }

    public byte ChunkType { get; set; }

    public byte ChunkSize { get; set; }

    internal static int BufferSize => sizeof(byte) * 2;

    public void AddSize(byte add) => ChunkSize += add;

    internal static MicroChunkHeader FromBuffer(ReadOnlySpan<byte> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, BufferSize);
        return new MicroChunkHeader(buffer[0], buffer[1]);
    }

    internal byte[] ToBuffer()
    {
        var buffer = new byte[BufferSize];
        buffer[0] = ChunkType;
        buffer[1] = ChunkSize;
        return buffer;
    }
}
