// -----------------------------------------------------------------------
// <copyright file="Lzo.cs" company="NewSage">
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

namespace NewSage.WwVegas;

public static class Lzo
{
    private const int M2MaxOffset = 0x0800;
    private const int M3MaxOffset = 0x4000;
    private const int M4MaxOffset = 0xBFFF;
    private const int M3Marker = 32;
    private const int M4Marker = 16;

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "This is a complex algorithm and breaking into chunks may make it difficult to follow."
    )]
    public static int Decompress(ReadOnlySpan<byte> input, Span<byte> output, out int outLen)
    {
        var ip = 0;
        var op = 0;
        int t;

        if (input[ip] > 17)
        {
            t = input[ip++] - 17;
            input.Slice(ip, t).CopyTo(output[op..]);
            ip += t;
            op += t;
        }

        var eofFound = false;
        while (ip < input.Length && !eofFound)
        {
            t = input[ip++];

            if (t < 16)
            {
                if (t == 0)
                {
                    t = 15;
                    while (input[ip] == 0)
                    {
                        t += 255;
                        ip++;
                    }

                    t += input[ip++];
                }

                input.Slice(ip, t + 3).CopyTo(output[op..]);
                ip += t + 3;
                op += t + 3;

                t = input[ip++];
            }

            var matchDone = false;
            while (!matchDone && !eofFound)
            {
                var isM1Match = false;

                int mPos;
                if (t >= 64)
                {
                    mPos = op - 1 - ((t >> 2) & 7) - (input[ip++] << 3);
                    t = (t >> 5) - 1;
                }
                else if (t >= 32)
                {
                    t &= 31;
                    if (t == 0)
                    {
                        t = 31;
                        while (input[ip] == 0)
                        {
                            t += 255;
                            ip++;
                        }

                        t += input[ip++];
                    }

                    mPos = op - 1 - (input[ip] >> 2) - (input[ip + 1] << 6);
                    ip += 2;
                }
                else if (t >= 16)
                {
                    mPos = op - ((t & 8) << 11);
                    t &= 7;
                    if (t == 0)
                    {
                        t = 7;
                        while (input[ip] == 0)
                        {
                            t += 255;
                            ip++;
                        }

                        t += input[ip++];
                    }

                    mPos -= (input[ip] >> 2) + (input[ip + 1] << 6);
                    ip += 2;
                    if (mPos == op)
                    {
                        eofFound = true;
                        break;
                    }

                    mPos -= 0x4000;
                }
                else
                {
                    mPos = op - 1 - (t >> 2) - (input[ip++] << 2);
                    output[op++] = output[mPos++];
                    output[op++] = output[mPos];
                    isM1Match = true;
                }

                if (!isM1Match)
                {
                    output[op++] = output[mPos++];
                    output[op++] = output[mPos++];
                    while (t-- > 0)
                    {
                        output[op++] = output[mPos++];
                    }
                }

                t = input[ip - 2] & 3;
                if (t == 0)
                {
                    matchDone = true;
                }
                else
                {
                    while (t-- > 0)
                    {
                        output[op++] = input[ip++];
                    }

                    t = input[ip++];
                }
            }
        }

        outLen = op;
        return eofFound ? 0 : -1;
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "This is a complex algorithm and breaking into chunks may make it difficult to follow."
    )]
    public static int Compress(ReadOnlySpan<byte> input, Span<byte> output, out int outLen)
    {
        var op = 0;
        var ip = 0;
        var inLen = input.Length;

        switch (inLen)
        {
            case <= 0:
                outLen = 0;
                return 0;
            case <= 13:
                output[op++] = (byte)(17 + inLen);
                input.CopyTo(output[op..]);
                op += inLen;
                WriteEof(output, ref op);
                outLen = op;
                return 0;
            default:
                break;
        }

        var dict = new int[16384];
        Array.Fill(dict, -1);

        var ipEnd = inLen - 13;
        var ii = 0;

        var dv = FetchHash(input, ip);
        UpdateDict(dict, dv, ip++);
        dv = NextHash(dv, input, ip);
        UpdateDict(dict, dv, ip++);
        dv = NextHash(dv, input, ip);
        UpdateDict(dict, dv, ip++);
        dv = NextHash(dv, input, ip);
        UpdateDict(dict, dv, ip++);

        while (ip < ipEnd)
        {
            dv = NextHash(dv, input, ip);
            var dIndex = GetDIndex(dv);
            var mPos = dict[dIndex];
            dict[dIndex] = ip;

            var mOff = ip - mPos;

            if (
                mPos != -1
                && mOff <= M4MaxOffset
                && input[mPos] == input[ip]
                && input[mPos + 1] == input[ip + 1]
                && input[mPos + 2] == input[ip + 2]
            )
            {
                var litLen = ip - ii;
                if (litLen > 0)
                {
                    EncodeLiteralRun(input.Slice(ii, litLen), output, ref op);
                }

                var mLen = 3;
                while (ip + mLen < inLen && input[mPos + mLen] == input[ip + mLen])
                {
                    mLen++;
                }

                EncodeMatch(mLen, mOff, output, ref op);

                ip += mLen;
                ii = ip;

                if (ip >= ipEnd)
                {
                    break;
                }

                dv = FetchHash(input, ip);
                UpdateDict(dict, dv, ip++);
                dv = NextHash(dv, input, ip);
                UpdateDict(dict, dv, ip++);
                dv = NextHash(dv, input, ip);
                UpdateDict(dict, dv, ip++);
                dv = NextHash(dv, input, ip);
                UpdateDict(dict, dv, ip++);
                continue;
            }

            ip++;
        }

        var finalLitLen = inLen - ii;
        if (finalLitLen > 0)
        {
            EncodeLiteralRun(input.Slice(ii, finalLitLen), output, ref op);
        }

        WriteEof(output, ref op);
        outLen = op;
        return 0;
    }

    private static void WriteEof(Span<byte> output, ref int op)
    {
        output[op++] = M4Marker | 1;
        output[op++] = 0;
        output[op++] = 0;
    }

    private static uint FetchHash(ReadOnlySpan<byte> p, int i) => (uint)((p[i + 2] << 10) ^ (p[i + 1] << 5) ^ p[i]);

    private static uint NextHash(uint dv, ReadOnlySpan<byte> p, int i) =>
        ((dv ^ p[i - 1]) >> 5) ^ (uint)(p[i + 2] << 10);

    private static int GetDIndex(uint dv) => (int)(((40799u * dv) >> 5) & 0x3FFF);

    private static void UpdateDict(int[] dict, uint dv, int ip) => dict[GetDIndex(dv)] = ip;

    private static void EncodeLiteralRun(ReadOnlySpan<byte> literals, Span<byte> output, ref int op)
    {
        var t = literals.Length;
        switch (t)
        {
            case <= 3:
                output[op - 2] |= (byte)t;
                break;
            case <= 18:
                output[op++] = (byte)(t - 3);
                break;
            default:
            {
                var tt = t - 18;
                output[op++] = 0;
                while (tt > 255)
                {
                    tt -= 255;
                    output[op++] = 0;
                }

                output[op++] = (byte)tt;
                break;
            }
        }

        literals.CopyTo(output[op..]);
        op += t;
    }

    private static void EncodeMatch(int mLen, int mOff, Span<byte> output, ref int op)
    {
        switch (mOff)
        {
            case <= M2MaxOffset when mLen <= 8:
                mOff--;
                output[op++] = (byte)(((mLen - 1) << 5) | ((mOff & 7) << 2));
                output[op++] = (byte)(mOff >> 3);
                break;
            case <= M3MaxOffset:
            {
                mOff--;
                if (mLen <= 33)
                {
                    output[op++] = (byte)(M3Marker | (mLen - 2));
                }
                else
                {
                    mLen -= 33;
                    output[op++] = M3Marker;
                    while (mLen > 255)
                    {
                        mLen -= 255;
                        output[op++] = 0;
                    }

                    output[op++] = (byte)mLen;
                }

                output[op++] = (byte)((mOff & 63) << 2);
                output[op++] = (byte)(mOff >> 6);
                break;
            }

            default:
            {
                mOff -= 0x4000;
                if (mLen <= 9)
                {
                    output[op++] = (byte)(M4Marker | ((mOff & 0x4000) >> 11) | (mLen - 2));
                }
                else
                {
                    mLen -= 9;
                    output[op++] = (byte)(M4Marker | ((mOff & 0x4000) >> 11));
                    while (mLen > 255)
                    {
                        mLen -= 255;
                        output[op++] = 0;
                    }

                    output[op++] = (byte)mLen;
                }

                output[op++] = (byte)((mOff & 63) << 2);
                output[op++] = (byte)(mOff >> 6);
                break;
            }
        }
    }
}
