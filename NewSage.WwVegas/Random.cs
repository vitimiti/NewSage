// -----------------------------------------------------------------------
// <copyright file="Random.cs" company="NewSage">
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

public class Random : IRandomNumberGenerator
{
    protected const uint MultConstant = 0x41C6_4E6D;
    protected const uint AddConstant = 0x0000_3039;
    protected const int ThrowAwayBits = 10;

    public Random(ulong seed = 0) => Seed = seed;

    public int SignificantBits => 15;

    public int ToInt32()
    {
        Seed = (Seed * MultConstant) + AddConstant;
        return (int)((Seed >> ThrowAwayBits) & ~(~0UL << SignificantBits));
    }

    public int ToInt32(int min, int max) => RandomNumber<Random>.Pick(this, min, max);

    protected ulong Seed { get; set; }

    public static implicit operator int(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToInt32();
    }
}
