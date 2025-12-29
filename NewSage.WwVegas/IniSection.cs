// -----------------------------------------------------------------------
// <copyright file="IniSection.cs" company="NewSage">
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

public class IniSection(string? section) : Node<IniSection>
{
    public string? Section { get; set; } = section;

    public int IndexId => (int)Crc.String(Section);

    public List<IniEntry> EntryList { get; } = new();

    public Index<int, IniEntry> EntryIndex { get; } = new();

    public IniEntry? FindEntry(string? entry)
    {
        if (entry is null)
        {
            return null;
        }

        var crc = Crc.String(entry);
        return EntryIndex.IsPresent((int)crc) ? EntryIndex[(int)crc] : null;
    }
}
