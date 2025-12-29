// -----------------------------------------------------------------------
// <copyright file="HashTableIterator.cs" company="NewSage">
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

public class HashTableIterator(HashTable table)
{
    private int _index;
    private Hashable? _nextEntry;

    public bool IsDone => Current is null;

    public Hashable? Current { get; private set; }

    public void First()
    {
        _index = 0;
        _nextEntry = table.Table[_index];
        AdvanceNext();
        Next();
    }

    public void Next()
    {
        Current = _nextEntry;
        if (_nextEntry is not null)
        {
            _nextEntry = _nextEntry.NextHash;
            AdvanceNext();
        }
    }

    private void AdvanceNext()
    {
        while (_nextEntry is null)
        {
            _index++;
            if (_index >= table.HashTableSize)
            {
                return;
            }

            _nextEntry = table.Table[_index];
        }
    }
}
