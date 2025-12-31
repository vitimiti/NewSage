// -----------------------------------------------------------------------
// <copyright file="Curve3D.cs" company="NewSage">
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
using System.Runtime.InteropServices;
using NewSage.WwVegas.WwSaveLoad;

namespace NewSage.WwVegas.WwMath;

public abstract class Curve3D : Persist
{
    private const uint ChunkVariables = 0x0002_0651;
    private const uint ChunkKeys = ChunkVariables + 1;
    private const byte VariableIsLooping = 0x00;
    private const byte VariableKeyCount = VariableIsLooping + 1;

    public bool IsLooping { get; set; }

    public virtual int KeyCount => Keys.Count;

    public float StartTime => Keys.Count > 0 ? Keys[0].Time : 0F;

    public float EndTime => Keys.Count > 0 ? Keys[^1].Time : 0F;

    protected IList<Curve3DKey> Keys { get; } = [];

    public abstract Vector3 Evaluate(float time);

    public virtual Curve3DKey GetKey(int index) => Keys[index];

    public virtual void SetKey(int index, Curve3DKey key) => Keys[index] = key;

    public virtual int AddKey(Curve3DKey key)
    {
        var idx = 0;
        while (idx < Keys.Count && Keys[idx].Time < key.Time)
        {
            idx++;
        }

        Keys.Insert(idx, key);
        return idx;
    }

    public virtual void RemoveKey(int index) => Keys.RemoveAt(index);

    public virtual void ClearKeys() => Keys.Clear();

    public override bool Save(ChunkSave cSave)
    {
        ArgumentNullException.ThrowIfNull(cSave);

        var keyCount = Keys.Count;
        _ = cSave.BeginChunk(ChunkVariables);
        _ = cSave.BeginMicroChunk(VariableIsLooping);
        _ = cSave.Write(BitConverter.GetBytes(IsLooping));
        _ = cSave.EndMicroChunk();
        _ = cSave.BeginMicroChunk(VariableKeyCount);
        _ = cSave.Write(BitConverter.GetBytes(keyCount));
        _ = cSave.EndMicroChunk();
        _ = cSave.EndChunk();

        _ = cSave.BeginChunk(ChunkKeys);
        foreach (Curve3DKey key in Keys)
        {
            Span<byte> pointBytes = new byte[Marshal.SizeOf<Vector3>()];
            MemoryMarshal.Write(pointBytes, key.Point);
            _ = cSave.Write(pointBytes);
            _ = cSave.Write(BitConverter.GetBytes(key.Time));
        }

        _ = cSave.EndChunk();
        return true;
    }

    public override bool Load(ChunkLoad cLoad)
    {
        ArgumentNullException.ThrowIfNull(cLoad);

        Span<byte> isLoopingBytes = stackalloc byte[sizeof(bool)];
        Span<byte> keyCountBytes = stackalloc byte[sizeof(int)];
        Span<byte> pointBytes = stackalloc byte[Marshal.SizeOf<Vector3>()];
        Span<byte> timeBytes = stackalloc byte[sizeof(float)];

        var keyCount = 0;
        Curve3DKey newKey = default;

        Keys.Clear();
        while (cLoad.OpenChunk())
        {
            switch (cLoad.CurrentChunkId)
            {
                case ChunkVariables:
                    while (cLoad.OpenMicroChunk())
                    {
                        switch (cLoad.CurrentMicroChunkId)
                        {
                            case VariableIsLooping:
                                _ = cLoad.Read(isLoopingBytes);
                                IsLooping = BitConverter.ToBoolean(isLoopingBytes);
                                break;
                            case VariableKeyCount:
                                _ = cLoad.Read(keyCountBytes);
                                keyCount = BitConverter.ToInt32(keyCountBytes);
                                break;
                            default:
                                break;
                        }

                        _ = cLoad.CloseMicroChunk();
                    }

                    break;

                case ChunkKeys:
                    for (var i = 0; i < keyCount; i++)
                    {
                        _ = cLoad.Read(pointBytes);
                        newKey.Point = MemoryMarshal.Read<Vector3>(pointBytes);

                        _ = cLoad.Read(timeBytes);
                        newKey.Time = BitConverter.ToSingle(timeBytes);

                        Keys.Add(newKey);
                    }

                    break;

                default:
                    Debug.WriteLine($"Unhandled chunk: 0x{cLoad.CurrentChunkId:X8}");
                    break;
            }

            _ = cLoad.CloseChunk();
        }

        return true;
    }

    protected (int I0, int I1, float T) FindInterval(float time)
    {
        Debug.Assert(time >= Keys[0].Time, $"Time {time} is before first key time {Keys[0].Time}");
        Debug.Assert(time <= Keys[^1].Time, $"Time {time} is after last key time {Keys[^1].Time}");

        var i = 0;
        while (time > Keys[i + 1].Time)
        {
            i++;
        }

        return (i, i + 1, (time - Keys[i].Time) / (Keys[i + 1].Time - Keys[i].Time));
    }
}
