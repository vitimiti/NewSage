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

public class Random4 : IRandomGenerator
{
    private const int N = 624;
    private const int M = 397;
    private const uint MatrixA = 0x9908_B0DF;
    private const uint UpperMask = 0x8000_0000;
    private const uint LowerMask = 0x7FFF_FFFF;

    private readonly uint[] _mt = new uint[N];
    private int _mti;

    public int SignificantBits => 32;

    public Random4(uint seed = 4357)
    {
        if (seed == 0)
        {
            seed = 4375;
        }

        _mt[0] = seed;
        for (_mti = 1; _mti < N; _mti++)
        {
            _mt[_mti] = 69069 * _mt[_mti - 1];
        }
    }

    public int GetNext()
    {
        uint y;
        ReadOnlySpan<uint> mag01 = [0, MatrixA];

        if (_mti >= N)
        {
            int kk;
            for (kk = 0; kk < N - M; kk++)
            {
                y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                _mt[kk] = _mt[kk + M] ^ (y >> 1) ^ mag01[(int)(y & 1)];
            }

            for (; kk < N - 1; kk++)
            {
                y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                _mt[kk] = _mt[kk + (M - N)] ^ (y >> 1) ^ mag01[(int)(y & 1)];
            }

            y = (_mt[N - 1] & UpperMask) | (_mt[0] & LowerMask);
            _mt[N - 1] = _mt[M - 1] ^ (y >> 1) ^ mag01[(int)(y & 1)];
            _mti = 0;
        }

        y = _mt[_mti++];
        y ^= y >> 11;
        y ^= (y << 7) & 0x9d2c5680;
        y ^= (y << 15) & 0xefc60000;
        y ^= y >> 18;

        return (int)y;
    }

    public int GetNext(int min, int max) => IRandomGenerator.Pick(this, min, max);

    public float GetFloat() => (uint)GetNext() * 2.3283064365386963e-10f;

    public int ToInt32() => GetNext();

    public static implicit operator int(Random4 random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToInt32();
    }
}
