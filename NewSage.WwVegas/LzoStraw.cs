// -----------------------------------------------------------------------
// <copyright file="LzoStraw.cs" company="NewSage">
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

public sealed class LzoStraw : Straw
{
    private readonly CompressionMode _control;
    private readonly int _blockSize;
    private readonly byte[] _buffer;
    private readonly byte[] _buffer2;
    private int _counter;

    private ushort _headerCompCount;
    private ushort _headerUncompCount;

    public LzoStraw(CompressionMode control, int blockSize = 1024 * 8)
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
    public override int Get(Span<byte> buffer)
    {
        var total = 0;
        var sourceLength = buffer.Length;
        var destIndex = 0;

        while (sourceLength > 0)
        {
            if (_counter > 0)
            {
                var len = Math.Min(sourceLength, _counter);
                if (_control is CompressionMode.Decompress)
                {
                    _buffer.AsSpan(_headerUncompCount - _counter, len).CopyTo(buffer[destIndex..]);
                }
                else
                {
                    _buffer2.AsSpan(_headerCompCount + 4 - _counter, len).CopyTo(buffer[destIndex..]);
                }

                destIndex += len;
                sourceLength -= len;
                _counter -= len;
                total += len;
            }

            if (sourceLength == 0)
            {
                break;
            }

            if (_control is CompressionMode.Decompress)
            {
                Span<byte> header = new byte[4];
                if (base.Get(header) != 4)
                {
                    break;
                }

                _headerCompCount = BitConverter.ToUInt16(header[..2]);
                _headerUncompCount = BitConverter.ToUInt16(header[2..]);

                var staging = new byte[_headerCompCount];
                if (base.Get(staging) != _headerCompCount)
                {
                    break;
                }

                _ = Lzo.Decompress(staging, _buffer, out _);
                _counter = _headerUncompCount;
            }
            else
            {
                var read = base.Get(_buffer.AsSpan(0, _blockSize));
                if (read == 0)
                {
                    break;
                }

                _ = Lzo.Compress(_buffer.AsSpan(0, read), _buffer2.AsSpan(4), out var compLen);
                _headerCompCount = (ushort)compLen;
                _headerUncompCount = (ushort)read;

                _ = BitConverter.TryWriteBytes(_buffer2.AsSpan(0, 2), _headerCompCount);
                _ = BitConverter.TryWriteBytes(_buffer2.AsSpan(2, 2), _headerUncompCount);

                _counter = compLen + 4;
            }
        }

        return total;
    }
}
