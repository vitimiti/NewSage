// -----------------------------------------------------------------------
// <copyright file="Base64.cs" company="NewSage">
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
using System.Runtime.InteropServices;

namespace NewSage.WwVegas;

public class Base64
{
    private const byte Bad = 0xFE;
    private const byte End = 0xFF;
    private const int PacketChars = 4;

    private static readonly byte Pad = (byte)'=';
    private static readonly byte[] Encoder =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"u8.ToArray();

    // csharpier-ignore
    private static readonly byte[] Decoder =
    [
        Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad,
        Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, 0x3E,
        Bad, Bad, Bad, 0x3F, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, Bad, Bad, Bad, End, Bad, Bad,
        Bad, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11,
        0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, Bad, Bad, Bad, Bad, Bad, Bad, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E,
        0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30,
        0x31, 0x32, 0x33, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad,
        Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad,
        Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad,
        Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad,
        Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad,
        Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad, Bad,
        Bad, Bad, Bad, Bad
    ];

    public static int Encode(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        if (source.Length == 0 || destination.Length == 0)
        {
            return 0;
        }

        var total = 0;
        var sourceIndex = 0;
        var destinationIndex = 0;
        var sourceLength = source.Length;
        var destinationLength = destination.Length;
        while (sourceLength > 0 && destinationLength >= PacketChars)
        {
            PacketType packet = default;
            var pad = 0;
            packet.Raw = 0;
            packet.C1 = source[sourceIndex++];
            sourceLength--;
            if (sourceLength > 0)
            {
                packet.C2 = source[sourceIndex++];
                sourceLength--;
            }
            else
            {
                pad++;
            }

            if (sourceLength > 0)
            {
                packet.C3 = source[sourceIndex++];
                sourceLength--;
            }
            else
            {
                pad++;
            }

            destination[destinationIndex++] = Encoder[packet.O1];
            destination[destinationIndex++] = Encoder[packet.O2];
            destination[destinationIndex++] = pad < 2 ? Encoder[packet.O3] : Pad;
            destination[destinationIndex++] = pad < 1 ? Encoder[packet.O4] : Pad;

            destinationLength -= PacketChars;
            total += PacketChars;
        }

        if (destinationLength > 0)
        {
            destination[destinationIndex] = (byte)'\0';
        }

        return total;
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "This is a complex algorithm and breaking into chunks may make it difficult to follow."
    )]
    public static int Decode(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        if (source.Length == 0 || destination.Length == 0)
        {
            return 0;
        }

        var total = 0;
        var sourceIndex = 0;
        var destinationIndex = 0;
        var sourceLength = source.Length;
        var destinationLength = destination.Length;
        while (sourceLength > 0 && destinationLength > 0)
        {
            PacketType packet = default;
            packet.Raw = 0;

            var pCount = 0;
            while (pCount < PacketChars && sourceLength > 0)
            {
                var c = source[sourceIndex++];
                sourceLength--;

                var code = Decoder[c];
                if (code == Bad)
                {
                    continue;
                }

                if (code == End)
                {
                    sourceLength = 0;
                    break;
                }

                switch (pCount)
                {
                    case 0:
                        packet.O1 = code;
                        break;
                    case 1:
                        packet.O2 = code;
                        break;
                    case 2:
                        packet.O3 = code;
                        break;
                    case 3:
                        packet.O4 = code;
                        break;
                    default:
                        break;
                }

                pCount++;
            }

            destination[destinationIndex++] = packet.C1;
            destinationLength--;
            total++;
            if (destinationLength > 0 && pCount > 2)
            {
                destination[destinationIndex++] = packet.C2;
                destinationLength--;
                total++;
            }

            if (destinationLength > 0 && pCount > 3)
            {
                destination[destinationIndex++] = packet.C3;
                destinationLength--;
                total++;
            }
        }

        return total;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct PacketType
    {
        [FieldOffset(0)]
        public uint Raw;

        public byte C1
        {
            readonly get => GetByte(BitConverter.IsLittleEndian ? 2 : 0);
            set => SetByte(BitConverter.IsLittleEndian ? 2 : 0, value);
        }

        public byte C2
        {
            readonly get => GetByte(1);
            set => SetByte(1, value);
        }

        public byte C3
        {
            readonly get => GetByte(BitConverter.IsLittleEndian ? 0 : 2);
            set => SetByte(BitConverter.IsLittleEndian ? 0 : 2, value);
        }

        public uint O1
        {
            readonly get => (GetRawByEndian() >> 18) & 0x3F;
            set => SetRawByEndian(18, value);
        }

        public uint O2
        {
            readonly get => (GetRawByEndian() >> 12) & 0x3F;
            set => SetRawByEndian(12, value);
        }

        public uint O3
        {
            readonly get => (GetRawByEndian() >> 6) & 0x3F;
            set => SetRawByEndian(6, value);
        }

        public uint O4
        {
            readonly get => GetRawByEndian() & 0x3F;
            set => SetRawByEndian(0, value);
        }

        private static uint ReverseBytes(uint value) =>
            ((value & 0x000000FFU) << 24)
            | ((value & 0x0000FF00U) << 8)
            | ((value & 0x00FF0000U) >> 8)
            | ((value & 0xFF000000U) >> 24);

        private readonly byte GetByte(int index) => (byte)((Raw >> (index * 8)) & 0xFF);

        private void SetByte(int index, byte value)
        {
            var mask = 0xFFU << (index * 8);
            Raw = (Raw & ~mask) | ((uint)value << (index * 8));
        }

        private readonly uint GetRawByEndian() => BitConverter.IsLittleEndian ? ReverseBytes(Raw) : Raw;

        private void SetRawByEndian(int shift, uint value)
        {
            if (BitConverter.IsLittleEndian)
            {
                var reversed = ReverseBytes(Raw);
                reversed = (reversed & ~(0x3FU << shift)) | ((value & 0x3F) << shift);
                Raw = ReverseBytes(reversed);
            }
            else
            {
                Raw = (Raw & ~(0x3FU << shift)) | ((value & 0x3F) << shift);
            }
        }
    }
}
