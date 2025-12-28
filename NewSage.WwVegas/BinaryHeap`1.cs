// -----------------------------------------------------------------------
// <copyright file="BinaryHeap`1.cs" company="NewSage">
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

using System.Diagnostics;

namespace NewSage.WwVegas;

public class BinaryHeap<TKey> : IDisposable
    where TKey : IComparable<TKey>
{
    private IList<IHeapNode<TKey>?>? _elements;
    private bool _ownArray;
    private bool _disposed;

    public BinaryHeap(IList<IHeapNode<TKey>?> list, int maxNumberOfElements)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentOutOfRangeException.ThrowIfLessThan(list.Count, 1);

        _elements = list;
        MaxElementCount = maxNumberOfElements;
        ElementCount = 0;
        _ownArray = false;
    }

    public BinaryHeap(int maxNumberOfElements)
    {
        MaxElementCount = maxNumberOfElements;
        ElementCount = 0;
        _elements = null;
        _ownArray = false;

        ResizeArray(MaxElementCount);
    }

    public int ElementCount { get; private set; }

    public int MaxElementCount { get; private set; }

    public void FlushArray()
    {
        if (_elements is not null)
        {
            for (var i = 0; i < _elements.Count; i++)
            {
                _elements[i] = null;
            }
        }

        ElementCount = 0;
    }

    public void ResizeArray(int newSize)
    {
        ReleaseArray();

        _elements = new IHeapNode<TKey>[newSize];
        MaxElementCount = newSize;
        ElementCount = 0;
        _ownArray = true;

        for (var i = 0; i < _elements.Count; i++)
        {
            _elements[i] = null;
        }
    }

    public void ReleaseArray()
    {
        if (_ownArray)
        {
            _elements?.Clear();
            _elements = null;
            ElementCount = 0;
            MaxElementCount = 0;
        }

        _ownArray = false;
    }

    public IHeapNode<TKey>? PeekNode(int location) => _elements![location];

    public void Insert(IHeapNode<TKey> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var i = ++ElementCount;
        Debug.Assert(
            ElementCount < MaxElementCount,
            $"Element count of {ElementCount} is greater than max element count of {MaxElementCount}."
        );

        while (_elements![i / 2] > node)
        {
            _elements[i] = _elements[i / 2];
            _elements[i]!.HeapLocation = i;
            i /= 2;
        }

        _elements[i] = node;
        _elements[i]!.HeapLocation = i;
    }

    public void PerlocateUp(int location)
    {
        Debug.Assert(location < ElementCount, $"Location {location} is greater than element count {ElementCount}.");

        var i = location;
        IHeapNode<TKey> node = _elements![i]!;

        while (_elements[i / 2] > node)
        {
            _elements[i] = _elements[i / 2];
            _elements[i]!.HeapLocation = i;
            i /= 2;
        }

        _elements[i] = node;
        _elements[i]!.HeapLocation = i;
    }

    public IHeapNode<TKey>? RemoveMin()
    {
        int child;
        if (ElementCount == 0)
        {
            return null;
        }

        Debug.Assert(ElementCount > 0, "Element count is 0.");
        Debug.Assert(_elements is not null, $"{nameof(_elements)} is null.");

        IHeapNode<TKey>? minElement = _elements[1];
        _ = minElement?.HeapLocation = 0;

        IHeapNode<TKey>? lastElement = _elements[ElementCount--];
        int i;
        for (i = 1; (i * 2) <= ElementCount; i = child)
        {
            child = i * 2;
            if (child != ElementCount && _elements[child + 1] < _elements[child])
            {
                child++;
            }

            if (lastElement > _elements[child])
            {
                _elements[i] = _elements[child];
                _elements[i]!.HeapLocation = i;
            }
            else
            {
                break;
            }
        }

        _elements[i] = lastElement;
        _elements[i]!.HeapLocation = i;

        return minElement;
    }

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
            ReleaseArray();
        }

        _disposed = true;
    }
}
