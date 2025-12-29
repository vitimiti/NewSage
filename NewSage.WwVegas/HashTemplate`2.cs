// -----------------------------------------------------------------------
// <copyright file="HashTemplate`2.cs" company="NewSage">
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

public sealed class HashTemplate<TKey, TValue> : IDisposable
    where TKey : IConvertibleToUInt32
{
    private const int Nil = -1;
    private readonly HashTemplateKey<TKey> _keyHasher = new();
    private int _first = Nil;
    private bool _disposed;

    public uint Size { get; private set; }

    internal int[]? InternalHash { get; private set; }

    internal Entry[]? InternalTable { get; private set; }

    public void Insert(TKey key, TValue value)
    {
        var hash = AllocateEntry();
        var hashValue = GetHashValue(key, Size);

        InternalTable![hash].Key = key;
        InternalTable[hash].Value = value;
        InternalTable[hash].Next = InternalHash![(int)hashValue];
        InternalHash[(int)hashValue] = hash;
    }

    public void SetValue(TKey key, TValue value)
    {
        if (InternalHash != null)
        {
            var hash = InternalHash[(int)GetHashValue(key, Size)];
            while (hash != Nil)
            {
                if (EqualityComparer<TKey>.Default.Equals(InternalTable![hash].Key, key))
                {
                    InternalTable[hash].Value = value;
                    return;
                }

                hash = InternalTable[hash].Next;
            }
        }

        Insert(key, value);
    }

    public void Remove(TKey key)
    {
        if (InternalHash == null)
        {
            return;
        }

        var hashValue = GetHashValue(key, Size);
        var prev = Nil;
        var hash = InternalHash[(int)hashValue];

        while (hash != Nil)
        {
            if (EqualityComparer<TKey>.Default.Equals(InternalTable![hash].Key, key))
            {
                if (prev != Nil)
                {
                    InternalTable![prev].Next = InternalTable[hash].Next;
                }
                else
                {
                    InternalHash[(int)hashValue] = InternalTable![hash].Next;
                }

                InternalTable[hash].Next = _first;
                _first = hash;
                return;
            }

            prev = hash;
            hash = InternalTable![hash].Next;
        }
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        value = default;
        if (InternalHash == null)
        {
            return false;
        }

        var hash = InternalHash[(int)GetHashValue(key, Size)];
        while (hash != Nil)
        {
            if (EqualityComparer<TKey>.Default.Equals(InternalTable![hash].Key, key))
            {
                value = InternalTable[hash].Value;
                return true;
            }

            hash = InternalTable[hash].Next;
        }

        return false;
    }

    public bool Exists(TKey key)
    {
        if (InternalHash == null)
        {
            return false;
        }

        var h = InternalHash[(int)GetHashValue(key, Size)];
        while (h != Nil)
        {
            if (EqualityComparer<TKey>.Default.Equals(InternalTable![h].Key, key))
            {
                return true;
            }

            h = InternalTable[h].Next;
        }

        return false;
    }

    public void RemoveAll()
    {
        if (InternalHash == null)
        {
            return;
        }

        for (var i = 0; i < Size; i++)
        {
            var firstHash = InternalHash[i];
            if (firstHash == Nil)
            {
                continue;
            }

            var hash = firstHash;
            while (InternalTable![hash].Next != Nil)
            {
                hash = InternalTable[hash].Next;
            }

            InternalTable[hash].Next = _first;
            _first = firstHash;
            InternalHash[i] = Nil;
        }
    }

    private uint GetHashValue(TKey key, uint hashArraySize) => _keyHasher.GetHashValue(key) & (hashArraySize - 1);

    private void ReHash()
    {
        var newSize = Size * 2;
        if (newSize < 4)
        {
            newSize = 4;
        }

        var newTable = new Entry[newSize];
        var newHash = new int[newSize];
        Array.Fill(newHash, Nil);

        var count = 0;
        if (Size > 0)
        {
            for (var i = 0; i < (int)Size; i++)
            {
                var hash = InternalHash![i];
                while (hash != Nil)
                {
                    var hashValue = GetHashValue(InternalTable![hash].Key!, newSize);
                    newTable[count].Key = InternalTable[hash].Key;
                    newTable[count].Value = InternalTable[hash].Value;
                    newTable[count].Next = newHash[(int)hashValue];
                    newHash[(int)hashValue] = count;
                    count++;
                    hash = InternalTable[hash].Next;
                }
            }
        }

        for (var i = count; i < (int)newSize; i++)
        {
            newTable[i].Next = i + 1;
        }

        newTable[(int)newSize - 1].Next = Nil;

        _first = count;
        InternalHash = newHash;
        InternalTable = newTable;
        Size = newSize;
    }

    private int AllocateEntry()
    {
        if (_first == Nil)
        {
            ReHash();
        }

        var hash = _first;
        _first = InternalTable![_first].Next;
        return hash;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        InternalHash = null;
        InternalTable = null;

        _disposed = true;
    }

    internal struct Entry
    {
        public int Next;
        public TKey? Key;
        public TValue? Value;
    }
}
