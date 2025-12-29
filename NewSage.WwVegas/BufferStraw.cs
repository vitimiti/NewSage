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

public class BufferStraw : Straw
{
    private readonly BufferedStream _stream;

    private int _index;
    private bool _disposed;

    public BufferStraw(BufferedStream stream) => _stream = stream;

    public BufferStraw(ReadOnlySpan<byte> buffer)
    {
        using var ms = new MemoryStream(buffer.ToArray());
        _stream = new BufferedStream(ms);
    }

    private bool IsValid => _stream.CanRead;

    public override int Get(Span<byte> buffer)
    {
        if (!IsValid || buffer.Length <= 0)
        {
            return 0;
        }

        var length = buffer.Length;
        if (_stream.Length != 0)
        {
            var theoreticalMax = _stream.Length - _index;
            length = (int)(buffer.Length < theoreticalMax ? buffer.Length : theoreticalMax);
        }

        if (length > 0)
        {
            _stream.Position = _index;
            _ = _stream.Read(buffer[..length]);
        }

        _index += length;
        return length;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _stream.Dispose();
        }

        base.Dispose(disposing);
        _disposed = true;
    }
}
