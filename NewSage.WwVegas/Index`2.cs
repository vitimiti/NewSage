// -----------------------------------------------------------------------
// <copyright file="Index`2.cs" company="NewSage">
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

public sealed class Index<TKey, TValue> : IDisposable
    where TKey : IComparable<TKey>
{
    private NodeElement[]? _indexTable;
    private int _indexSize;
    private bool _isSorted;
    private int _archiveIndex = -1;
    private bool _disposed;

    public Index() => InvalidateArchive();

    public int Count { get; private set; }

    public TValue? this[TKey id] => IsPresent(id) ? _indexTable![_archiveIndex].Data : default;

    public bool AddIndex(TKey id, TValue data)
    {
#if DEBUG
        for (var i = 0; i < Count; i++)
        {
            Debug.Assert(!_indexTable![i].Id.Equals(id), "Duplicate ID added to IndexClass.");
        }
#endif

        if (Count + 1 > _indexSize)
        {
            if (!IncreaseTableSize(_indexSize == 0 ? 10 : _indexSize))
            {
                return false;
            }
        }

        _indexTable![Count] = new NodeElement(id, data);
        Count++;
        _isSorted = false;
        return true;
    }

    public bool RemoveIndex(TKey id)
    {
        var foundIndex = -1;
        for (var i = 0; i < Count; i++)
        {
            if (_indexTable![i].Id.CompareTo(id) != 0)
            {
                continue;
            }

            foundIndex = i;
            break;
        }

        if (foundIndex == -1)
        {
            return false;
        }

        for (var i = foundIndex + 1; i < Count; i++)
        {
            _indexTable![i - 1] = _indexTable[i];
        }

        Count--;
        _indexTable![Count] = default;
        InvalidateArchive();
        return true;
    }

    public bool IsPresent(TKey id)
    {
        if (Count == 0)
        {
            return false;
        }

        if (IsArchiveSame(id))
        {
            return true;
        }

        var nodeIndex = SearchForNode(id);
        if (nodeIndex == -1)
        {
            return false;
        }

        SetArchive(nodeIndex);
        return true;
    }

    public TValue FetchByPosition(int pos)
    {
        Debug.Assert(pos >= 0 && pos < Count, $"Position {pos} is out of bounds.");
        return _indexTable![pos].Data;
    }

    public TKey FetchIDByPosition(int pos)
    {
        Debug.Assert(pos >= 0 && pos < Count, $"Position {pos} is out of bounds.");
        return _indexTable![pos].Id;
    }

    public void Clear()
    {
        _indexTable = null;
        Count = 0;
        _indexSize = 0;
        _isSorted = false;
        InvalidateArchive();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Clear();

        _disposed = true;
    }

    private void InvalidateArchive() => _archiveIndex = -1;

    private bool IncreaseTableSize(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        var newSize = _indexSize + amount;
        var newTable = new NodeElement[newSize];

        if (_indexTable is not null)
        {
            Array.Copy(_indexTable!, newTable, Count);
        }

        _indexTable = newTable;
        _indexSize = newSize;
        InvalidateArchive();
        return true;
    }

    private bool IsArchiveSame(TKey id) => _archiveIndex != -1 && _indexTable![_archiveIndex].Id.CompareTo(id) == 0;

    private void SetArchive(int index) => _archiveIndex = index;

    private int SearchForNode(TKey id)
    {
        if (Count == 0)
        {
            return -1;
        }

        if (!_isSorted)
        {
            Array.Sort(_indexTable!, 0, Count);
            InvalidateArchive();
            _isSorted = true;
        }

        var low = 0;
        var high = Count - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            var compare = _indexTable![mid].Id.CompareTo(id);

            if (compare == 0)
            {
                return mid;
            }

            if (compare < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return -1;
    }

    private readonly struct NodeElement(TKey id, TValue data) : IComparable<NodeElement>
    {
        public TValue Data { get; } = data;

        public TKey Id { get; } = id;

        public int CompareTo(NodeElement other) => Id.CompareTo(other.Id);
    }
}
