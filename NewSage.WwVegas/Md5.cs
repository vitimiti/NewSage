// -----------------------------------------------------------------------
// <copyright file="Md5.cs" company="NewSage">
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

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace NewSage.WwVegas;

public sealed class Md5
{
    // csharpier-ignore
    private static readonly byte[] Padding =
    [
        0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    ];

    private readonly uint[] _state = new uint[4];
    private readonly uint[] _count = new uint[2];
    private readonly byte[] _buffer = new byte[64];

    public Md5() => Init();

    public void Init()
    {
        _count[0] = _count[1] = 0;
        _state[0] = 0x67452301;
        _state[1] = 0xefcdab89;
        _state[2] = 0x98badcfe;
        _state[3] = 0x10325476;
    }

    public void Update(ReadOnlySpan<byte> input)
    {
        var index = (_count[0] >> 3) & 0x3F;
        if ((_count[0] += (uint)input.Length << 3) < (uint)input.Length << 3)
        {
            _count[1]++;
        }

        _count[1] += (uint)input.Length >> 29;

        var partLength = 64 - index;
        var i = 0;

        if (input.Length >= partLength)
        {
            var inputLength = (int)partLength;
            input[..inputLength].CopyTo(_buffer.AsSpan((int)index));
            Transform(_state, _buffer);

            for (i = inputLength; i + 63 < input.Length; i += 64)
            {
                Transform(_state, input.Slice(i, 64));
            }

            index = 0;
        }

        if (i < input.Length)
        {
            input[i..].CopyTo(_buffer.AsSpan((int)index));
        }
    }

    public void Final(Span<byte> digest)
    {
        Span<byte> bits = stackalloc byte[8];
        Encode(bits, _count);

        var index = (_count[0] >> 3) & 0x3f;
        var padLength = (index < 56) ? (56 - index) : (120 - index);

        Update(Padding.AsSpan(0, (int)padLength));
        Update(bits);

        Encode(digest[..16], _state);

        Array.Clear(_state);
        Array.Clear(_count);
        Array.Clear(_buffer);
    }

    private static uint F(uint x, uint y, uint z) => (x & y) | (~x & z);

    private static uint G(uint x, uint y, uint z) => (x & z) | (y & ~z);

    private static uint H(uint x, uint y, uint z) => x ^ y ^ z;

    private static uint I(uint x, uint y, uint z) => y ^ (x | ~z);

    private static uint RotateLeft(uint x, int n) => (x << n) | (x >> (32 - n));

    private static void Ff(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        a += F(b, c, d) + x + ac;
        a = RotateLeft(a, s);
        a += b;
    }

    private static void Gg(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        a += G(b, c, d) + x + ac;
        a = RotateLeft(a, s);
        a += b;
    }

    private static void Hh(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        a += H(b, c, d) + x + ac;
        a = RotateLeft(a, s);
        a += b;
    }

    private static void Ii(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        a += I(b, c, d) + x + ac;
        a = RotateLeft(a, s);
        a += b;
    }

    private static void Encode(Span<byte> output, ReadOnlySpan<uint> input)
    {
        for (int i = 0, j = 0; j < output.Length; i++, j += 4)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(output[j..], input[i]);
        }
    }

    private static void Decode(Span<uint> output, ReadOnlySpan<byte> input)
    {
        for (int i = 0, j = 0; j < input.Length; i++, j += 4)
        {
            output[i] = BinaryPrimitives.ReadUInt32LittleEndian(input[j..]);
        }
    }

    [SuppressMessage(
        "csharpsquid",
        "S2234:Arguments should be passed in the same order as the method parameters",
        Justification = "This is intentional."
    )]
    private static void Transform(uint[] state, ReadOnlySpan<byte> block)
    {
        var a = state[0];
        var b = state[1];
        var c = state[2];
        var d = state[3];
        Span<uint> x = stackalloc uint[16];
        Decode(x, block);

        Ff(ref a, b, c, d, x[0], 7, 0xD76A_A478);
        Ff(ref d, a, b, c, x[1], 12, 0xE8C7_B756);
        Ff(ref c, d, a, b, x[2], 17, 0x2420_70DB);
        Ff(ref b, c, d, a, x[3], 22, 0xC1BD_CEEE);
        Ff(ref a, b, c, d, x[4], 7, 0xF57C_0FAF);
        Ff(ref d, a, b, c, x[5], 12, 0x4787_C62A);
        Ff(ref c, d, a, b, x[6], 17, 0xA830_4613);
        Ff(ref b, c, d, a, x[7], 22, 0xFD46_9501);
        Ff(ref a, b, c, d, x[8], 7, 0x6980_98D8);
        Ff(ref d, a, b, c, x[9], 12, 0x8B44_F7AF);
        Ff(ref c, d, a, b, x[10], 17, 0xFFFF_5BB1);
        Ff(ref b, c, d, a, x[11], 22, 0x895C_D7BE);
        Ff(ref a, b, c, d, x[12], 7, 0x6B90_1122);
        Ff(ref d, a, b, c, x[13], 12, 0xFD98_7193);
        Ff(ref c, d, a, b, x[14], 17, 0xA679_438E);
        Ff(ref b, c, d, a, x[15], 22, 0x49B4_0821);

        Gg(ref a, b, c, d, x[1], 5, 0xF61E_2562);
        Gg(ref d, a, b, c, x[6], 9, 0xC040_B340);
        Gg(ref c, d, a, b, x[11], 14, 0x265E_5A51);
        Gg(ref b, c, d, a, x[0], 20, 0xE9B6_C7AA);
        Gg(ref a, b, c, d, x[5], 5, 0xD62F_105D);
        Gg(ref d, a, b, c, x[10], 9, 0x0244_1453);
        Gg(ref c, d, a, b, x[15], 14, 0xD8A1_E681);
        Gg(ref b, c, d, a, x[4], 20, 0xE7D3_FBC8);
        Gg(ref a, b, c, d, x[9], 5, 0x21E1_CDE6);
        Gg(ref d, a, b, c, x[14], 9, 0xC337_07D6);
        Gg(ref c, d, a, b, x[3], 14, 0xF4D5_0D87);
        Gg(ref b, c, d, a, x[8], 20, 0x455A_14ED);
        Gg(ref a, b, c, d, x[13], 5, 0xA9E3_E905);
        Gg(ref d, a, b, c, x[2], 9, 0xFCEF_A3F8);
        Gg(ref c, d, a, b, x[7], 14, 0x676F_02D9);
        Gg(ref b, c, d, a, x[12], 20, 0x8D2A_4C8A);

        Hh(ref a, b, c, d, x[5], 4, 0xFFFA_3942);
        Hh(ref d, a, b, c, x[8], 11, 0x8771_F681);
        Hh(ref c, d, a, b, x[11], 16, 0x6D9D_6122);
        Hh(ref b, c, d, a, x[14], 23, 0xFDE5_380C);
        Hh(ref a, b, c, d, x[1], 4, 0xA4BE_EA44);
        Hh(ref d, a, b, c, x[4], 11, 0x4BDE_CFA9);
        Hh(ref c, d, a, b, x[7], 16, 0xF6BB_4B60);
        Hh(ref b, c, d, a, x[10], 23, 0xBEBF_BC70);
        Hh(ref a, b, c, d, x[13], 4, 0x289B_7EC6);
        Hh(ref d, a, b, c, x[0], 11, 0xEAA1_27FA);
        Hh(ref c, d, a, b, x[3], 16, 0xD4EF_3085);
        Hh(ref b, c, d, a, x[6], 23, 0x0488_1D05);
        Hh(ref a, b, c, d, x[9], 4, 0xD9D4_D039);
        Hh(ref d, a, b, c, x[12], 11, 0xE6DB_99E5);
        Hh(ref c, d, a, b, x[15], 16, 0x1FA2_7CF8);
        Hh(ref b, c, d, a, x[2], 23, 0xC4AC_5665);

        Ii(ref a, b, c, d, x[0], 6, 0xF429_2244);
        Ii(ref d, a, b, c, x[7], 10, 0x432A_FF97);
        Ii(ref c, d, a, b, x[14], 15, 0xAB94_23A7);
        Ii(ref b, c, d, a, x[5], 21, 0xFC93_A039);
        Ii(ref a, b, c, d, x[12], 6, 0x655B_59C3);
        Ii(ref d, a, b, c, x[3], 10, 0x8F0C_CC92);
        Ii(ref c, d, a, b, x[10], 15, 0xFFEF_F47D);
        Ii(ref b, c, d, a, x[1], 21, 0x8584_5DD1);
        Ii(ref a, b, c, d, x[8], 6, 0x6FA8_7E4F);
        Ii(ref d, a, b, c, x[15], 10, 0xFE2C_E6E0);
        Ii(ref c, d, a, b, x[6], 15, 0xA301_4314);
        Ii(ref b, c, d, a, x[13], 21, 0x4E08_11A1);
        Ii(ref a, b, c, d, x[4], 6, 0xF753_7E82);
        Ii(ref d, a, b, c, x[11], 10, 0xBD3A_F235);
        Ii(ref c, d, a, b, x[2], 15, 0x2AD7_D2BB);
        Ii(ref b, c, d, a, x[9], 21, 0xEB86_D391);

        state[0] += a;
        state[1] += b;
        state[2] += c;
        state[3] += d;
    }
}
