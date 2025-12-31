// -----------------------------------------------------------------------
// <copyright file="SaveLoadSystem.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwSaveLoad;

public static class SaveLoadSystem
{
    private static readonly System.Collections.Generic.List<PostLoadable> PostLoadList = [];
    private static readonly PointerRemapper PointerRemapper = new();

    private static SaveLoadSubSystem? _subSystemListHead;
    private static PersistFactory? _factoryListHead;

    public static unsafe void RegisterPointer(void* oldPointer, void* newPointer) =>
        PointerRemapper.RegisterPointer(oldPointer, newPointer);

    public static unsafe void RequestPointerRemap(void** pointerToConvert, string? file = null, int line = 0) =>
        PointerRemapper.RequestPointerRemap(pointerToConvert, file, line);

    public static bool Save(ChunkSave cSave, SaveLoadSubSystem subSystem)
    {
        ArgumentNullException.ThrowIfNull(cSave);
        ArgumentNullException.ThrowIfNull(subSystem);

        if (!subSystem.ContainsData)
        {
            return true;
        }

        _ = cSave.BeginChunk(subSystem.ChunkId);
        var ok = subSystem.Save(cSave);
        _ = cSave.EndChunk();

        return ok;
    }

    public static bool Load(ChunkLoad cLoad, bool autoPostLoad = true)
    {
        ArgumentNullException.ThrowIfNull(cLoad);

        PointerRemapper.Reset();
        var ok = true;

        while (cLoad.OpenChunk())
        {
            SaveLoadStatus.IncStatusCount();
            SaveLoadSubSystem? sys = FindSubSystem(cLoad.CurrentChunkId);
            if (sys is not null)
            {
                SaveLoadStatus.SetStatusText(sys.Name, 1);
                ok &= sys.Load(cLoad);
            }

            _ = cLoad.CloseChunk();
        }

        PointerRemapper.Process();
        PointerRemapper.Reset();

        if (autoPostLoad)
        {
            _ = PostLoadProcessing();
        }

        return ok;
    }

    public static bool PostLoadProcessing(Action? networkCallback = null)
    {
        while (PostLoadList.Count > 0)
        {
            PostLoadable obj = PostLoadList[0];
            PostLoadList.RemoveAt(0);

            networkCallback?.Invoke();

            obj.OnPostLoad();
            obj.IsPostLoadRegistered = false;
        }

        return true;
    }

    public static void RegisterSubSystem(SaveLoadSubSystem sys)
    {
        ArgumentNullException.ThrowIfNull(sys);

        sys.NextSubSystem = _subSystemListHead;
        _subSystemListHead = sys;
    }

    public static PersistFactory? FindPersistFactory(uint chunkId)
    {
        PersistFactory? factory = _factoryListHead;
        while (factory is not null)
        {
            if (factory.ChunkId == chunkId)
            {
                return factory;
            }

            factory = factory.NextFactory;
        }

        return null;
    }

    public static void RegisterPostLoadCallback(PostLoadable obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.IsPostLoadRegistered)
        {
            return;
        }

        obj.IsPostLoadRegistered = true;
        PostLoadList.Add(obj);
    }

    internal static void RegisterPersistFactory(PersistFactory factory)
    {
        factory.NextFactory = _factoryListHead;
        _factoryListHead = factory;
    }

    private static SaveLoadSubSystem? FindSubSystem(uint chunkId)
    {
        SaveLoadSubSystem? sys = _subSystemListHead;
        while (sys is not null)
        {
            if (sys.ChunkId == chunkId)
            {
                return sys;
            }

            sys = sys.NextSubSystem;
        }

        return null;
    }
}
