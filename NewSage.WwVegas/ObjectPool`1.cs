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

using NewSage.WwVegas.WwDebug;

namespace NewSage.WwVegas;

public class ObjectPool<T>
    where T : class, new()
{
    private readonly Stack<T> _freeList;
    private readonly int _blockSize;
    private readonly Lock _lock = new();

    public ObjectPool(int blockSize = 64)
    {
        _blockSize = blockSize;
        _freeList = new Stack<T>(blockSize);
        AllocateBlock();
    }

    public int FreeCount
    {
        get
        {
            lock (_lock)
            {
                return _freeList.Count;
            }
        }
    }

    public int TotalCount { get; private set; }

    public T Allocate()
    {
        lock (_lock)
        {
            if (_freeList.Count == 0)
            {
                AllocateBlock();
            }

            MemoryLog.RegisterAlloc(MemoryLog.Current);
            return _freeList.Pop();
        }
    }

    public void Free(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj is IPoolable poolable)
        {
            poolable.Reset();
        }

        lock (_lock)
        {
            MemoryLog.RegisterFree(MemoryLog.Current);
            _freeList.Push(obj);
        }
    }

    private void AllocateBlock()
    {
        for (var i = 0; i < _blockSize; i++)
        {
            _freeList.Push(new T());
        }

        TotalCount += _blockSize;
    }
}
