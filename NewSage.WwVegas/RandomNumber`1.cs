// -----------------------------------------------------------------------
// <copyright file="RandomNumber`1.cs" company="NewSage">
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

namespace NewSage.WwVegas;

public static class RandomNumber<T>
    where T : IRandomNumberGenerator
{
    public static int Pick(T generator, int min, int max)
    {
        if (min == max)
        {
            return min;
        }

        if (min > max)
        {
            (min, max) = (max, min);
        }

        var magnitude = max - min;
        var highBit = generator.SignificantBits - 1;
        while ((magnitude & (1 << highBit)) == 0 && highBit > 0)
        {
            highBit--;
        }

        var mask = ~(~0 << (highBit + 1));

        var pick = magnitude + 1;
        while (pick > magnitude)
        {
            pick = generator.ToInt32() & mask;
        }

        return pick + min;
    }
}
