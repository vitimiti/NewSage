// -----------------------------------------------------------------------
// <copyright file="ObjectPool`1.cs" company="NewSage">
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

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace NewSage.Utilities;

public sealed class ObjectPool<T>(Func<T> factory, int initialSize, int overflowSize)
    where T : class, IPoolable
{
    private readonly ConcurrentStack<T> _stack = new();

    public void Initialize()
    {
        for (var i = 0; i < initialSize; i++)
        {
            _stack.Push(factory());
        }
    }

    public T Get()
    {
        if (_stack.TryPop(out T? item))
        {
            return item;
        }

        Grow(overflowSize);

        _ = _stack.TryPop(out T? newItem);
        return newItem ?? factory();
    }

    public void Return([NotNull] T item)
    {
        item.Reset();
        _stack.Push(item);
    }

    private void Grow(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _stack.Push(factory());
        }
    }
}
