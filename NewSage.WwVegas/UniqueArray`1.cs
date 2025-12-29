// -----------------------------------------------------------------------
// <copyright file="UniqueArray`1.cs" company="NewSage">
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

public sealed class UniqueArray<T> : IHashser
{
    private const int NoItem = -1;

    private readonly IHashCalculator<T> _hashCalculator;
    private readonly int[] _hashTable;
    private readonly int _growthRate;

    private HashItem[] _uniqueItems;

    public UniqueArray(int initialSize, int growthRate, IHashCalculator<T> hashCalculator)
    {
        _hashCalculator = hashCalculator;
        _growthRate = growthRate;
        _uniqueItems = new HashItem[initialSize];

        var tableSize = 1 << _hashCalculator.HashBitsCount;
        _hashTable = new int[tableSize];

        Reset();
    }

    public int Add(T item)
    {
        _hashCalculator.ComputeHash(item);
        var hash = _hashCalculator.GetHashValue(0);

        var entryIndex = _hashTable[hash];
        while (entryIndex != NoItem)
        {
            if (_hashCalculator.ItemsMatch(item, _uniqueItems[entryIndex].Item))
            {
                return entryIndex;
            }

            entryIndex = _uniqueItems[entryIndex].NextHashIndex;
        }

        if (Count == _uniqueItems.Length)
        {
            Array.Resize(ref _uniqueItems, _uniqueItems.Length + _growthRate);
        }

        var newIndex = Count++;
        _uniqueItems[newIndex] = new HashItem { Item = item, NextHashIndex = _hashTable[hash] };

        _hashTable[hash] = newIndex;

        return newIndex;
    }

    public void Reset()
    {
        Count = 0;
        Array.Fill(_hashTable, NoItem);
    }

    public int Count { get; private set; }

    public T this[int index] => _uniqueItems[index].Item;

    private struct HashItem
    {
        public T Item;
        public int NextHashIndex;
    }
}
