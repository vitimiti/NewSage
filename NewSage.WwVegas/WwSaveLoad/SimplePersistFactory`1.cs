// -----------------------------------------------------------------------
// <copyright file="SimplePersistFactory`1.cs" company="NewSage">
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
using System.Runtime.CompilerServices;

namespace NewSage.WwVegas.WwSaveLoad;

public class SimplePersistFactory<T>(uint chunkId) : PersistFactory
    where T : Persist, new()
{
    public const uint ChunkIdObjPointer = 0x0010_0100;
    public const uint ChunkIdObjData = 0x0010_0101;

    public override uint ChunkId => chunkId;

    public override unsafe Persist Load(ChunkLoad cLoad)
    {
        ArgumentNullException.ThrowIfNull(cLoad);

        T newObj = new();
        nuint oldPtr = 0;

        _ = cLoad.OpenChunk();
        Debug.Assert(
            cLoad.CurrentChunkId == ChunkIdObjPointer,
            $"Expected chunk {ChunkIdObjPointer}, got {cLoad.CurrentChunkId}"
        );

        _ = cLoad.Read(new Span<byte>(&oldPtr, sizeof(nuint)));
        _ = cLoad.CloseChunk();

        // Load the object's actual data via its overridden Load method
        _ = cLoad.OpenChunk();
        Debug.Assert(
            cLoad.CurrentChunkId == ChunkIdObjData,
            $"Expected chunk {ChunkIdObjData}, got {cLoad.CurrentChunkId}"
        );

        _ = newObj.Load(cLoad);
        _ = cLoad.CloseChunk();

        SaveLoadSystem.RegisterPointer((void*)oldPtr, Unsafe.AsPointer(ref newObj));

        return newObj;
    }

    public override unsafe void Save(ChunkSave cSave, Persist persist)
    {
        ArgumentNullException.ThrowIfNull(cSave);
        ArgumentNullException.ThrowIfNull(persist);

        if (persist is not T obj)
        {
            throw new ArgumentException($"Object provided to factory is not of type {typeof(T).Name}");
        }

        var objPtr = (nuint)Unsafe.AsPointer(ref obj);

        _ = cSave.BeginChunk(ChunkIdObjPointer);
        _ = cSave.Write(new ReadOnlySpan<byte>(&objPtr, sizeof(nuint)));
        _ = cSave.EndChunk();

        _ = cSave.BeginChunk(ChunkIdObjData);
        _ = obj.Save(cSave);
        _ = cSave.EndChunk();
    }
}
