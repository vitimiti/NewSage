// -----------------------------------------------------------------------
// <copyright file="Random4.cs" company="NewSage">
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

public class Random4 : IRandomNumberGenerator
{
    private const int N = 624;
    private const int M = 397;
    private const uint MatrixA = 0x9908_B0DF;
    private const uint UpperMask = 0x8000_0000;
    private const uint LowerMask = 0x7FFF_FFFF;
    private const uint TemperingMaskB = 0x9D2C_5680;
    private const uint TemperingMaskC = 0xEFC6_0000;

    private static readonly uint[] Mag01 = { 0x0000_0000, MatrixA };

    public Random4(uint seed = 4357)
    {
        if (seed == 0)
        {
            seed = 4375;
        }

        Mt[0] = seed & 0xFFFF_FFFF;
        for (MtIndex = 1; MtIndex < N; MtIndex++)
        {
            Mt[MtIndex] = (69_069 * Mt[MtIndex - 1]) & 0xFFFF_FFFF;
        }
    }

    public int SignificantBits => 32;

    protected IList<uint> Mt { get; } = new uint[624];

    protected int MtIndex { get; set; }

    public int ToInt32()
    {
        uint y;
        if (MtIndex >= N)
        {
            int kk;
            for (kk = 0; kk < N - M; kk++)
            {
                y = (Mt[kk] & UpperMask) | (Mt[kk + 1] & LowerMask);
                Mt[kk] = Mt[kk + M] ^ (y >> 1) ^ Mag01[y & 0x0000_0001];
            }

            for (; kk < N - 1; kk++)
            {
                y = (Mt[kk] & UpperMask) | (Mt[kk + 1] & LowerMask);
                Mt[kk] = Mt[kk + (M - N)] ^ (y >> 1) ^ Mag01[y & 0x0000_0001];
            }

            y = (Mt[N - 1] & UpperMask) | (Mt[0] & LowerMask);
            Mt[N - 1] = Mt[M - 1] ^ (y >> 1) ^ Mag01[y & 0x0000_0001];

            MtIndex = 0;
        }

        y = Mt[MtIndex++];
        y ^= TemperingShiftU(y);
        y ^= TemperingShiftS(y) & TemperingMaskB;
        y ^= TemperingShiftT(y) & TemperingMaskC;
        y ^= TemperingShiftL(y);

        var x = BitConverter.ToInt32(BitConverter.GetBytes(y));
        return x;
    }

    public int ToInt32(int min, int max) => RandomNumber<Random4>.Pick(this, min, max);

    public float ToSingle()
    {
        var x = ToInt32();
        var y = BitConverter.ToUInt32(BitConverter.GetBytes(x));

        return y * 2.3283064370807973754314699618685e-10F;
    }

    public static implicit operator int(Random4 random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToInt32();
    }

    public static implicit operator float(Random4 random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToSingle();
    }

    private static uint TemperingShiftU(uint y) => y >> 11;

    private static uint TemperingShiftS(uint y) => y << 7;

    private static uint TemperingShiftT(uint y) => y << 15;

    private static uint TemperingShiftL(uint y) => y >> 18;
}
