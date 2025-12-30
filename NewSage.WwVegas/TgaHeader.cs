// -----------------------------------------------------------------------
// <copyright file="TgaHeader.cs" company="NewSage">
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
public struct TgaHeader
{
    public byte IDLength;
    public byte ColorMapType;
    public byte ImageType;
    public short CMapStart;
    public short CMapLength;
    public byte CMapDepth;
    public short XOffset;
    public short YOffset;
    public short Width;
    public short Height;
    public byte PixelDepth;
    public byte ImageDescriptor;
}
