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

using System.Diagnostics;

namespace NewSage.WwVegas.WwMath;

public static class VegasMath
{
    public const float Sqrt2 = 1.414213562F;

    private const int ArcTableSize = 1_024;
    private const int SinTableSize = 1_024;

    private static readonly Lock SyncLock = new();
    private static readonly float[] FastAcosTable = new float[ArcTableSize];
    private static readonly float[] FastSinTable = new float[SinTableSize];

    private static uint _randNext = 1;

    static VegasMath()
    {
        for (var a = 0; a < ArcTableSize; a++)
        {
            var cv = (a - (ArcTableSize / 2F)) * (1F / (ArcTableSize / 2F));
            FastAcosTable[a] = float.Acos(cv);
        }

        for (var a = 0; a < SinTableSize; a++)
        {
            var cv = a * 2F * float.Pi / SinTableSize;
            FastSinTable[a] = float.Sin(cv);
        }
    }

    public static float FastAcos(float value)
    {
        if (float.Abs(value) > 0.975F)
        {
            return float.Acos(value);
        }

        var localValue = value * ArcTableSize / 2F;

        var idx0 = SingleToInt32Floor(value);
        var idx1 = idx0 + 1;
        var frac = localValue - idx0;

        idx0 += ArcTableSize / 2;
        idx1 += ArcTableSize / 2;

        Debug.Assert(idx0 is >= 0 and < ArcTableSize, "Index out of bounds for acos table");
        Debug.Assert(idx1 is >= 0 and < ArcTableSize, "Index out of bounds for acos table");

        return ((1F - frac) * FastAcosTable[idx0]) + (frac * FastAcosTable[idx1]);
    }

    public static float FastSin(float value)
    {
        var localValue = value * SinTableSize / (2F * float.Pi);

        var idx0 = SingleToInt32Floor(value);
        var idx1 = idx0 + 1;
        var frac = localValue - idx0;

        idx0 &= SinTableSize - 1;
        idx1 &= SinTableSize - 1;

        return ((1F - frac) * FastSinTable[idx0]) + (frac * FastSinTable[idx1]);
    }

    public static int SingleToInt32Floor(float value)
    {
        var a = BitConverter.SingleToInt32Bits(value);
        var sign = a >> 31;
        a &= 0x7FFF_FFFF;

        var exponent = (a >> 23) - 127;
        var expSign = ~(exponent >> 31);
        var iMask = (1 << (31 - exponent)) - 1;
        var mantissa = a & ((1 << 23) - 1);
        var r = unchecked((int)((uint)(mantissa | (1 << 23)) << 8) >> (31 - exponent));

        return ((r & expSign) ^ sign) + (~((mantissa << 8) & iMask) & (expSign ^ ((a - 1) >> 31)) & sign);
    }

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

    public static int FindPot(int value)
    {
        var recPosition = 0;
        var recCount = 0;
        var localValue = value;

        var lp = 0;
        while (localValue != 0)
        {
            if ((localValue & 1) != 0)
            {
                recPosition = lp;
                recCount++;
            }

            localValue >>= 1;
            lp++;
        }

        return recCount < 2 ? 1 << recPosition : 1 << (recPosition + 1);
    }

    public static uint FindPotLog2(uint value)
    {
        var recPosition = 0;
        var recCount = 0;
        var localValue = value;

        var lp = 0;
        while (localValue != 0)
        {
            if ((value & 1) != 0)
            {
                recPosition = lp;
                recCount++;
            }

            localValue >>= 1;
            lp++;
        }

        return (uint)(recCount < 2 ? recPosition : recPosition + 1);
    }
}
