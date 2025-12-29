// -----------------------------------------------------------------------
// <copyright file="IRandomGenerator.cs" company="NewSage">
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

public interface IRandomGenerator
{
    static int Pick(IRandomGenerator generator, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(generator);

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

        var mask = ~((~0) << (highBit + 1));
        var pick = magnitude + 1;
        while (pick > magnitude)
        {
            pick = generator.GetNext() & mask;
        }

        return pick + min;
    }

    int SignificantBits { get; }

    int GetNext();

    int GetNext(int min, int max);

    int ToInt32() => GetNext();
}
