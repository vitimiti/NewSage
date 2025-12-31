// -----------------------------------------------------------------------
// <copyright file="MiniDumper.cs" company="NewSage">
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
using System.Globalization;
using Microsoft.Diagnostics.NETCore.Client;

namespace NewSage.Debug;

internal sealed class MiniDumper
{
    private readonly DumpOptions _options;

    public MiniDumper(DumpOptions dumpOptions)
    {
        _options = dumpOptions;
        DumpDirectoryPath = Path.Combine(
            OperatingSystem.IsWindows()
                ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            _options.DumpDirectory
        );
    }

    public string DumpDirectoryPath { get; }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Do not throw in crashing pathways."
    )]
    public void WriteDump(string reason)
    {
        if (!_options.Enabled)
        {
            System.Diagnostics.Debug.WriteLine($"Dumping disabled: {reason}");
            return;
        }

        try
        {
            if (!Directory.Exists(DumpDirectoryPath) && !Directory.CreateDirectory(DumpDirectoryPath).Exists)
            {
                Console.Error.WriteLine($"Failed to create dump directory: {DumpDirectoryPath}");
                return;
            }

            CleanOldDumps();

            var processId = Environment.ProcessId;
            var client = new DiagnosticsClient(processId);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var fileName = $"{_options.FilePrefix}_{reason}_{timestamp}.dmp";
            var fullPath = Path.Combine(DumpDirectoryPath, fileName);

            client.WriteDump(_options.DumpType, fullPath);
            System.Diagnostics.Debug.WriteLine($"Dump written to: {fullPath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to write dump: {ex}");
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Do not throw in crashing pathways."
    )]
    private void CleanOldDumps()
    {
        if (!Directory.Exists(DumpDirectoryPath))
        {
            return;
        }

        try
        {
            var files = new DirectoryInfo(DumpDirectoryPath)
                .GetFiles($"{_options.FilePrefix}*.dmp")
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            if (files.Count < _options.MaxDumpFiles)
            {
                return;
            }

            foreach (FileInfo file in files.Skip((int)(_options.MaxDumpFiles - 1)))
            {
                file.Delete();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clean old dumps: {ex}");
        }
    }
}
