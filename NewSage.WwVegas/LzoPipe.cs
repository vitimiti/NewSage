// -----------------------------------------------------------------------
// <copyright file="LzoPipe.cs" company="NewSage">
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

using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace NewSage.WwVegas;

public sealed class LzoPipe : Pipe
{
    private readonly CompressionMode _control;
    private readonly int _blockSize;
    private readonly byte[] _buffer;
    private readonly byte[] _buffer2;
    private int _counter;

    private ushort _headerCompCount = 0xFFFF;
    private ushort _headerUncompressedCount;

    public LzoPipe(CompressionMode control, int blockSize = 1024 * 8)
    {
        _control = control;
        _blockSize = blockSize;

        var safetySize = blockSize + (blockSize / 16) + 64;
        _buffer = new byte[safetySize];
        _buffer2 = new byte[safetySize];
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "This is a complex algorithm and breaking into chunks may make it difficult to follow."
    )]
    public override int Put(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty)
        {
            return base.Put(source);
        }

        var total = 0;
        var sourceLength = source.Length;
        var sourceIndex = 0;

        if (_control is CompressionMode.Decompress)
        {
            while (sourceLength > 0)
            {
                int needed;
                int toCopy;

                if (_headerCompCount == 0xFFFF)
                {
                    needed = 4 - _counter;
                    toCopy = Math.Min(sourceLength, needed);
                    source.Slice(sourceIndex, toCopy).CopyTo(_buffer.AsSpan(_counter));
                    sourceIndex += toCopy;
                    sourceLength -= toCopy;
                    _counter += toCopy;

                    if (_counter == 4)
                    {
                        _headerCompCount = BitConverter.ToUInt16(_buffer, 0);
                        _headerUncompressedCount = BitConverter.ToUInt16(_buffer, 2);
                        _counter = 0;
                    }
                }

                if (sourceLength <= 0 || _headerCompCount == 0xFFFF)
                {
                    continue;
                }

                needed = _headerCompCount - _counter;
                toCopy = Math.Min(sourceLength, needed);
                source.Slice(sourceIndex, toCopy).CopyTo(_buffer.AsSpan(_counter));
                sourceIndex += toCopy;
                sourceLength -= toCopy;
                _counter += toCopy;

                if (_counter != _headerCompCount)
                {
                    continue;
                }

                _ = Lzo.Decompress(_buffer.AsSpan(0, _headerCompCount), _buffer2, out _);
                total += base.Put(_buffer2.AsSpan(0, _headerUncompressedCount));
                _counter = 0;
                _headerCompCount = 0xFFFF;
            }
        }
        else
        {
            if (_counter > 0)
            {
                var toCopy = Math.Min(sourceLength, _blockSize - _counter);
                source.Slice(sourceIndex, toCopy).CopyTo(_buffer.AsSpan(_counter));
                sourceIndex += toCopy;
                sourceLength -= toCopy;
                _counter += toCopy;

                if (_counter == _blockSize)
                {
                    _ = Lzo.Compress(_buffer.AsSpan(0, _blockSize), _buffer2, out var compressedLen);
                    Span<byte> header = stackalloc byte[4];
                    _ = BitConverter.TryWriteBytes(header, (ushort)compressedLen);
                    _ = BitConverter.TryWriteBytes(header[2..], (ushort)_blockSize);
                    total += base.Put(header);
                    total += base.Put(_buffer2.AsSpan(0, compressedLen));
                    _counter = 0;
                }
            }

            while (sourceLength >= _blockSize)
            {
                _ = Lzo.Compress(source.Slice(sourceIndex, _blockSize), _buffer2, out var compressedLen);
                Span<byte> header = new byte[4];
                _ = BitConverter.TryWriteBytes(header, (ushort)compressedLen);
                _ = BitConverter.TryWriteBytes(header[2..], (ushort)_blockSize);
                total += base.Put(header);
                total += base.Put(_buffer2.AsSpan(0, compressedLen));
                sourceIndex += _blockSize;
                sourceLength -= _blockSize;
            }

            if (sourceLength <= 0)
            {
                return total;
            }

            source[sourceIndex..].CopyTo(_buffer);
            _counter = sourceLength;
        }

        return total;
    }

    public override int Flush()
    {
        if (_counter <= 0 || _control is not CompressionMode.Compress)
        {
            return base.Flush();
        }

        _ = Lzo.Compress(_buffer.AsSpan(0, _counter), _buffer2, out var compressedLen);
        Span<byte> header = stackalloc byte[4];
        _ = BitConverter.TryWriteBytes(header, (ushort)compressedLen);
        _ = BitConverter.TryWriteBytes(header[2..], (ushort)_counter);
        _ = base.Put(header);
        _ = base.Put(_buffer2.AsSpan(0, compressedLen));
        _counter = 0;
        return base.Flush();
    }
}
