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
using System.Runtime.InteropServices;

namespace NewSage.WwVegas.WwSaveLoad;

public class SimplePersistFactory<T>(uint chunkId) : PersistFactory
    where T : struct
{
    public const uint SimpleFactoryChunkIdObjPointer = 0x0010_0100;
    public const uint SimpleFactoryChunkIdObjData = SimpleFactoryChunkIdObjPointer + 1;

    public override uint ChunkId => chunkId;

    public override unsafe Persist Load(ChunkLoad cLoad)
    {
        ArgumentNullException.ThrowIfNull(cLoad);

        nuint oldPtr = 0;
        T data = default;

        _ = cLoad.OpenChunk();
        Debug.Assert(
            cLoad.CurrentChunkId == SimpleFactoryChunkIdObjPointer,
            $"Expected chunk {SimpleFactoryChunkIdObjPointer}, got {cLoad.CurrentChunkId}"
        );

        _ = cLoad.Read(new Span<byte>(&oldPtr, sizeof(nuint)));
        _ = cLoad.CloseChunk();

        _ = cLoad.OpenChunk();
        Debug.Assert(
            cLoad.CurrentChunkId == SimpleFactoryChunkIdObjData,
            $"Expected chunk {SimpleFactoryChunkIdObjData}, got {cLoad.CurrentChunkId}"
        );

        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        if (cLoad.Read(buffer) == (uint)buffer.Length)
        {
            data = MemoryMarshal.Read<T>(buffer);
        }

        _ = cLoad.CloseChunk();

        var container = new PersistStructContainer<T>(this, data);
        SaveLoadSystem.RegisterPointer((void*)oldPtr, Unsafe.AsPointer(ref container));

        return container;
    }

    public override unsafe void Save(ChunkSave cSave, Persist persist)
    {
        ArgumentNullException.ThrowIfNull(cSave);
        ArgumentNullException.ThrowIfNull(persist);

        if (persist is not PersistStructContainer<T> container)
        {
            throw new ArgumentException($"Persist object is not a container for {typeof(T).Name}");
        }

        var objId = (nuint)Unsafe.AsPointer(ref container);

        _ = cSave.BeginChunk(SimpleFactoryChunkIdObjPointer);
        _ = cSave.Write(new ReadOnlySpan<byte>(&objId, sizeof(nuint)));
        _ = cSave.EndChunk();

        _ = cSave.BeginChunk(SimpleFactoryChunkIdObjData);

        T data = container.Data;
        ReadOnlySpan<byte> buffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref data, 1));
        _ = cSave.Write(buffer);

        _ = cSave.EndChunk();
    }
}
