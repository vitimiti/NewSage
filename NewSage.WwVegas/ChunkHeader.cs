// -----------------------------------------------------------------------
// <copyright file="ChunkHeader.cs" company="NewSage">
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

public class ChunkHeader
{
    private uint _chunkSize;

    public ChunkHeader() { }

    public ChunkHeader(uint type, uint size)
    {
        ChunkType = type;
        ChunkSize = size;
    }

    public uint ChunkType { get; set; }

    public uint ChunkSize
    {
        get => _chunkSize & 0x7FFF_FFFF;
        set
        {
            _chunkSize &= 0x8000_0000;
            _chunkSize |= value & 0x7FFF_FFFF;
        }
    }

    public bool SubChunkFlag => (_chunkSize & 0x8000_0000) != 0;

    internal static int BufferSize => sizeof(uint) * 2;

    public void AddSize(uint add) => ChunkSize += add;

    public void SetSubChunkFlag(bool onOff)
    {
        if (onOff)
        {
            _chunkSize |= 0x8000_0000;
        }
        else
        {
            _chunkSize &= 0x7FFF_FFFF;
        }
    }

    internal static ChunkHeader FromBuffer(ReadOnlySpan<byte> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, BufferSize);
        return new ChunkHeader(BitConverter.ToUInt32(buffer), BitConverter.ToUInt32(buffer[sizeof(uint)..]));
    }

    internal byte[] ToBuffer()
    {
        var buffer = new byte[BufferSize];
        BitConverter.GetBytes(ChunkType).CopyTo(buffer, 0);
        BitConverter.GetBytes(ChunkSize).CopyTo(buffer, sizeof(uint));
        return buffer;
    }
}
