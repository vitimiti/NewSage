// -----------------------------------------------------------------------
// <copyright file="MultiListNode.cs" company="NewSage">
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

public sealed class MultiListNode : AutoPool<MultiListNode>, IPoolable
{
    public MultiListNode? Prev { get; set; }

    public MultiListNode? Next { get; set; }

    public MultiListNode? NextList { get; set; }

    [SuppressMessage(
        "Naming",
        "CA1720:Identifiers should not contain type names",
        Justification = "Naming conventions of the engine."
    )]
    public MultiListObject? Object { get; set; }

    public GenericMultiList? List { get; set; }

    public void Reset()
    {
        Prev = null;
        Next = null;
        NextList = null;
        Object = null;
        List = null;
    }
}
