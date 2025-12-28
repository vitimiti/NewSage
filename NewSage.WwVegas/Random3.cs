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

public class Random3 : IRandomNumberGenerator
{
    public Random3(int seed1 = 0, int seed2 = 0)
    {
        Seed = seed1;
        Index = seed2;
    }

    public int SignificantBits => 32;

    // csharpier-ignore
    protected static IList<int> Mix1 { get; } =
    [
        unchecked((int)0xBAA9_6887), 0x1E17_D32C, 0x03BC_DC3C, 0x0F33_D1B2, 0x76A6_491D, unchecked((int)0xC570_D85D),
        unchecked((int)0xE382_B1E3), 0x78DB_4362, 0x7439_A9D4, unchecked((int)0x9CEA_8AC5), unchecked((int)0x8953_7C5C),
        0x2588_F55D, 0x415B_5E1D, 0x216E_3D95, unchecked((int)0x85C6_62E7), 0x5E8A_B368, 0x3EA5_CC8C,
        unchecked((int)0xD26A_0F74), unchecked((int)0xF3A9_222B), 0x48AA_D7E4
    ];

    // csharpier-ignore
    protected static IList<int> Mix2 { get; } =
    [
        0x4B0F_3B58, unchecked((int)0xE874_F0C3), 0x6955_C5A6, 0x55A7_CA46, 0x4D9A_9D86, unchecked((int)0xFE28_A195),
        unchecked((int)0xB1CA_7865), 0x6B23_5751, unchecked((int)0x9A99_7A61), unchecked((int)0xAA6E_95C8),
        unchecked((int)0xAAA9_8EE1), 0x5AF9_154C, unchecked((int)0xFC8E_2263), 0x390F_5E8C, 0x58FF_D802,
        unchecked((int)0xAC0A_5EBA), unchecked((int)0xAC48_74F6), unchecked((int)0xA9DF_0913),
        unchecked((int)0x86BE_4C74), unchecked((int)0xED2C_123B)
    ];

    protected int Seed { get; set; }

    protected int Index { get; set; }

    public int ToInt32()
    {
        var lowWord = Seed;
        var hiWord = Index++;
        for (var i = 0; i < 4; i++)
        {
            var hiHold = hiWord;
            var temp = hiHold ^ Mix1[i];
            var itMpl = temp & 0xFFFF;
            var itMph = temp >> 16;
            temp = (itMpl * itMpl) + ~(itMph * itMph);
            temp = (temp >> 16) | (temp << 16);
            hiWord = lowWord ^ ((temp ^ Mix2[i]) + (itMpl * itMph));
            lowWord = hiHold;
        }

        return hiWord;
    }

    public int ToInt32(int min, int max) => RandomNumber<Random3>.Pick(this, min, max);

    public static implicit operator int(Random3 random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToInt32();
    }
}
