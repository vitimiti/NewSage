// -----------------------------------------------------------------------
// <copyright file="UnhandledExceptionHandler.cs" company="NewSage">
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
using NewSage.Debug.Internals;

namespace NewSage.Debug;

public static class UnhandledExceptionHandler
{
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Do not throw in crashing pathways."
    )]
    public static void Install(DumpOptions? dumpOptions = null) =>
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            DumpOptions options = dumpOptions ?? new DumpOptions();

            try
            {
                var exception = (Exception)args.ExceptionObject;
                Console.WriteLine(exception);
                var dumpService = new MiniDumper(options);
                dumpService.WriteDump("UnhandledException");
                Internals.CrashDialog.ShowCrashDialog(exception, dumpService.DumpDirectoryPath, options);
            }
            catch
            {
                // Let the OS deal with any exception here!
            }
        };
}
