// -----------------------------------------------------------------------
// <copyright file="FileStraw.cs" company="NewSage">
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

public sealed class FileStraw : Straw
{
    private readonly FileStream _stream;

    private bool _hasOpened;
    private bool _disposed;

    public FileStraw(FileStream fileStream) => _stream = fileStream;

    public override int Get(Span<byte> buffer)
    {
        if (buffer.Length == 0)
        {
            return 0;
        }

        if (!_hasOpened)
        {
            _hasOpened = _stream.CanRead;
        }

        return _hasOpened && buffer.Length > 0 ? _stream.Read(buffer) : 0;
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
