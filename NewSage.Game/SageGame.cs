// -----------------------------------------------------------------------
// <copyright file="SageGame.cs" company="NewSage">
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
using System.Text.Json;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Extensions.Configuration;
using NewSage.Debug;
using NewSage.Profile;

namespace NewSage.Game;

internal sealed class SageGame : IDisposable
{
    private readonly string[] _args;
    private readonly GameOptions _options;

    private Profiler? _profiler;
    private bool _disposed;

    public SageGame(string[] args, string configPath = "settings.json")
    {
        _args = args;
        _options = LoadOptions(configPath);

        ApplyCommandLineOverrides();
    }

    public void Run() => Initialize();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _profiler?.Dispose();

        _disposed = true;
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Do not throw during settings loading, we have defaults."
    )]
    private static GameOptions LoadOptions(string path)
    {
        if (!File.Exists(path))
        {
            return new GameOptions();
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize(json, GameJsonContext.Default.GameOptions) ?? new GameOptions();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load settings from file '{path}'.\n{ex}");
            return new GameOptions();
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Do not throw before initialization actually starts."
    )]
    private void ApplyCommandLineOverrides()
    {
        if (_args.Length == 0)
        {
            return;
        }

        var switchMappings = new Dictionary<string, string>
        {
            { "--profile", "EnableProfiling" },
            { "--dump", "DumpOptions:Enabled" },
            { "--dump-dir", "DumpOptions:DumpDirectory" },
            { "--dump-type", "DumpOptions:DumpType" },
            { "--dump-max", "DumpOptions:MaxDumpFiles" },
            { "--dump-prefix", "DumpOptions:FilePrefix" },
            { "--working-dir", "WorkingDirectory" },
        };

        var builder = new ConfigurationBuilder();
        _ = builder.AddCommandLine(_args, switchMappings);

        IConfigurationRoot config = builder.Build();

        if (bool.TryParse(config["EnableProfiling"], out var profile))
        {
            _options.EnableProfiling = profile;
        }

        if (bool.TryParse(config["DumpOptions:Enabled"], out var dump))
        {
            _options.DumpOptions.Enabled = dump;
        }

        var dir = config["DumpOptions:DumpDirectory"];
        if (!string.IsNullOrEmpty(dir))
        {
            _options.DumpOptions.DumpDirectory = dir;
        }

        if (Enum.TryParse(config["DumpOptions:DumpType"], ignoreCase: true, out DumpType dumpType))
        {
            _options.DumpOptions.DumpType = dumpType;
        }

        if (uint.TryParse(config["DumpOptions:MaxDumpFiles"], out var maxFiles))
        {
            _options.DumpOptions.MaxDumpFiles = maxFiles;
        }

        var prefix = config["DumpOptions:FilePrefix"];
        if (!string.IsNullOrEmpty(prefix))
        {
            _options.DumpOptions.FilePrefix = prefix;
        }

        var workingDir = config["WorkingDirectory"];
        if (!string.IsNullOrEmpty(workingDir))
        {
            try
            {
                Directory.SetCurrentDirectory(workingDir);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to set working directory to '{workingDir}'.\n{ex.Message}");
            }
        }
    }

    private void Initialize()
    {
        if (_options.EnableProfiling)
        {
            _profiler = Profiler.Start($"{nameof(SageGame)}.{nameof(Initialize)}");
        }

        UnhandledExceptionHandler.Install(_options.DumpOptions);

        _profiler?.Dispose();
    }
}
