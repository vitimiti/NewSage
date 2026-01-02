// -----------------------------------------------------------------------
// <copyright file="TransferCrcService.cs" company="NewSage">
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
using System.Numerics;
using System.Runtime.InteropServices;
using NewSage.Game.SaveLoad;

namespace NewSage.Game.Transfer;

internal class TransferCrcService : TransferService
{
    private uint _crc;

    public TransferCrcService() => Mode = TransferMode.Crc;

    public uint Crc
    {
        get => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(_crc) : _crc;
        protected set => _crc = value;
    }

    public override void Open(string identifier)
    {
        base.Open(identifier);
        Crc = 0;
    }

    public override void Close() { }

    public override int BeginBlock() => 0;

    public override void EndBlock() { }

    public override void Skip(int dataSize) { }

    public override void TransferSnapshot(ref ISnapshot? snapshot) => snapshot?.Crc(this);

    protected override void TransferCore(Span<byte> data)
    {
        unchecked
        {
            Span<uint> uintSpan = MemoryMarshal.Cast<byte, uint>(data);
            foreach (var value in uintSpan)
            {
                AddCrc(value);
            }

            var remainder = data.Length & 3;
            if (remainder <= 0)
            {
                return;
            }

            uint remainderValue = 0;
            var offset = data.Length - remainder;

            if (remainder >= 3)
            {
                remainderValue += (uint)data[offset + 2] << 16;
            }

            if (remainder >= 2)
            {
                remainderValue += (uint)data[offset + 1] << 8;
            }

            remainderValue += data[offset];

            _crc = BitOperations.RotateLeft(_crc, 1) + remainderValue;
        }
    }

    protected void AddCrc(uint value)
    {
        unchecked
        {
            var bigEndianValue = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
            _crc = BitOperations.RotateLeft(_crc, 1) + bigEndianValue;
        }
    }
}
