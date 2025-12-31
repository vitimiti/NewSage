// -----------------------------------------------------------------------
// <copyright file="CacheStraw.cs" company="NewSage">
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

public sealed class CacheStraw : Straw
{
    private readonly byte[] _cache;
    private int _index;
    private int _length;

    public CacheStraw(int length = 4096)
    {
        _cache = new byte[length];
        _index = 0;
        _length = 0;
    }

    public override int GetFrom(Span<byte> buffer)
    {
        var total = 0;
        var sourceLength = buffer.Length;
        var currentOffset = 0;

        if (sourceLength <= 0)
        {
            return total;
        }

        while (sourceLength > 0)
        {
            if (_length > 0)
            {
                var toCopy = _length < sourceLength ? _length : sourceLength;
                _cache.AsSpan(_index, toCopy).CopyTo(buffer.Slice(currentOffset, toCopy));

                sourceLength -= toCopy;
                _index += toCopy;
                total += toCopy;
                _length -= toCopy;
                currentOffset += toCopy;
            }

            if (sourceLength == 0)
            {
                break;
            }

            _length = base.GetFrom(_cache);
            _index = 0;
            if (_length == 0)
            {
                break;
            }
        }

        return total;
    }
}
