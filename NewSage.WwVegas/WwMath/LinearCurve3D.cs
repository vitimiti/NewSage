// -----------------------------------------------------------------------
// <copyright file="LinearCurve3D.cs" company="NewSage">
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
using NewSage.WwVegas.WwSaveLoad;

namespace NewSage.WwVegas.WwMath;

public class LinearCurve3D : Curve3D
{
    private const uint ChunkCurve3D = 0x0002_0653;

    public override PersistFactory Factory => new SimplePersistFactory<LinearCurve3D>(MathIds.ChunkIdLinearCurve3D);

    public override Vector3 Evaluate(float time)
    {
        if (time < Keys[0].Time)
        {
            return Keys[0].Point;
        }

        if (time >= Keys[^1].Time)
        {
            return Keys[^1].Point;
        }

        (var i0, var i1, var t) = FindInterval(time);
        return Keys[i0].Point + (t * (Keys[i1].Point - Keys[i0].Point));
    }

    public override bool Save(ChunkSave cSave)
    {
        ArgumentNullException.ThrowIfNull(cSave);
        _ = cSave.BeginChunk(ChunkCurve3D);
        _ = base.Save(cSave);
        _ = cSave.EndChunk();
        return true;
    }

    public override bool Load(ChunkLoad cLoad)
    {
        ArgumentNullException.ThrowIfNull(cLoad);
        while (cLoad.OpenChunk())
        {
            switch (cLoad.CurrentChunkId)
            {
                case ChunkCurve3D:
                    _ = base.Load(cLoad);
                    break;

                default:
                    Debug.WriteLine($"Unhandled chunk: 0x{cLoad.CurrentChunkId:X8}");
                    break;
            }

            _ = cLoad.CloseChunk();
        }

        return true;
    }
}
