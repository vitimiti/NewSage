// -----------------------------------------------------------------------
// <copyright file="Straw.cs" company="NewSage">
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

namespace NewSage.WwVegas;

public class Straw : IDisposable
{
    private bool _disposed;

    public Straw? ChainTo { get; set; }

    public Straw? ChainFrom { get; set; }

    public virtual void GetFrom(Straw? straw)
    {
        if (ChainTo != straw)
        {
            if (straw?.ChainFrom is not null)
            {
                straw.ChainFrom.GetFrom(null);
                straw.ChainFrom = null;
            }

            _ = ChainTo?.ChainFrom = null;

            ChainTo = straw;
            _ = ChainTo?.ChainFrom = this;
        }
    }

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "This is a valid name.")]
    public virtual int Get(Span<byte> buffer) => ChainTo?.Get(buffer) ?? 0;

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
            ChainFrom?.GetFrom(ChainTo);

            ChainFrom?.Dispose();
            ChainTo?.Dispose();

            ChainFrom = null;
            ChainTo = null;
        }

        _disposed = true;
    }
}
