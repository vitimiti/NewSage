// -----------------------------------------------------------------------
// <copyright file="VegasMath.cs" company="NewSage">
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
using System.Runtime.Versioning;

namespace NewSage.WwVegas.WwMath;

public static partial class VegasMath
{
    public static float InvSqrt(float value) => 1F / float.Sqrt(value);

    public static bool IsValid(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    [SuppressMessage(
        "Security",
        "CA5394:Do not use insecure randomness",
        Justification = "This is not designed for safety."
    )]
    [SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Worse readability.")]
    public static float RandomFloat()
    {
        int randValue;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            randValue = Native.MsVcRtRand();
        }
        else if (
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
        )
        {
            randValue = Native.LibCRand();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            randValue = Native.LibSystemRand();
        }
        else
        {
            randValue = Random.Shared.Next();
        }

        const int mask = 0x0FFF;
        return (randValue & mask) / (float)mask;
    }

    private static partial class Native
    {
        [SupportedOSPlatform("windows")]
        [LibraryImport("msvcrt.dll", EntryPoint = "rand")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int MsVcRtRand();

        [SupportedOSPlatform("macos")]
        [LibraryImport("libSystem", EntryPoint = "rand")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int LibSystemRand();

        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freebsd")]
        [LibraryImport("libc", EntryPoint = "rand")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int LibCRand();
    }
}
