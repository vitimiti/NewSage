// -----------------------------------------------------------------------
// <copyright file="Random2.cs" company="NewSage">
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

public class Random2 : IRandomNumberGenerator
{
    public Random2(int seed = 0)
    {
        Index1 = 0;
        Index2 = 103;

        var random = new Random3(seed);

        for (var i = 0; i < Table.Count; i++)
        {
            Table[i] = random;
        }
    }

    public int SignificantBits => 32;

    protected int Index1 { get; set; }

    protected int Index2 { get; set; }

    protected IList<int> Table { get; } = new int[250];

    public int ToInt32()
    {
        Table[Index1] ^= Table[Index2];
        var value = Table[Index1];

        Index1++;
        Index2++;
        if (Index1 >= Table.Count)
        {
            Index1 = 0;
        }

        if (Index2 >= Table.Count)
        {
            Index2 = 0;
        }

        return value;
    }

    public int ToInt32(int min, int max) => RandomNumber<Random2>.Pick(this, min, max);

    public static implicit operator int(Random2 random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToInt32();
    }
}
