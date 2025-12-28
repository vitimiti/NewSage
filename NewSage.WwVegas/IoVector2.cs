// -----------------------------------------------------------------------
// <copyright file="IoVector2.cs" company="NewSage">
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

public record IoVector2(float X, float Y)
{
    internal static int BufferSize => sizeof(float) * 2;

    internal static IoVector2 FromBuffer(ReadOnlySpan<byte> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, BufferSize);
        return new IoVector2(BitConverter.ToSingle(buffer), BitConverter.ToSingle(buffer[sizeof(float)..]));
    }

    internal byte[] ToBuffer()
    {
        var buffer = new byte[BufferSize];
        BitConverter.GetBytes(X).CopyTo(buffer, 0);
        BitConverter.GetBytes(Y).CopyTo(buffer, sizeof(float));
        return buffer;
    }
}
