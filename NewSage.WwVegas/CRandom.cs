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

    public int GetInt() => _generator.Get();

    public int GetInt(int max)
    {
        Debug.Assert(max > 0, $"{nameof(max)} must be positive");
        return (_generator.Get() & 0x7FFF_FFFF) % max;
    }

    public int GetInt(int min, int max)
    {
        if (min > max)
        {
            (min, max) = (max, min);
        }

        return GetInt(max - min) + min;
    }

    public float GetFloat() => GetInt(FloatRange + 1) / (float)FloatRange;

    public float GetFloat(float max) => GetFloat() * max;

    public float GetFloat(float min, float max)
    {
        if (min > max)
        {
            (min, max) = (max, min);
        }

        return (GetFloat() * (max - min)) + min;
    }
}
