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
using NewSage.Debug;
using NewSage.Profile;

namespace NewSage.Game;

internal sealed class SageGame : IDisposable
{
    private readonly GameOptions _options;

    private bool _disposed;

    public SageGame(string[] args, string configPath = "settings.json")
    {
        _options = LoadOptions(configPath);

        CommandLine.ApplyUserRuntimeOptions(args, _options);
    }

    public void Run() => Initialize();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

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

    private void Initialize()
    {
        using var profiler = Profiler.Start("Game initialization", _options.EnableProfiling);
        UnhandledExceptionHandler.Install(_options.DumpOptions);
    }
}
