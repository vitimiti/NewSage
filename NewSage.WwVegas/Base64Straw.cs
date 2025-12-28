// -----------------------------------------------------------------------
// <copyright file="Base64Straw.cs" company="NewSage">
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

public class Base64Straw(CodeControl control) : Straw
{
    private readonly byte[] _cBuffer = new byte[4];

    private readonly byte[] _pBuffer = new byte[3];

    private int _counter;

    public override int Get(Span<byte> buffer)
    {
        var sourceLength = buffer.Length;
        var sourceIndex = 0;
        var total = 0;
        byte[] from;
        int fromSize;
        byte[] to;
        int toSize;

        if (control is CodeControl.Encode)
        {
            from = _pBuffer;
            fromSize = _pBuffer.Length;
            to = _cBuffer;
            toSize = _cBuffer.Length;
        }
        else
        {
            from = _cBuffer;
            fromSize = _cBuffer.Length;
            to = _pBuffer;
            toSize = _pBuffer.Length;
        }

        while (sourceLength > 0)
        {
            if (_counter > 0)
            {
                var length = sourceLength < _counter ? sourceLength : _counter;
                to.AsSpan(toSize - _counter, length).CopyTo(buffer[sourceIndex..]);
                _counter -= length;
                sourceLength -= length;
                sourceIndex += length;
                total += length;
            }

            if (sourceLength == 0)
            {
                break;
            }

            var inCount = base.Get(from.AsSpan()[..fromSize]);
            _counter =
                control is CodeControl.Encode
                    ? Base64.Encode(from.AsSpan()[..inCount], to.AsSpan()[..toSize])
                    : Base64.Decode(from.AsSpan()[..inCount], to.AsSpan()[..toSize]);

            if (_counter == 0)
            {
                break;
            }
        }

        return total;
    }
}
