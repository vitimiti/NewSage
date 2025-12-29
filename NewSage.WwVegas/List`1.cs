// -----------------------------------------------------------------------
// <copyright file="List`1.cs" company="NewSage">
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

using System.Collections;

namespace NewSage.WwVegas;

public class List<T> : GenericList, IEnumerable<T>
    where T : GenericNode
{
    public new T? First => base.First as T;

    public new T? FirstValid => base.FirstValid as T;

    public new T? Last => base.Last as T;

    public new T? LastValid => base.LastValid as T;

    public void Delete()
    {
        while (!IsEmpty)
        {
            First?.Unlink();
        }
    }

    public new IEnumerator<T> GetEnumerator()
    {
        T? current = FirstValid;
        while (current != null)
        {
            yield return current;
            current = current.NextValid as T;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
