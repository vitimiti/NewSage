// -----------------------------------------------------------------------
// <copyright file="Vector3SolidBoxRandomizer.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwMath;

public class Vector3SolidBoxRandomizer : Vector3Randomizer
{
    private Vector3 _extents;

    public Vector3SolidBoxRandomizer(Vector3 extents) =>
        _extents.Set(float.Max(extents.X, 0F), float.Max(extents.Y, 0F), float.Max(extents.Z, 0F));

    private Vector3SolidBoxRandomizer(Vector3SolidBoxRandomizer other) => _extents = other._extents;

    public override Vector3RandomizerClassId ClassId => Vector3RandomizerClassId.SolidBox;

    public virtual Vector3 Extents => _extents;

    public override Vector3 Vector =>
        new(RandomFloatMinus1To1 * Extents.X, RandomFloatMinus1To1 * Extents.Y, RandomFloatMinus1To1 * Extents.Z);

    public override float MaximumExtent => float.Max(float.Max(Extents.X, Extents.Y), Extents.Z);

    public override void Scale(float scale)
    {
        scale = float.Max(scale, 0F);
        _extents.Set(Extents.X * scale, Extents.Y * scale, Extents.Z * scale);
    }

    public override object Clone() => new Vector3SolidBoxRandomizer(this);
}
