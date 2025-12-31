// -----------------------------------------------------------------------
// <copyright file="PointerRemapper.cs" company="NewSage">
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

using System.Diagnostics;

namespace NewSage.WwVegas.WwSaveLoad;

public sealed unsafe class PointerRemapper
{
    private readonly System.Collections.Generic.List<PointerRemapEntry> _mappings = [];
    private readonly System.Collections.Generic.List<PointerRemapRequest> _requests = [];

    public void Reset()
    {
        _mappings.Clear();
        _requests.Clear();
    }

    public void RegisterPointer(void* oldPointer, void* newPointer) =>
        _mappings.Add(new PointerRemapEntry { OldAddress = (nuint)oldPointer, NewAddress = (nuint)newPointer });

    public void RequestPointerRemap(void** pointerToConvert, string? file = null, int line = 0)
    {
        var request = new PointerRemapRequest { PointerToConvert = (nuint*)pointerToConvert };

#if DEBUG
        request.File = file;
        request.Line = line;
#endif

        _requests.Add(request);
    }

    public void Process()
    {
        foreach (PointerRemapRequest request in _requests)
        {
            var oldAddr = *request.PointerToConvert;
            if (oldAddr == 0)
            {
                continue;
            }

            PointerRemapEntry mapping = _mappings.FirstOrDefault(m => m.OldAddress == oldAddr);
            if (mapping.OldAddress != 0)
            {
                *request.PointerToConvert = mapping.NewAddress;
            }
            else
            {
                Debug.WriteLine($"Pointer remap failed for address 0x{oldAddr:X} at {request.File}:{request.Line}");
                *request.PointerToConvert = 0;
            }
        }
    }
}
