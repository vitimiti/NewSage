// -----------------------------------------------------------------------
// <copyright file="Vector3SolidSphereRandomizer.cs" company="NewSage">
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

public class Vector3SolidSphereRandomizer : Vector3Randomizer
{
    private float _radius;

    public Vector3SolidSphereRandomizer(float radius) => _radius = float.Max(radius, 0F);

    private Vector3SolidSphereRandomizer(Vector3SolidSphereRandomizer other) => _radius = other._radius;

    public override Vector3RandomizerClassId ClassId => Vector3RandomizerClassId.SolidSphere;

    public virtual float Radius => _radius;

    public override Vector3 Vector
    {
        get
        {
            Vector3 result = default;
            var radSquared = Radius * Radius;
            do
            {
                result.Set(RandomFloatMinus1To1 * Radius, RandomFloatMinus1To1 * Radius, RandomFloatMinus1To1 * Radius);
            } while (result.Length2 > radSquared);

            return result;
        }
    }

    public override float MaximumExtent => Radius;

    public override void Scale(float scale) => _radius *= float.Max(scale, 0F);

    public override object Clone() => new Vector3SolidSphereRandomizer(this);
}
