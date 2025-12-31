// -----------------------------------------------------------------------
// <copyright file="HashTemplateIterator`2.cs" company="NewSage">
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

[SuppressMessage(
    "Performance",
    "CA1815:Override equals and operator equals on value types",
    Justification = "Not used in this type."
)]
public struct HashTemplateIterator<TKey, TValue>
    where TKey : IConvertibleToUInt32
{
    private const int Nil = -1;

    private readonly HashTemplate<TKey, TValue> _hashTable;

    private int _hashIndex;
    private int _handle;

    public HashTemplateIterator(HashTemplate<TKey, TValue> hashTable)
    {
        _hashTable = hashTable;
        _hashIndex = 0;
        _handle = Nil;
        First();
    }

    public readonly bool IsDone => _hashIndex == (int)_hashTable.Size;

    public readonly TValue? PeekValue() => _hashTable.InternalTable![_handle].Value;

    public readonly TKey? PeekKey() => _hashTable.InternalTable![_handle].Key;

    public void First()
    {
        _handle = Nil;
        var hash = _hashTable.InternalHash;
        if (hash is null)
        {
            _hashIndex = (int)_hashTable.Size;
            return;
        }

        for (_hashIndex = 0; _hashIndex < (int)_hashTable.Size; ++_hashIndex)
        {
            _handle = hash[_hashIndex];
            if (_handle != Nil)
            {
                break;
            }
        }
    }

    public void Next()
    {
        HashTemplate<TKey, TValue>.Entry[]? table = _hashTable.InternalTable;
        var hash = _hashTable.InternalHash;

        _handle = table![_handle].Next;
        if (_handle != Nil)
        {
            return;
        }

        for (++_hashIndex; _hashIndex < (int)_hashTable.Size; ++_hashIndex)
        {
            _handle = hash![_hashIndex];
            if (_handle != Nil)
            {
                break;
            }
        }
    }
}
