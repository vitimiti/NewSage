// -----------------------------------------------------------------------
// <copyright file="SdlImage.cs" company="NewSage">
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NewSage.Interop.NativeSdl;

[SuppressMessage(
    "csharpsquid",
    "S4200:Native methods should be wrapped",
    Justification = "This is an unsafe, interop library for internal use."
)]
public static partial class SdlImage
{
    [LibraryImport("SDL3_image", EntryPoint = "IMG_Load", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static partial Sdl.Surface Load(string file);

    [LibraryImport("SDL3_image", EntryPoint = "IMG_Load_IO")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static partial Sdl.Surface Load(Sdl.IoStream io, [MarshalAs(UnmanagedType.I1)] bool closeIo);
}
