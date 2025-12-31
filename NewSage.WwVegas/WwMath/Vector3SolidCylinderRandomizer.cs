// -----------------------------------------------------------------------
// <copyright file="Vector3SolidCylinderRandomizer.cs" company="NewSage">
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

public class Vector3SolidCylinderRandomizer : Vector3Randomizer
{
    private float _extent;
    private float _radius;

    public Vector3SolidCylinderRandomizer(float extent, float radius) => (_extent, _radius) = (extent, radius);

    private Vector3SolidCylinderRandomizer(Vector3SolidCylinderRandomizer other) =>
        (_extent, _radius) = (other._extent, other._radius);

    public override Vector3RandomizerClassId ClassId => Vector3RandomizerClassId.SolidCylinder;

    public virtual float Radius => _radius;

    public virtual float Extent => _extent;

    public override Vector3 Vector
    {
        get
        {
            Vector3 result = default;
            result.X = RandomFloatMinus1To1 * Extent;

            Vector2 vec2 = default;
            var radSquared = Radius * Radius;
            do
            {
                vec2.X = RandomFloatMinus1To1 * Radius;
                vec2.Y = RandomFloatMinus1To1 * Radius;
            } while (vec2.Length2 > radSquared);

            result.Y = vec2.X;
            result.Z = vec2.Y;
            return result;
        }
    }

    public override float MaximumExtent => float.Max(Extent, Radius);

    public override void Scale(float scale)
    {
        scale = float.Max(scale, 0F);
        _extent *= scale;
        _radius *= scale;
    }

    public override object Clone() => new Vector3SolidCylinderRandomizer(this);
}
