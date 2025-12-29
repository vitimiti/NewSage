// -----------------------------------------------------------------------
// <copyright file="CrcEngine.cs" company="NewSage">
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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace NewSage.WwVegas;

public class CrcEngine
{
    public CrcEngine(long initial = 0)
    {
        Crc = initial;
        Index = 0;
        StagingBuffer = default;
        StagingBuffer = default;
    }

    protected long Crc { get; set; }

    protected int Index { get; set; }

    protected bool BufferNeedsData => Index != 0;

    protected long Value => BufferNeedsData ? BitOperations.RotateLeft((uint)Crc, 1) + StagingBuffer.Composite : Crc;

    protected StagingBufferStruct StagingBuffer { get; set; }

    public long ToInt64() => Value;

    public long ToInt64(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length <= 0)
        {
            return Value;
        }

        var bufferIndex = 0;
        var bytesLeft = buffer.Length;

        while (bytesLeft > 0 && BufferNeedsData)
        {
            Submit(buffer[bufferIndex++]);
            bytesLeft--;
        }

        var longCount = bytesLeft / sizeof(long);
        while (longCount-- > 0)
        {
            Crc = (long)(BitOperations.RotateLeft((ulong)Crc, 1) + (ulong)BitConverter.ToInt64(buffer[bufferIndex..]));
            bufferIndex += sizeof(long);
            bytesLeft -= sizeof(long);
        }

        while (bytesLeft > 0)
        {
            Submit(buffer[bufferIndex++]);
            bytesLeft--;
        }

        return Value;
    }

    public unsafe void Submit(byte dataNumber)
    {
        StagingBufferStruct stagingBuffer = StagingBuffer;
        stagingBuffer.Buffer[Index++] = dataNumber;
        StagingBuffer = stagingBuffer;
        if (Index == sizeof(long))
        {
            Crc = Value;
            StagingBuffer = StagingBuffer with { Composite = 0 };
            Index = 0;
        }
    }

    public static implicit operator long(CrcEngine crcEngine)
    {
        ArgumentNullException.ThrowIfNull(crcEngine);
        return crcEngine.ToInt64();
    }

    [StructLayout(LayoutKind.Explicit)]
    [SuppressMessage(
        "Performance",
        "CA1815:Override equals and operator equals on value types",
        Justification = "This is an unsafe union that is never compared."
    )]
    protected unsafe struct StagingBufferStruct
    {
        [FieldOffset(0)]
        public long Composite;

        [FieldOffset(0)]
        public fixed byte Buffer[sizeof(long)];
    }
}
