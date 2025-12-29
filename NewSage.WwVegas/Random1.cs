// -----------------------------------------------------------------------
// <copyright file="Random1.cs" company="NewSage">
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

public class Random1(uint seed = 0) : IRandomGenerator
{
    private const uint MultConstant = 0x41C64E6D;
    private const uint AddConstant = 0x00003039;

    private uint _seed = seed;

    public int SignificantBits => 15;

    public int GetNext()
    {
        _seed = (_seed * MultConstant) + AddConstant;
        return (int)((_seed >> 10) & 0x7FFF);
    }

    public int GetNext(int min, int max) => IRandomGenerator.Pick(this, min, max);

    public int ToInt32() => GetNext();

    public static implicit operator int(Random1 random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.ToInt32();
    }
}
