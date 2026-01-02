// -----------------------------------------------------------------------
// <copyright file="CrashDialog.cs" company="NewSage">
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

namespace NewSage.DebugUtilities.Internals;

internal static partial class CrashDialog
{
    public static void ShowCrashDialog([NotNull] Exception ex, string dumpPath, [NotNull] DumpOptions dumpOptions)
    {
        const string title = "NewSage Engine Crash";
        var message = dumpOptions.Enabled
            ? $"""
                The engine has encountered a critical error and must close.

                Dump Path: {dumpPath}                       

                -- Technical Details --
                {ex}
                """
            : $"""
                The engine has encountered a critical error and must close.

                -- Technical Details --
                {ex}
                """;

        _ = Sdl.ShowErrorDialog(title, message);
    }

    private static partial class Sdl
    {
        private const uint MessageBoxError = 0x0010;

        public static bool ShowErrorDialog(string title, string message) =>
            ShowSimpleMessageBox(MessageBoxError, title, message, nint.Zero);

        [LibraryImport("SDL3", EntryPoint = "SDL_ShowMessageBox", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static partial bool ShowSimpleMessageBox(uint flags, string title, string message, nint window);
    }
}
