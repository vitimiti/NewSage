// -----------------------------------------------------------------------
// <copyright file="CRandom.cs" company="NewSage">
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

namespace NewSage.WwVegas;

public class CRandom
{
    public const ushort FloatRange = 0x1000;

    private readonly Random2 _generator = new();

    public static CRandom FreeRandom => field ??= new CRandom();

    public int ToInt32() => _generator;

    public int ToInt32(int max)
    {
        Debug.Assert(max > 0, $"{nameof(max)} must be positive");
        return (_generator & 0x7FFF_FFFF) % max;
    }

    public int ToInt32(int min, int max)
    {
        if (min > max)
        {
            (min, max) = (max, min);
        }

        return ToInt32(max - min) + min;
    }

    public float ToSingle() => ToInt32(FloatRange + 1) / (float)FloatRange;

    public float ToSingle(float max) => ToSingle() * max;

    public float ToSingle(float min, float max)
    {
        if (min > max)
        {
            (min, max) = (max, min);
        }

        return (ToSingle() * (max - min)) + min;
    }

    public static implicit operator int(CRandom random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToInt32();
    }

    public static implicit operator float(CRandom random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToSingle();
    }
}
