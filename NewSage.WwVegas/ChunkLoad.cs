// -----------------------------------------------------------------------
// <copyright file="ChunkLoad.cs" company="NewSage">
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
using System.Runtime.InteropServices;

namespace NewSage.WwVegas;

public class ChunkLoad(FileStream file)
{
    private const int MaxStackDepth = 256;

    private readonly uint[] _positionStack = new uint[MaxStackDepth];
    private readonly ChunkHeader[] _headerStack = new ChunkHeader[MaxStackDepth];

    private bool _inMicroChunk;
    private int _microChunkPosition;
    private MicroChunkHeader _microChunkHeader = new();

    public uint CurrentChunkId
    {
        get
        {
            Debug.Assert(CurrentChunkDepth >= 1, "Cannot get chunk id when stack is empty");
            return _headerStack[CurrentChunkDepth - 1].ChunkType;
        }
    }

    public uint CurrentChunkLength
    {
        get
        {
            Debug.Assert(CurrentChunkDepth >= 1, "Cannot get chunk length when stack is empty");
            return _headerStack[CurrentChunkDepth - 1].ChunkSize;
        }
    }

    public int CurrentChunkDepth { get; private set; }

    public bool ContainsChunks
    {
        get
        {
            Debug.Assert(CurrentChunkDepth >= 1, "Cannot check for chunks when stack is empty");
            return _headerStack[CurrentChunkDepth - 1].SubChunkFlag;
        }
    }

    public uint CurrentMicroChunkId
    {
        get
        {
            Debug.Assert(_inMicroChunk, "Cannot get micro chunk id when not in micro chunk");
            return _microChunkHeader.ChunkType;
        }
    }

    public uint CurrentMicroChunkLength
    {
        get
        {
            Debug.Assert(_inMicroChunk, "Cannot get micro chunk length when not in micro chunk");
            return _microChunkHeader.ChunkSize;
        }
    }

    public bool OpenChunk()
    {
        Debug.Assert(!_inMicroChunk, "Cannot open a chunk while inside a microchunk");
        Debug.Assert(CurrentChunkDepth < MaxStackDepth - 1, "Stack overflow: too many nested chunks");

        if (
            CurrentChunkDepth > 0
            && _positionStack[CurrentChunkDepth - 1] == _headerStack[CurrentChunkDepth - 1].ChunkSize
        )
        {
            return false;
        }

        var headerBuffer = new byte[ChunkHeader.BufferSize];
        if (file.Read(headerBuffer) != ChunkHeader.BufferSize)
        {
            return false;
        }

        _headerStack[CurrentChunkDepth] = ChunkHeader.FromBuffer(headerBuffer);
        _positionStack[CurrentChunkDepth] = 0;
        CurrentChunkDepth++;
        return true;
    }

    public bool CloseChunk()
    {
        Debug.Assert(!_inMicroChunk, "Cannot close a chunk while in micro chunk");
        Debug.Assert(CurrentChunkDepth > 0, "Cannot close a chunk when stack is empty");

        var chunkSize = _headerStack[CurrentChunkDepth - 1].ChunkSize;
        var position = (int)_positionStack[CurrentChunkDepth - 1];

        if (position < chunkSize)
        {
            _ = file.Seek(chunkSize - position, SeekOrigin.Current);
        }

        CurrentChunkDepth--;
        if (CurrentChunkDepth > 0)
        {
            _positionStack[CurrentChunkDepth - 1] += (uint)(chunkSize + ChunkHeader.BufferSize);
        }

        return true;
    }

    public bool OpenMicroChunk()
    {
        Debug.Assert(!_inMicroChunk, "Cannot open a micro chunk while in a micro chunk");

        var microChunkBuffer = new byte[MicroChunkHeader.BufferSize];
        if (file.Read(microChunkBuffer) != MicroChunkHeader.BufferSize)
        {
            return false;
        }

        _microChunkHeader = MicroChunkHeader.FromBuffer(microChunkBuffer);
        _inMicroChunk = true;
        _microChunkPosition = 0;
        return true;
    }

    public bool CloseMicroChunk()
    {
        Debug.Assert(_inMicroChunk, "Cannot close micro chunk if not in micro chunk");

        _inMicroChunk = false;

        var chunkSize = _microChunkHeader.ChunkSize;
        var position = _microChunkPosition;

        if (position < chunkSize)
        {
            _ = file.Seek(chunkSize - position, SeekOrigin.Current);

            if (CurrentChunkDepth > 0)
            {
                _positionStack[CurrentChunkDepth - 1] += (uint)(chunkSize - position);
            }
        }

        return true;
    }

    public uint Seek(uint bytesCount)
    {
        Debug.Assert(CurrentChunkDepth >= 1, "Cannot seek when stack is empty");

        if (_positionStack[CurrentChunkDepth - 1] + bytesCount > (int)_headerStack[CurrentChunkDepth - 1].ChunkSize)
        {
            return 0;
        }

        if (_inMicroChunk && _microChunkPosition + bytesCount > _microChunkHeader.ChunkSize)
        {
            return 0;
        }

        var currentPosition = (uint)file.Position;
        if (file.Seek(bytesCount, SeekOrigin.Current) - currentPosition != bytesCount)
        {
            return 0;
        }

        _positionStack[CurrentChunkDepth - 1] += bytesCount;

        if (_inMicroChunk)
        {
            _microChunkPosition += (int)bytesCount;
        }

        return bytesCount;
    }

    public uint Read(Span<byte> buffer)
    {
        Debug.Assert(CurrentChunkDepth >= 1, "Cannot read when stack is empty");

        if (_positionStack[CurrentChunkDepth - 1] + buffer.Length > (int)_headerStack[CurrentChunkDepth - 1].ChunkSize)
        {
            return 0;
        }

        if (_inMicroChunk && _microChunkPosition + buffer.Length > _microChunkHeader.ChunkSize)
        {
            return 0;
        }

        if (file.Read(buffer) != buffer.Length)
        {
            return 0;
        }

        _positionStack[CurrentChunkDepth - 1] += (uint)buffer.Length;

        if (_inMicroChunk)
        {
            _microChunkPosition += buffer.Length;
        }

        return (uint)buffer.Length;
    }

    public uint Read(out IoVector2 v)
    {
        Span<byte> buffer = stackalloc byte[Marshal.SizeOf<IoVector2>()];
        var result = Read(buffer);
        v = MemoryMarshal.Read<IoVector2>(buffer);
        return result;
    }

    public uint Read(out IoVector3 v)
    {
        Span<byte> buffer = stackalloc byte[Marshal.SizeOf<IoVector3>()];
        var result = Read(buffer);
        v = MemoryMarshal.Read<IoVector3>(buffer);
        return result;
    }

    public uint Read(out IoVector4 v)
    {
        Span<byte> buffer = stackalloc byte[Marshal.SizeOf<IoVector4>()];
        var result = Read(buffer);
        v = MemoryMarshal.Read<IoVector4>(buffer);
        return result;
    }

    public uint Read(out IoQuaternion q)
    {
        Span<byte> buffer = stackalloc byte[Marshal.SizeOf<IoQuaternion>()];
        var result = Read(buffer);
        q = MemoryMarshal.Read<IoQuaternion>(buffer);
        return result;
    }
}
