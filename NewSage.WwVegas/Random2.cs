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

public class Random2 : IRandomGenerator
{
    private readonly int[] _table = new int[250];

    private int _index1;
    private int _index2 = 103;

    public int SignificantBits => 32;

    public Random2(uint seed = 0)
    {
        var primer = new Random3(seed);
        for (int i = 0; i < _table.Length; i++)
        {
            _table[i] = primer.GetNext();
        }
    }

    public int GetNext()
    {
        _table[_index1] ^= _table[_index2];
        var val = _table[_index1];

        if (++_index1 >= 250)
        {
            _index1 = 0;
        }

        if (++_index2 >= 250)
        {
            _index2 = 0;
        }

        return val;
    }

    public int GetNext(int min, int max) => IRandomGenerator.Pick(this, min, max);

    public int ToInt32() => GetNext();

    public static implicit operator int(Random2 random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToInt32();
    }
}
