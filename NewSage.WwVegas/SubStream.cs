// -----------------------------------------------------------------------
// <copyright file="SubStream.cs" company="NewSage">
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

public class SubStream : Stream
{
    private readonly Stream _baseStream;
    private readonly long _biasStart;
    private readonly long _biasLength;

    private long _position;
    private bool _disposed;

    public SubStream(Stream baseStream, long start, long length)
    {
        ArgumentNullException.ThrowIfNull(baseStream);

        _baseStream = baseStream;
        _biasStart = start;
        _biasLength = length == -1 ? baseStream.Length - start : length;
        _position = 0;
    }

    public override bool CanRead => _baseStream.CanRead;

    public override bool CanSeek => _baseStream.CanSeek;

    public override bool CanWrite => _baseStream.CanWrite;

    public override long Length => _biasLength;

    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer)
    {
        var remaining = _biasLength - _position;
        if (remaining <= 0)
        {
            return 0;
        }

        var toRead = (int)Math.Min(buffer.Length, remaining);
        _baseStream.Position = _biasStart + _position;

        var read = _baseStream.Read(buffer[..toRead]);
        _position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var target = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _biasLength + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin)),
        };

        _position = Math.Clamp(target, 0, _biasLength);
        return _position;
    }

    public override void SetLength(long value) =>
        throw new NotSupportedException("Cannot resize a biased sub-portion of a file.");

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var remaining = _biasLength - _position;
        if (remaining <= 0)
        {
            return;
        }

        var toWrite = (int)Math.Min(buffer.Length, remaining);
        _baseStream.Position = _biasStart + _position;
        _baseStream.Write(buffer[..toWrite]);
        _position += toWrite;
    }

    public override void Flush() => _baseStream.Flush();

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _baseStream.Dispose();
        }

        base.Dispose(disposing);
        _disposed = true;
    }
}
