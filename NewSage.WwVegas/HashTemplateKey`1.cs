// -----------------------------------------------------------------------
// <copyright file="HashTemplateKey`1.cs" company="NewSage">
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

public class HashTemplateKey<T>
    where T : IConvertibleToUInt32
{
    public uint GetHashValue(T key)
    {
        var hashValue = BitConverter.ToUInt32(BitConverter.GetBytes(key.ToUInt32()));
        return hashValue + (hashValue >> 5) + (hashValue >> 10) + (hashValue >> 20);
    }

    public uint GetHashValue(float value)
    {
        var z = BitConverter.ToUInt32(BitConverter.GetBytes(value));
        return (z >> 22) + (z >> 12) + z;
    }
}
