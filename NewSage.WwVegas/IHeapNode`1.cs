// -----------------------------------------------------------------------
// <copyright file="IHeapNode`1.cs" company="NewSage">
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

public interface IHeapNode<TKey> : IComparable<IHeapNode<TKey>>
    where TKey : IComparable<TKey>
{
    int HeapLocation { get; set; }

    TKey HeapKey { get; }

    new int CompareTo(IHeapNode<TKey>? other) => other is null ? 1 : HeapKey.CompareTo(other.HeapKey);

    [SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "This would result in a multiconditional ternary operator. Difficult to read."
    )]
    static bool operator <(IHeapNode<TKey>? x, IHeapNode<TKey>? y)
    {
        if (x is null)
        {
            return true;
        }

        return x.CompareTo(y) < 0;
    }

    [SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "This would result in a multiconditional ternary operator. Difficult to read."
    )]
    static bool operator <=(IHeapNode<TKey>? x, IHeapNode<TKey>? y)
    {
        if (x is null)
        {
            return true;
        }

        return x.CompareTo(y) <= 0;
    }

    [SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "This would result in a multiconditional ternary operator. Difficult to read."
    )]
    static bool operator >(IHeapNode<TKey>? x, IHeapNode<TKey>? y)
    {
        if (x is null)
        {
            return true;
        }

        return x.CompareTo(y) > 0;
    }

    [SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "This would result in a multiconditional ternary operator. Difficult to read."
    )]
    static bool operator >=(IHeapNode<TKey>? x, IHeapNode<TKey>? y)
    {
        if (x is null)
        {
            return true;
        }

        return x.CompareTo(y) >= 0;
    }
}
