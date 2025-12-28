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

public class Base64Pipe(Base64PipeCodeControl control) : Pipe
{
    private readonly byte[] _cBuffer = new byte[4];
    private readonly byte[] _pBuffer = new byte[3];

    private int _counter;

    public override int Flush()
    {
        var length = 0;
        if (_counter > 0)
        {
            if (control is Base64PipeCodeControl.Encode)
            {
                var chars = Base64.Encode(_pBuffer.AsSpan()[.._counter], _cBuffer);
                length += base.Put(_cBuffer.AsSpan()[..chars]);
            }
            else
            {
                var chars = Base64.Decode(_cBuffer.AsSpan()[.._counter], _pBuffer);
                length += base.Put(_pBuffer.AsSpan()[..chars]);
            }

            _counter = 0;
        }

        length += base.Flush();
        return length;
    }

    public override int Put(ReadOnlySpan<byte> source)
    {
        if (source.Length < 1)
        {
            return base.Put(source);
        }

        var sourceLength = source.Length;
        var sourceIndex = 0;
        var total = 0;
        byte[] from;
        int fromSize;
        byte[] to;
        int toSize;

        if (control is Base64PipeCodeControl.Encode)
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

        if (_counter > 0)
        {
            var length = sourceLength < fromSize - _counter ? sourceLength : fromSize - _counter;
            Array.Copy(source[sourceIndex..].ToArray(), from[_counter..], length);
            _counter += length;
            sourceLength -= length;
            sourceIndex += length;

            if (_counter == fromSize)
            {
                var outCount =
                    control is Base64PipeCodeControl.Encode
                        ? Base64.Encode(from.AsSpan()[..fromSize], to.AsSpan()[..toSize])
                        : Base64.Decode(from.AsSpan()[..fromSize], to.AsSpan()[..toSize]);

                total += base.Put(to.AsSpan()[..outCount]);
                _counter = 0;
            }
        }

        while (sourceLength >= fromSize)
        {
            var outCount =
                control is Base64PipeCodeControl.Encode
                    ? Base64.Encode(source[sourceIndex..fromSize], to.AsSpan()[..toSize])
                    : Base64.Decode(source[sourceIndex..fromSize], to.AsSpan()[..toSize]);

            sourceIndex += fromSize;
            total += base.Put(to.AsSpan()[..outCount]);
            sourceLength -= fromSize;
        }

        if (sourceLength > 0)
        {
            Array.Copy(source.ToArray(), from, sourceLength);
            _counter = sourceLength;
        }

        return total;
    }
}
