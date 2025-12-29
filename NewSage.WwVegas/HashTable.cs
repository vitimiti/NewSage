// -----------------------------------------------------------------------
// <copyright file="HashTable.cs" company="NewSage">
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

public class HashTable
{
    public HashTable(int size)
    {
        HashTableSize = size;

        Debug.Assert(
            (HashTableSize & (HashTableSize - 1)) == 0,
            $"Hash table size must be a power of 2, was {HashTableSize}"
        );

        Table = new Hashable?[HashTableSize];
        Reset();
    }

    internal int HashTableSize { get; }

    internal Hashable?[] Table { get; }

    public void Reset()
    {
        for (var i = 0; i < HashTableSize; i++)
        {
            Table[i] = null;
        }
    }

    public void Add(Hashable entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var index = Hash(entry.Key);

        Debug.Assert(entry.NextHash is null, "Hash table entries should not have a next hash.");

        entry.NextHash = Table[index];
        Table[index] = entry;
    }

    public bool Remove(Hashable entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var key = entry.Key;
        var index = Hash(key);
        if (Table[index] is null)
        {
            return false;
        }

        if (ReferenceEquals(Table[index], entry))
        {
            Table[index] = entry.NextHash;
            return true;
        }

        Hashable? node = Table[index];
        while (node?.NextHash is not null)
        {
            if (ReferenceEquals(node.NextHash, entry))
            {
                node.NextHash = entry.NextHash;
                return true;
            }

            node = node.NextHash;
        }

        return false;
    }

    public Hashable? Find(string key)
    {
        var index = Hash(key);
        for (Hashable? node = Table[index]; node is not null; node = node.NextHash)
        {
            if (node.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return node;
            }
        }

        return null;
    }

    private int Hash(string key) => (int)(RealCrc.StringIgnoreCase(key) & (ulong)(HashTableSize - 1));
}
