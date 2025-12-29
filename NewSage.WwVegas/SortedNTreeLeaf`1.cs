// -----------------------------------------------------------------------
// <copyright file="SortedNTreeLeaf`1.cs" company="NewSage">
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

public sealed class SortedNTreeLeaf<T> : NTreeLeaf<T>
{
    public string Name { get; set; } = string.Empty;

    public SortedNTreeLeaf<T> AddSorted(T value, string name)
    {
        var newSibling = new SortedNTreeLeaf<T> { Value = value, Name = name };

        SortedNTreeLeaf<T>? start = this;
        while (start.PrevSibling is not null)
        {
            start = (SortedNTreeLeaf<T>)start.PrevSibling;
        }

        InsertionSort(start, newSibling);
        return newSibling;
    }

    private void InsertionSort(SortedNTreeLeaf<T> start, SortedNTreeLeaf<T> newSibling)
    {
        var inserted = false;
        SortedNTreeLeaf<T>? current = start;

        while (current is not null && !inserted)
        {
            if (string.Compare(newSibling.Name, current.Name, StringComparison.OrdinalIgnoreCase) < 0)
            {
                var prev = (SortedNTreeLeaf<T>?)current.PrevSibling;
                newSibling.PrevSibling = prev;
                newSibling.NextSibling = current;
                current.PrevSibling = newSibling;
                _ = prev?.NextSibling = newSibling;

                inserted = true;
            }
            else if (current.NextSibling is null)
            {
                newSibling.PrevSibling = current;
                current.NextSibling = newSibling;
                inserted = true;
            }

            current = (SortedNTreeLeaf<T>?)current.NextSibling;
        }
    }
}
