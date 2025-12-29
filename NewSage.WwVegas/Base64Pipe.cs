// -----------------------------------------------------------------------
// <copyright file="Base64Pipe.cs" company="NewSage">
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

public class Base64Pipe(CodeControl control) : Pipe
{
    private readonly byte[] _cBuffer = new byte[4];
    private readonly byte[] _pBuffer = new byte[3];

    private int _counter;

    public override int Flush()
    {
        var length = 0;
        if (_counter > 0)
        {
            if (control is CodeControl.Encode)
            {
                var chars = Base64.Encode(_pBuffer.AsSpan(0, _counter), _cBuffer);
                length += base.Put(_cBuffer.AsSpan(0, chars));
            }
            else
            {
                var chars = Base64.Decode(_cBuffer.AsSpan(0, _counter), _pBuffer);
                length += base.Put(_pBuffer.AsSpan(0, chars));
            }

            _counter = 0;
        }

        length += base.Flush();
        return length;
    }

    public override int Put(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty)
        {
            return base.Put(source);
        }

        var total = 0;
        var sourceLength = source.Length;
        var sourceIndex = 0;

        byte[] from;
        int fromSize;
        byte[] to;

        if (control == CodeControl.Encode)
        {
            from = _pBuffer;
            fromSize = 3;
            to = _cBuffer;
        }
        else
        {
            from = _cBuffer;
            fromSize = 4;
            to = _pBuffer;
        }

        if (_counter > 0)
        {
            var len = (sourceLength < (fromSize - _counter)) ? sourceLength : (fromSize - _counter);
            source.Slice(sourceIndex, len).CopyTo(from.AsSpan(_counter));

            _counter += len;
            sourceLength -= len;
            sourceIndex += len;

            if (_counter == fromSize)
            {
                var outCount = (control == CodeControl.Encode) ? Base64.Encode(from, to) : Base64.Decode(from, to);

                total += base.Put(to.AsSpan(0, outCount));
                _counter = 0;
            }
        }

        while (sourceLength >= fromSize)
        {
            var outCount =
                control is CodeControl.Encode
                    ? Base64.Encode(source.Slice(sourceIndex, fromSize), to)
                    : Base64.Decode(source.Slice(sourceIndex, fromSize), to);

            sourceIndex += fromSize;
            total += base.Put(to.AsSpan(0, outCount));
            sourceLength -= fromSize;
        }

        if (sourceLength <= 0)
        {
            return total;
        }

        source[sourceIndex..].CopyTo(from);
        _counter = sourceLength;

        return total;
    }
}
