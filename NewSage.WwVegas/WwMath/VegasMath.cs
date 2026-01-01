// -----------------------------------------------------------------------
// <copyright file="VegasMath.cs" company="NewSage">
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

public static class VegasMath
{
    public const float Sqrt2 = 1.414213562F;

    private static readonly Lock SyncLock = new();

    private static uint _randNext = 1;

    public static float InvSqrt(float value) => 1F / float.Sqrt(value);

    public static bool IsValid(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    public static int Rand()
    {
        lock (SyncLock)
        {
            // The MSVC LCG: next = next * 214013 + 2531011
            // The result is the bits 16 through 30 (0x7FFF)
            _randNext = (_randNext * 214_013U) + 2_531_011U;
            return (int)((_randNext >> 16) & 0x7FFF);
        }
    }

    public static float RandomFloat() => (Rand() & 0x0FFF) / (float)0x0FFF;

    public static void Seed(uint seed)
    {
        lock (SyncLock)
        {
            _randNext = seed;
        }
    }
}
