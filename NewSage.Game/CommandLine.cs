// -----------------------------------------------------------------------
// <copyright file="CommandLine.cs" company="NewSage">
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

using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Extensions.Configuration;

namespace NewSage.Game;

internal static class CommandLine
{
    private static readonly Dictionary<string, string> SwitchMappings = new()
    {
        { "--profile", "EnableProfiling" },
        { "--dump", "DumpOptions:Enabled" },
        { "--dump-dir", "DumpOptions:DumpDirectory" },
        { "--dump-type", "DumpOptions:DumpType" },
        { "--dump-max", "DumpOptions:MaxDumpFiles" },
        { "--dump-prefix", "DumpOptions:FilePrefix" },
        { "--working-dir", "WorkingDirectory" },
    };

    public static void ApplyUserRuntimeOptions(string[] args, GameOptions options)
    {
        if (args.Length == 0)
        {
            return;
        }

        var builder = new ConfigurationBuilder();
        _ = builder.AddCommandLine(args, SwitchMappings);

        IConfigurationRoot config = builder.Build();

        ProcessProfilingOptions(config, options);
        ProcessDumpOptions(config, options);
    }

    private static void ProcessProfilingOptions(IConfigurationRoot config, GameOptions options)
    {
        if (bool.TryParse(config["EnableProfiling"], out var profile))
        {
            options.EnableProfiling = profile;
        }
    }

    private static void ProcessDumpOptions(IConfigurationRoot config, GameOptions options)
    {
        if (bool.TryParse(config["DumpOptions:Enabled"], out var dump))
        {
            options.DumpOptions.Enabled = dump;
        }

        var dir = config["DumpOptions:DumpDirectory"];
        if (!string.IsNullOrEmpty(dir))
        {
            options.DumpOptions.DumpDirectory = dir;
        }

        if (Enum.TryParse(config["DumpOptions:DumpType"], ignoreCase: true, out DumpType dumpType))
        {
            options.DumpOptions.DumpType = dumpType;
        }

        if (uint.TryParse(config["DumpOptions:MaxDumpFiles"], out var maxFiles))
        {
            options.DumpOptions.MaxDumpFiles = maxFiles;
        }

        var prefix = config["DumpOptions:FilePrefix"];
        if (!string.IsNullOrEmpty(prefix))
        {
            options.DumpOptions.FilePrefix = prefix;
        }
    }
}
