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
using System.Globalization;
using System.Text.Json;
using NewSage.Debug;
using NewSage.Logging;
using NewSage.Logging.DefaultSinks;
using NewSage.Profile;
using NewSage.Utilities;
using NewSage.Video;

namespace NewSage.Game;

public sealed class SageGame : IDisposable
{
    private const string SplashScreenName = "Install_Final.bmp";

    private readonly GameOptions _options;

    private FileStreamSink? _fileStreamSink;
    private Image? _loadScreenBitmap;

    private bool _disposed;

    public SageGame(string[] args, string configPath = "settings.json")
    {
        ArgumentNullException.ThrowIfNull(args);
        _options = LoadOptions(configPath);

        CommandLine.ApplyUserRuntimeOptions(args, _options);
        Log.MinimumLevel = _options.LogLevel;
        Log.AddSink(new ConsoleLogSink());
        if (_options.LogToFile)
        {
            _fileStreamSink = new FileStreamSink(_options.GameId);
            Log.AddSink(_fileStreamSink);
        }
    }

    public void Run() => Initialize();

    ~SageGame() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
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
            Log.Error($"Failed load settings from file '{path}'.\n{ex}");
            return new GameOptions();
        }
    }

    private void Initialize()
    {
        using var profiler = Profiler.Start("Game initialization", _options.EnableProfiling);
        UnhandledExceptionHandler.Install(_options.DumpOptions);

        _loadScreenBitmap = LoadSplashScreen();

        if (_loadScreenBitmap is { IsLoaded: true })
        {
            // Show the splash screen
        }

        // Start background initialization here

        // Then, once finished:
        _loadScreenBitmap?.Dispose();
        _loadScreenBitmap = null;

        Log.Debug($"Game initialized after {profiler.Elapsed.TotalMicroseconds}\u00b5s");
    }

    private Image? LoadSplashScreen()
    {
        try
        {
            using Stream? stream = ResourceLoader.GetEmbeddedStream(SplashScreenName);
            if (stream is not null)
            {
                var img = new Image();
                try
                {
                    img.Load(stream);
                    Log.Debug("Splash screen image loaded from embedded resources.");
                    return img;
                }
                catch
                {
                    img.Dispose();
                    throw;
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            Log.Error($"Failed to load the splash screen image from embedded resources.\n{ex}");
        }

        var paths = new[]
        {
            Path.Combine(
                _options.GameDirectory,
                "Data",
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                SplashScreenName
            ),
            Path.Combine(_options.GameDirectory, SplashScreenName),
        };

        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            var img = new Image();
            try
            {
                img.Load(path);
                Log.Debug($"Splash screen image loaded from '{path}'.");
                return img;
            }
            catch (InvalidOperationException ex)
            {
                img.Dispose();
                Log.Error($"Failed to load splash screen image from '{path}'.\n{ex}");
            }
        }

        return null;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _loadScreenBitmap?.Dispose();
            _loadScreenBitmap = null;

            _fileStreamSink?.Dispose();
            _fileStreamSink = null;
        }

        _disposed = true;
    }
}
