// -----------------------------------------------------------------------
// <copyright file="TransferService.cs" company="NewSage">
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using NewSage.BaseTypes;
using NewSage.Game.Exceptions.TransferServiceExceptions;
using NewSage.Game.SaveLoad;

namespace NewSage.Game.Transfer;

internal abstract class TransferService
{
    public TransferOptions Options { get; set; } = TransferOptions.None;

    public TransferMode Mode { get; protected set; } = TransferMode.Invalid;

    public string Identifier { get; protected set; } = string.Empty;

    public virtual void Open(string identifier) => Identifier = identifier;

    public abstract void Close();

    public abstract int BeginBlock();

    public abstract void EndBlock();

    public abstract void Skip(int dataSize);

    public abstract void TransferSnapshot(ISnapshot snapshot);

    public virtual void TransferVersion(ref byte versionData, byte currentVersion)
    {
        Span<byte> versionSpan = [currentVersion];
        TransferCore(versionSpan);
        versionData = versionSpan[0];

        if (versionData > currentVersion)
        {
            throw new TransferServiceInvalidVersionException(
                $"Unknown version '{versionData}' should be no higher than '{currentVersion}'."
            );
        }
    }

    public virtual void TransferSByte(ref sbyte sbyteData)
    {
        Span<byte> sbyteSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref sbyteData, 1));
        TransferCore(sbyteSpan);
    }

    public virtual void TransferByte(ref byte byteData)
    {
        Span<byte> byteSpan = MemoryMarshal.CreateSpan(ref byteData, 1);
        TransferCore(byteSpan);
    }

    public virtual void TransferBoolean(ref bool booleanData)
    {
        Span<byte> booleanSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<bool, byte>(ref booleanData), 1);
        TransferCore(booleanSpan);
    }

    public virtual void TransferInt32(ref int intData)
    {
        Span<byte> intSpan = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(intSpan, intData);
        TransferCore(intSpan);
        intData = BinaryPrimitives.ReadInt32BigEndian(intSpan);
    }

    public virtual void TransferInt64(ref long longData)
    {
        Span<byte> longSpan = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(longSpan, longData);
        TransferCore(longSpan);
        longData = BinaryPrimitives.ReadInt64BigEndian(longSpan);
    }

    public virtual void TransferUInt32(ref uint uintData)
    {
        Span<byte> uintSpan = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(uintSpan, uintData);
        TransferCore(uintSpan);
        uintData = BinaryPrimitives.ReadUInt32BigEndian(uintSpan);
    }

    public virtual void TransferInt16(ref short shortData)
    {
        Span<byte> shortSpan = stackalloc byte[sizeof(short)];
        BinaryPrimitives.WriteInt16BigEndian(shortSpan, shortData);
        TransferCore(shortSpan);
        shortData = BinaryPrimitives.ReadInt16BigEndian(shortSpan);
    }

    public virtual void TransferUInt16(ref ushort ushortData)
    {
        Span<byte> ushortSpan = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(ushortSpan, ushortData);
        TransferCore(ushortSpan);
        ushortData = BinaryPrimitives.ReadUInt16BigEndian(ushortSpan);
    }

    public virtual void TransferSingle(ref float floatData)
    {
        Span<byte> floatSpan = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleBigEndian(floatSpan, floatData);
        TransferCore(floatSpan);
        floatData = BinaryPrimitives.ReadSingleBigEndian(floatSpan);
    }

    public virtual void TransferMarkerLabel(string labelData) { }

    public virtual void TransferAsciiString(ref string stringData)
    {
        Span<byte> stringSpan = Encoding.ASCII.GetBytes(stringData);
        TransferCore(stringSpan);
        stringData = Encoding.ASCII.GetString(stringSpan);
    }

    public virtual void TransferUnicodeString(ref string stringData)
    {
        Span<byte> stringSpan = Encoding.Unicode.GetBytes(stringData);
        TransferCore(stringSpan);
        stringData = Encoding.Unicode.GetString(stringSpan);
    }

    public virtual void TransferFCoord3D(ref FCoord3D coordData)
    {
        var x = coordData.X;
        var y = coordData.Y;
        var z = coordData.Z;

        TransferSingle(ref x);
        TransferSingle(ref y);
        TransferSingle(ref z);

        coordData.X = x;
        coordData.Y = y;
        coordData.Z = z;
    }

    public virtual void TransferCoord3D(ref Coord3D coordData)
    {
        var x = coordData.X;
        var y = coordData.Y;
        var z = coordData.Z;

        TransferInt32(ref x);
        TransferInt32(ref y);
        TransferInt32(ref z);

        coordData.X = x;
        coordData.Y = y;
        coordData.Z = z;
    }

    public virtual void TransferFRegion3D(ref FRegion3D regionData)
    {
        FCoord3D lo = regionData.Lo;
        FCoord3D hi = regionData.Hi;

        TransferFCoord3D(ref lo);
        TransferFCoord3D(ref hi);

        regionData.Lo = lo;
        regionData.Hi = hi;
    }

    public virtual void TransferRegion3D(ref Region3D regionData)
    {
        Coord3D lo = regionData.Lo;
        Coord3D hi = regionData.Hi;

        TransferCoord3D(ref lo);
        TransferCoord3D(ref hi);

        regionData.Lo = lo;
        regionData.Hi = hi;
    }

    public virtual void TransferFCoord2D(ref FCoord2D coordData)
    {
        var x = coordData.X;
        var y = coordData.Y;

        TransferSingle(ref x);
        TransferSingle(ref y);

        coordData.X = x;
        coordData.Y = y;
    }

    public virtual void TransferCoord2D(ref Coord2D coordData)
    {
        var x = coordData.X;
        var y = coordData.Y;

        TransferInt32(ref x);
        TransferInt32(ref y);

        coordData.X = x;
        coordData.Y = y;
    }

    public virtual void TransferFRegion2D(ref FRegion2D regionData)
    {
        FCoord2D lo = regionData.Lo;
        FCoord2D hi = regionData.Hi;

        TransferFCoord2D(ref lo);
        TransferFCoord2D(ref hi);

        regionData.Lo = lo;
        regionData.Hi = hi;
    }

    public virtual void TransferRegion2D(ref Region2D regionData)
    {
        Coord2D lo = regionData.Lo;
        Coord2D hi = regionData.Hi;

        TransferCoord2D(ref lo);
        TransferCoord2D(ref hi);

        regionData.Lo = lo;
        regionData.Hi = hi;
    }

    public virtual void TransferFRange(ref SingleRange rangeData)
    {
        var lo = rangeData.Lo;
        var hi = rangeData.Hi;

        TransferSingle(ref lo);
        TransferSingle(ref hi);

        rangeData.Lo = lo;
        rangeData.Hi = hi;
    }

    protected abstract void TransferCore(Span<byte> data);
}
