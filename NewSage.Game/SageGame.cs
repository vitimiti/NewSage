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
using NewSage.Game.Scenes;
using NewSage.Interop.NativeSdl;
using NewSage.Logging;
using NewSage.Logging.DefaultSinks;
using NewSage.Profile;

namespace NewSage.Game;

public sealed class SageGame : IDisposable
{
    private readonly GameOptions _options;

    private FileStreamSink? _fileStreamSink;
    private IScene? _scene;
    private bool _userRequestedQuit;
    private bool _disposed;

    public SageGame(string[] args, string configPath = "settings.json")
    {
        ArgumentNullException.ThrowIfNull(args);
        _options = LoadOptions(configPath);

        CommandLine.ApplyUserRuntimeOptions(args, _options);
        Log.MinimumLevel = _options.LogLevel;
        Log.AddSink(new ConsoleLogSink());
        if (!_options.LogToFile)
        {
            return;
        }

        _fileStreamSink = new FileStreamSink(_options.GameId);
        Log.AddSink(_fileStreamSink);
    }

    public void Run()
    {
        Initialize();
        while (!_userRequestedQuit)
        {
            Update();
            Draw();
        }
    }

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

    private static void InitializeSdl()
    {
        if (!Sdl.Init(Sdl.InitVideo))
        {
            throw new InvalidOperationException($"SDL video subsystem not initialized: {Sdl.GetError()}.");
        }
    }

    private void Initialize()
    {
        using var profiler = Profiler.Start("Game initialization", _options.EnableProfiling);
        _options.DumpOptions.DumpDirectory = Path.Combine(
            OperatingSystem.IsWindows()
                ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            _options.GameId,
            _options.DumpOptions.DumpDirectory
        );

        UnhandledExceptionHandler.Install(_options.DumpOptions);

        InitializeSdl();
        _scene = new SplashScene(_options);
        _scene.Initialize();

        if (profiler.Enabled)
        {
            Log.Debug($"{profiler.What}: {profiler.Elapsed.TotalMicroseconds}\u00b5s");
        }
    }

    private void Update()
    {
        while (Sdl.PollEvent(out Sdl.Event ev))
        {
            if (ev.Type is Sdl.EventType.Quit)
            {
                _userRequestedQuit = true;
            }
        }

        IScene? currentScene = _scene;
        if (currentScene is null)
        {
            return;
        }

        currentScene.Update();
        if (currentScene.NextScene is { } nextScene)
        {
            _scene = null;
            currentScene.Dispose();

            _scene = nextScene;
            _scene.Initialize();
        }
    }

    private void Draw() => _scene?.Draw();

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _scene?.Dispose();
            _scene = null;

            _fileStreamSink?.Dispose();
            _fileStreamSink = null;
        }

        Sdl.Quit();

        _disposed = true;
    }
}
