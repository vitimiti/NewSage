// -----------------------------------------------------------------------
// <copyright file="Vector3Randomizer.cs" company="NewSage">
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

public abstract class Vector3Randomizer : ICloneable
{
    public abstract Vector3RandomizerClassId ClassId { get; }

    public abstract Vector3 Vector { get; }

    public abstract float MaximumExtent { get; }

    protected static Random3 Randomizer => new();

    protected static float OneOverInt32Max => 1F / int.MaxValue;

    protected static float OneOverUInt32Max => 1F / uint.MaxValue;

    protected static float RandomFloatMinus1To1 => Randomizer * OneOverInt32Max;

    protected static float RandomFloat0To1 => Randomizer * OneOverUInt32Max;

    public abstract void Scale(float scale);

    public abstract object Clone();
}
