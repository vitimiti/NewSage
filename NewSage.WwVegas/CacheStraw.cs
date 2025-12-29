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

public class CacheStraw(BufferedStream bufferedStream) : Straw
{
    private int _index;
    private int _length;

    public override int Get(Span<byte> buffer)
    {
        var total = 0;
        var sourceLength = buffer.Length;
        var sourceIndex = 0;
        if (sourceLength <= 0)
        {
            return total;
        }

        while (sourceLength > 0)
        {
            if (_length > 0)
            {
                var toCopy = _length < sourceLength ? _length : sourceLength;
                var count = sourceIndex + toCopy;
                bufferedStream.Position = _index;
                _ = bufferedStream.Read(buffer[sourceIndex..count]);

                sourceLength -= toCopy;
                _index += toCopy;
                total += toCopy;
                _length -= toCopy;
                sourceIndex += toCopy;
            }

            if (sourceLength == 0)
            {
                break;
            }

            _length = base.Get(buffer[sourceIndex..]);
            _index = 0;
            if (_length == 0)
            {
                break;
            }
        }

        return total;
    }
}
