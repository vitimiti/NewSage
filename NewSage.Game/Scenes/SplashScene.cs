// -----------------------------------------------------------------------
// <copyright file="SplashScene.cs" company="NewSage">
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using NewSage.Game.Ini;
using NewSage.Game.NameKeys;
using NewSage.Game.Subsystems;
using NewSage.Game.Transfer;
using NewSage.Interop.NativeSdl;
using NewSage.Logging;
using NewSage.Profile;
using NewSage.Utilities;

namespace NewSage.Game.Scenes;

internal sealed class SplashScene(GameOptions options) : IScene
{
    private const string SplashScreenName = "Install_Final.bmp";

    private Sdl.Window? _window;
    private Sdl.Surface? _splashBmp;
    private bool _disposed;

    public IScene? NextScene { get; private set; }

    public void Initialize()
    {
        InitializeSplashImage();
        InitializeWindow();
        _ = Task.Run(BackgroundInitialize);
    }

    public void Update() { }

    public void Draw()
    {
        if (_window is null || _window.IsInvalid || _splashBmp is null || _splashBmp.IsInvalid)
        {
            return;
        }

        // Get the current window surface. This handle is managed by SDL, so we don't dispose it.
        using Sdl.Surface target = Sdl.GetWindowSurface(_window);
        if (!target.IsInvalid)
        {
            // Perform the blit
            _ = Sdl.BlitSurface(_splashBmp, target);

            // Present the surface to the window
            _ = Sdl.UpdateWindowSurface(_window);
        }
    }

    ~SplashScene() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static T InitializeSubsystem<T>(
        string name,
        Func<T> factory,
        TransferService? transfer,
        string? path1 = null,
        string? path2 = null
    )
        where T : SubsystemBase
    {
        T sys = factory();
        SubsystemList.TheSubsystemList!.InitializeSubsystem(sys, path1, path2, transfer, name);
        return sys;
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Initialization can continue without a splash screen."
    )]
    private void InitializeSplashImageFromEmbeddedResource()
    {
        using Stream? stream = ResourceLoader.GetEmbeddedStream(SplashScreenName);
        if (stream is null)
        {
            return;
        }

        var length = (int)stream.Length;
        var nativeBuffer = Marshal.AllocHGlobal(length);

        try
        {
            unsafe
            {
                using var unmanagedStream = new UnmanagedMemoryStream(
                    (byte*)nativeBuffer,
                    length,
                    length,
                    FileAccess.Write
                );

                stream.CopyTo(unmanagedStream);
                using Sdl.IoStream ioStream = Sdl.IoFromConstMem(new ReadOnlySpan<byte>((byte*)nativeBuffer, length));
                if (ioStream.IsInvalid)
                {
                    Log.Error($"Failed to create SDL IO stream: {Sdl.GetError()}.");
                    Marshal.FreeHGlobal(nativeBuffer);
                    return;
                }

                _splashBmp = SdlImage.Load(ioStream, closeIo: false);
                Marshal.FreeHGlobal(nativeBuffer);
                nativeBuffer = nint.Zero;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load splash screen.\n{ex}");
            if (nativeBuffer != nint.Zero)
            {
                Marshal.FreeHGlobal(nativeBuffer);
            }
        }
    }

    private void InitializeSplashImageFromLocalFile()
    {
        var paths = new[]
        {
            Path.Combine(
                options.GameDirectory,
                "Data",
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                SplashScreenName
            ),
            Path.Combine(options.GameDirectory, SplashScreenName),
        };

        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            _splashBmp = SdlImage.Load(path);
            if (!_splashBmp.IsInvalid)
            {
                return;
            }

            Log.Error($"Failed to load splash screen image from '{path}': {Sdl.GetError()}.");
        }
    }

    private void InitializeSplashImage()
    {
        InitializeSplashImageFromEmbeddedResource();
        if (_splashBmp is { IsInvalid: false })
        {
            return;
        }

        InitializeSplashImageFromLocalFile();
    }

    private void InitializeWindow()
    {
        if (_splashBmp is not { IsInvalid: false })
        {
            return;
        }

        _window = Sdl.CreateWindow(
            $"{options.GameTitle} - Loading...",
            _splashBmp.Width,
            _splashBmp.Height,
            Sdl.WindowBorderless
        );

        _ = Sdl.ShowWindow(_window);
    }

    private void BackgroundInitialize()
    {
        using var profiler = Profiler.Start("Game Initialization", options.EnableProfiling);
        using var ini = new IniParser();

        VersionInformation.LogVersionHeader();

        Log.Information("Initializing the subsystems list...");
        SubsystemList.TheSubsystemList = new SubsystemList(options);

        Log.Information("Initializing the name key generator...");
        NameKeyGenerator.TheNameKeyGenerator = new NameKeyGenerator(options);
        NameKeyGenerator.TheNameKeyGenerator.Initialize();

        Log.Information("Opening the light CRC transfer service...");
        var transferCrc = new TransferCrcService();
        transferCrc.Open("lightCRC");

        Log.Information("Initializing the archive file system...");
        ArchiveFileSystem.TheArchiveFileSystem = InitializeSubsystem(
            "TheArchiveFileSystem",
            () => new ArchiveFileSystem(options),
            null
        );

        Debug.Assert(GlobalData.TheWritableGlobalData is not null, "Global data is not initialized.");
        GlobalData.TheWritableGlobalData = InitializeSubsystem(
            "TheWritableGlobalData",
            () => GlobalData.TheWritableGlobalData,
            transferCrc,
            Path.Combine(options.GameDirectory, "Data", "INI", "Default", "GameData"),
            Path.Combine(options.GameDirectory, "Data", "INI", "GameData")
        );

        GlobalData.TheWritableGlobalData.ParseCustomDefinition();

#if DEBUG
        ini.LoadFileDirectory(
            Path.Combine(options.GameDirectory, "Data", "INI", "GameDataDebug"),
            IniLoadType.Overwrite,
            null
        );
#endif

        transferCrc.Close();
        Log.Debug($"Light CRC result: 0x{transferCrc.Crc:X8}");

        Log.Information("Post-processing all subsystems...");
        SubsystemList.TheSubsystemList.PostProcessLoadAll();

        // TODO: Change this for the next scene conversion.
        Log.Information("Game initialization completed.");
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _splashBmp?.Dispose();
            _splashBmp = null;

            _window?.Dispose();
            _window = null;
        }

        _disposed = true;
    }
}
