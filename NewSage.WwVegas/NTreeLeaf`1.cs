// -----------------------------------------------------------------------
// <copyright file="NTreeLeaf`1.cs" company="NewSage">
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

public class NTreeLeaf<T> : IDisposable
{
    private bool _disposed;

    public T? Value { get; set; }

    public NTreeLeaf<T>? Parent { get; internal set; }

    public NTreeLeaf<T>? Child { get; internal set; }

    public NTreeLeaf<T>? NextSibling { get; internal set; }

    public NTreeLeaf<T>? PrevSibling { get; internal set; }

    public virtual NTreeLeaf<T> AddChild(T value)
    {
        var newChild = new NTreeLeaf<T> { Value = value, Parent = this };

        if (Child is not null)
        {
            Child.PrevSibling = newChild;
            newChild.NextSibling = Child;
        }

        Child = newChild;
        return newChild;
    }

    public virtual NTreeLeaf<T> Add(T value)
    {
        var newLeaf = new NTreeLeaf<T>
        {
            Value = value,
            PrevSibling = this,
            NextSibling = NextSibling,
        };

        _ = NextSibling?.PrevSibling = newLeaf;
        NextSibling = newLeaf;
        return newLeaf;
    }

    public virtual void Remove()
    {
        if (Parent is not null && Parent.Child == this)
        {
            Parent.Child = NextSibling;
        }

        while (Child is not null)
        {
            Child.Remove();
        }

        _ = NextSibling?.PrevSibling = PrevSibling;
        _ = PrevSibling?.NextSibling = NextSibling;

        Parent = null;
        NextSibling = null;
        PrevSibling = null;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
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
            Remove();
        }

        _disposed = true;
    }
}
