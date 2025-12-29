// -----------------------------------------------------------------------
// <copyright file="Random3.cs" company="NewSage">
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

public class Random3(uint seed1 = 0, uint seed2 = 0) : IRandomGenerator
{
    private readonly int _seed = (int)seed1;

    private int _index = (int)seed2;

    private static ReadOnlySpan<int> Mix1 => [unchecked((int)0xbaa96887), 0x1e17d32c, 0x03bcdc3c, 0x0f33d1b2];

    private static ReadOnlySpan<int> Mix2 => [0x4b0f3b58, unchecked((int)0xe874f0c3), 0x6955c5a6, 0x55a7ca46];

    public int SignificantBits => 32;

    public int GetNext()
    {
        var loWord = _seed;
        var hiWord = _index++;
        for (var i = 0; i < 4; i++)
        {
            var hiHold = hiWord;
            var temp = hiHold ^ Mix1[i];
            var itMpl = temp & 0xffff;
            var itMph = temp >> 16;
            temp = (itMpl * itMpl) + ~(itMph * itMph);
            temp = (temp >> 16) | (temp << 16);
            hiWord = loWord ^ ((temp ^ Mix2[i]) + (itMpl * itMph));
            loWord = hiHold;
        }

        return hiWord;
    }

    public int GetNext(int min, int max) => IRandomGenerator.Pick(this, min, max);

    public int ToInt32() => GetNext();

    public static implicit operator int(Random3 random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToInt32();
    }
}
