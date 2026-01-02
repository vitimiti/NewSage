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

using System.Diagnostics;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Extensions.Configuration;
using NewSage.Game.Subsystems;
using NewSage.Logging;

namespace NewSage.Game;

internal static class CommandLine
{
    private static readonly Dictionary<string, string> GameOptionsSwitchMappings = new()
    {
        { "--log-level", "LogLevel" },
        { "--dump-dir", "DumpOptions:DumpDirectory" },
        { "--dump-type", "DumpOptions:DumpType" },
        { "--dump-max", "DumpOptions:MaxDumpFiles" },
        { "--dump-prefix", "DumpOptions:FilePrefix" },
        { "--working-dir", "WorkingDirectory" },
        { "--game-dir", "GameDirectory" },
        { "--game-id", "GameId" },
        { "--game-title", "GameTitle" },
    };

    public static void ApplyUserGameOptions(string[] args, GameOptions options)
    {
        if (args.Length == 0)
        {
            return;
        }

        var builder = new ConfigurationBuilder();
        _ = builder.AddCommandLine(args, GameOptionsSwitchMappings);

        IConfigurationRoot config = builder.Build();

        UserGameOptionsParser.ParseDirectOptions(args, options);
        UserGameOptionsParser.ParseDumpOptions(config, options);
        UserGameOptionsParser.ParseLoggingOptions(config, options);
        UserGameOptionsParser.ParseGameDataOptions(config, options);
    }

    public static void ApplyUserGlobalData(string[] args)
    {
        if (args.Length == 0)
        {
            return;
        }

        Debug.Assert(GlobalData.TheWritableGlobalData is not null, "The global data is not initialized.");

        UserGlobalDataOptionsParser.ParseDirectOptions(args, GlobalData.TheWritableGlobalData);

#if DEBUG
        UserGlobalDataOptionsParser.ParseDebugDirectOptions(args, GlobalData.TheWritableGlobalData);
#endif
    }

    private static class UserGameOptionsParser
    {
        public static void ParseDirectOptions(string[] args, GameOptions options)
        {
            if (args.Contains("--profile"))
            {
                options.EnableProfiling = true;
            }

            if (args.Contains("--dump"))
            {
                options.DumpOptions.Enabled = true;
            }

            if (args.Contains("--log-to-file"))
            {
                options.LogToFile = true;
            }
        }

        public static void ParseDumpOptions(IConfigurationRoot config, GameOptions options)
        {
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

        public static void ParseLoggingOptions(IConfigurationRoot config, GameOptions options)
        {
            if (Enum.TryParse(config["LogLevel"], ignoreCase: true, out LogLevel logLevel))
            {
                options.LogLevel = logLevel;
            }
        }

        public static void ParseGameDataOptions(IConfigurationRoot config, GameOptions options)
        {
            var dir = config["GameDirectory"];
            if (!string.IsNullOrEmpty(dir))
            {
                options.GameDirectory = dir;
            }

            var id = config["GameId"];
            if (!string.IsNullOrEmpty(id))
            {
                options.GameId = id;
            }

            var title = config["GameTitle"];
            if (!string.IsNullOrEmpty(title))
            {
                options.GameTitle = title;
            }
        }
    }

    private static class UserGlobalDataOptionsParser
    {
        public static void ParseDirectOptions(string[] args, GlobalData globalData)
        {
            if (args.Contains("--win"))
            {
                globalData.Windowed = true;
            }

            if (args.Contains("--no-music"))
            {
                globalData.MusicOn = false;
            }

            if (args.Contains("--no-video"))
            {
                globalData.VideoOn = false;
            }

            if (args.Contains("--use-wave-editor"))
            {
                globalData.UsingWaterTrackEditor = true;
            }

            if (args.Contains("--full-viewport"))
            {
                globalData.ViewportHeightScale = 1F;
            }

            if (args.Contains("--use-csf"))
            {
                globalData.UseStringFile = true;
            }
        }

#if DEBUG
        public static void ParseDebugDirectOptions(string[] args, GlobalData globalData)
        {
            if (args.Contains("--no-input-disable"))
            {
                globalData.DisableScriptedInputDisabling = true;
            }

            if (args.Contains("--no-fade"))
            {
                globalData.DisableCameraFade = true;
            }

            if (args.Contains("--no-mil-cap"))
            {
                globalData.DisableMilitaryCaption = true;
            }
        }
#endif
    }
}
