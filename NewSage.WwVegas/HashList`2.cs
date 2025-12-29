// -----------------------------------------------------------------------
// <copyright file="HashList`2.cs" company="NewSage">
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

public class HashList<T, TUser> : IDisposable
    where TUser : class, new()
{
    private const int LargePrimeNumber = 257;

    private readonly HashNode<T, TUser>?[] _hashTable;
    private readonly List<HashNode<T, TUser>> _list = new();
    private readonly int _numHashValues;

    private int _numRecords;
    private int _usedValues;
    private bool _disposed;

    public HashList(int numHashValues = LargePrimeNumber)
    {
        _numHashValues = numHashValues;
        _hashTable = new HashNode<T, TUser>?[numHashValues];
    }

    public int NumRecords => _numRecords;

    public int NumUsedValues => _usedValues;

    public HashNode<T, TUser>? First => _list.First;

    public HashNode<T, TUser> Add(T record, uint key)
    {
        var node = new HashNode<T, TUser>(record, key) { ListCreated = true };

        return Add(node);
    }

    public HashNode<T, TUser> Add(HashNode<T, TUser> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        Debug.Assert(!node.IsInList, "Cannot add a node that is already in a list.");
        var hashIdx = node.Key % (uint)_numHashValues;

        node.InList = true;
        node.SetNewInList(true);
        HashNode<T, TUser>? first = _hashTable[hashIdx];

        if (first is not null)
        {
            first.Link(node);
            if (first.LastInTable)
            {
                first.LastInTable = false;
                node.LastInTable = true;
            }
        }
        else
        {
            _list.AddHead(node);
            node.FirstInTable = true;
            node.LastInTable = true;
            _hashTable[hashIdx] = node;
            _usedValues++;
        }

        _numRecords++;
        return node;
    }

    public HashNode<T, TUser>? Find(uint key)
    {
        var hashIdx = key % (uint)_numHashValues;
        HashNode<T, TUser>? cur = _hashTable[hashIdx];

        while (cur is not null)
        {
            if (cur.Key == key)
            {
                return cur;
            }

            if (cur.LastInTable)
            {
                break;
            }

            cur = cur.Next;
        }

        return null;
    }

    public void Remove(HashNode<T, TUser> node)
    {
        ArgumentNullException.ThrowIfNull(node);
        Debug.Assert(node.IsInList, "Cannot remove a node that is not in a list.");

        if (node.FirstInTable)
        {
            var hashIdx = node.Key % (uint)_numHashValues;
            node.FirstInTable = false;

            if (node.LastInTable)
            {
                node.LastInTable = false;
                _hashTable[hashIdx] = null;
                _usedValues--;
            }
            else
            {
                _hashTable[hashIdx] = node.NextValid;
                if (_hashTable[hashIdx] is not null)
                {
                    _hashTable[hashIdx]!.FirstInTable = true;
                }
            }
        }
        else if (node.LastInTable)
        {
            node.LastInTable = false;
            _ = node.Previous?.LastInTable = true;
        }

        node.InList = false;
        node.SetNewInList(false);
        if (!node.ListCreated)
        {
            node.Unlink();
        }

        _numRecords--;
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
            while (!_list.IsEmpty)
            {
                if (_list.First is { } node)
                {
                    Remove(node);
                }
            }

            _list.Dispose();
        }

        _disposed = true;
    }
}
