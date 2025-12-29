// -----------------------------------------------------------------------
// <copyright file="NTree`1.cs" company="NewSage">
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

public class NTree<T> : IDisposable
{
    private bool _disposed;

    protected NTreeLeaf<T>? Root { get; set; }

    public NTreeLeaf<T>? PeekRoot() => Root;

    public virtual NTreeLeaf<T> Add(T value)
    {
        if (Root is not null)
        {
            return Root.Add(value);
        }

        Root = AllocateLeaf();
        Root.Value = value;
        return Root;
    }

    public virtual void Reset()
    {
        if (Root is null)
        {
            return;
        }

        NTreeLeaf<T>? endLeaf = Root;
        while (endLeaf.NextSibling is not null)
        {
            endLeaf = endLeaf.NextSibling;
        }

        NTreeLeaf<T>? current = endLeaf;
        while (current is not null)
        {
            NTreeLeaf<T>? prev = current.PrevSibling;
            current.Remove();
            current = prev;
        }

        Root = null;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual NTreeLeaf<T> AllocateLeaf() => new();

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Reset();
        }

        _disposed = true;
    }
}
