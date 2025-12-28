// -----------------------------------------------------------------------
// <copyright file="ChunkSave.cs" company="NewSage">
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

using System.Diagnostics;

namespace NewSage.WwVegas;

public class ChunkSave(FileStream file)
{
    private const int MaxStackDepth = 256;

    private readonly int[] _positionStack = new int[MaxStackDepth];
    private readonly ChunkHeader[] _headerStack = new ChunkHeader[MaxStackDepth];
    private readonly MicroChunkHeader _microChunkHeader = new();

    private bool _inMicroChunk;
    private int _microChunkPosition;

    public int CurrentChunkDepth { get; private set; }

    public bool BeginChunk(uint id)
    {
        var chunkHeader = new ChunkHeader();
        if (CurrentChunkDepth > 0)
        {
            _headerStack[CurrentChunkDepth - 1].SetSubChunkFlag(onOff: true);
        }

        chunkHeader.ChunkType = id;
        chunkHeader.ChunkSize = 0;
        var filePosition = (int)file.Seek(0, SeekOrigin.Current);

        _positionStack[CurrentChunkDepth] = filePosition;
        _headerStack[CurrentChunkDepth] = chunkHeader;
        CurrentChunkDepth++;

        try
        {
            file.Write(chunkHeader.ToBuffer());
        }
        catch (IOException)
        {
            return false;
        }

        return true;
    }

    public bool EndChunk()
    {
        Debug.Assert(!_inMicroChunk, "Cannot end chunk while in micro chunk");

        var currentPosition = (int)file.Seek(0, SeekOrigin.Current);

        CurrentChunkDepth--;
        var chunkPosition = _positionStack[CurrentChunkDepth];
        ChunkHeader chunkHeader = _headerStack[CurrentChunkDepth];

        _ = file.Seek(chunkPosition, SeekOrigin.Begin);
        try
        {
            file.Write(chunkHeader.ToBuffer());
        }
        catch (IOException)
        {
            return false;
        }

        if (CurrentChunkDepth != 0)
        {
            _headerStack[CurrentChunkDepth - 1].AddSize((uint)(chunkHeader.ChunkSize + ChunkHeader.BufferSize));
        }

        _ = file.Seek(currentPosition, SeekOrigin.Begin);
        return true;
    }

    public bool BeginMicroChunk(byte id)
    {
        Debug.Assert(!_inMicroChunk, "Cannot begin micro chunk while in micro chunk");

        _microChunkHeader.ChunkType = id;
        _microChunkHeader.ChunkSize = 0;
        _microChunkPosition = (int)file.Seek(0, SeekOrigin.Current);

        if (Write(_microChunkHeader.ToBuffer()) > 0)
        {
            return false;
        }

        _inMicroChunk = true;
        return true;
    }

    public bool EndMicroChunk()
    {
        Debug.Assert(_inMicroChunk, "Cannot end micro chunk while not in micro chunk");

        var currentPosition = (int)file.Seek(0, SeekOrigin.Current);

        _ = file.Seek(_microChunkPosition, SeekOrigin.Begin);
        try
        {
            file.Write(_microChunkHeader.ToBuffer());
        }
        catch (IOException)
        {
            return false;
        }

        _ = file.Seek(currentPosition, SeekOrigin.Begin);
        _inMicroChunk = false;
        return true;
    }

    public uint Write(ReadOnlySpan<byte> buffer)
    {
        Debug.Assert(
            !_headerStack[CurrentChunkDepth - 1].SubChunkFlag,
            "You mixed data and chunks within the same chunk. NO NO!"
        );

        Debug.Assert(CurrentChunkDepth > 0, "You didn't open any chunks yet");

        try
        {
            file.Write(buffer);
        }
        catch (IOException)
        {
            return 0;
        }

        _headerStack[CurrentChunkDepth - 1].AddSize((uint)buffer.Length);

        if (_inMicroChunk)
        {
            Debug.Assert(
                _microChunkHeader.ChunkSize < 255 - buffer.Length,
                "Micro chunks can only be 255 bytes or less"
            );

            _microChunkHeader.AddSize((byte)buffer.Length);
        }

        return (uint)buffer.Length;
    }

    public uint Write(IoVector2 v)
    {
        ArgumentNullException.ThrowIfNull(v);
        return Write(v.ToBuffer());
    }

    public uint Write(IoVector3 v)
    {
        ArgumentNullException.ThrowIfNull(v);
        return Write(v.ToBuffer());
    }

    public uint Write(IoVector4 v)
    {
        ArgumentNullException.ThrowIfNull(v);
        return Write(v.ToBuffer());
    }

    public uint Write(IoQuaternion q)
    {
        ArgumentNullException.ThrowIfNull(q);
        return Write(q.ToBuffer());
    }
}
