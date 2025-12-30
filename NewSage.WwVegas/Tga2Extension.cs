// -----------------------------------------------------------------------
// <copyright file="Tga2Extension.cs" company="NewSage">
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[SuppressMessage(
    "Performance",
    "CA1815:Override equals and operator equals on value types",
    Justification = "Not used in this structure."
)]
public struct Tga2Extension
{
    public short ExtSize;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
    public byte[] AuthName;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 324)]
    public byte[] AuthComment;
    public short Month;
    public short Day;
    public short Year;
    public short Hour;
    public short Minute;
    public short Second;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
    public byte[] JobName;
    public short JobHour;
    public short JobMinute;
    public short JobSecond;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
    public byte[] SoftID;
    public short SoftVerNumber;
    public byte SoftVerLetter;
    public int KeyColor;
    public short AspectNumer;
    public short AspectDenom;
    public short GammaNumer;
    public short GammaDenom;
    public int ColorCor;
    public int PostStamp;
    public int ScanLine;
    public byte Attributes;
}
