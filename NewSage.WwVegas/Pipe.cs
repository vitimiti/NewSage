// -----------------------------------------------------------------------
// <copyright file="Pipe.cs" company="NewSage">
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

using System.Runtime.InteropServices;

namespace NewSage.WwVegas;

public class Pipe : IDisposable
{
    private bool _disposed;

    public Pipe? ChainTo { get; set; }

    public Pipe? ChainFrom { get; set; }

    public virtual int Flush() => ChainTo?.Flush() ?? 0;

    public virtual int End() => Flush();

    public virtual void PutTo(Pipe? pipe)
    {
        if (ReferenceEquals(ChainTo, pipe))
        {
            return;
        }

        if (pipe is { ChainFrom: not null })
        {
            pipe.ChainFrom.PutTo(null);
            pipe.ChainFrom = null;
        }

        if (ChainTo != null)
        {
            ChainTo.ChainFrom = null;
            _ = ChainTo.Flush();
        }

        ChainTo = pipe;
        _ = ChainTo?.ChainFrom = this;
    }

    public virtual int Put(ReadOnlySpan<byte> source) => ChainTo?.Put(source) ?? source.Length;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _ = ChainTo?.ChainFrom = ChainFrom;
            ChainFrom?.PutTo(ChainTo);

            ChainFrom?.Dispose();
            ChainTo?.Dispose();

            ChainFrom = null;
            ChainTo = null;
        }

        _disposed = true;
    }
}
