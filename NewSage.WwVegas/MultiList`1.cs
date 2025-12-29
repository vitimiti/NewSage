// -----------------------------------------------------------------------
// <copyright file="MultiList`1.cs" company="NewSage">
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

public sealed class MultiList<T> : GenericMultiList
    where T : MultiListObject
{
    public T? PeekHead() => SentinelHead.Next?.Object as T;

    public bool Add(T obj, bool onlyOnce = true) => InternalAdd(obj, onlyOnce);

    public bool Remove(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return InternalRemove(obj);
    }

    public T? RemoveHead()
    {
        T? obj = PeekHead();
        if (obj is not null)
        {
            _ = InternalRemove(obj);
        }

        return obj;
    }
}
