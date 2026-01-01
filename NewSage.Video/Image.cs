// -----------------------------------------------------------------------
// <copyright file="Image.cs" company="NewSage">
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

using System.Text;
using NewSage.Video.Internals;

namespace NewSage.Video;

public sealed class Image : IDisposable
{
    private static readonly Lock Lock = new();

    private static int _refCount;

    private Sdl.Surface? _imageHandle;

    private bool _disposed;

    public Image()
    {
        lock (Lock)
        {
            if (_refCount == 0 && !Sdl.Init(Sdl.InitVideo))
            {
                throw new InvalidOperationException($"Unable to initialize SDL video: {Sdl.GetError()}");
            }

            _ = Interlocked.Increment(ref _refCount);
        }
    }

    public bool IsLoaded { get; private set; }

    public int Width => _imageHandle is { } surface ? surface.Width : -1;

    public int Height => _imageHandle is { } surface ? surface.Height : -1;

    public void Load(string path)
    {
        _imageHandle?.Dispose();
        _imageHandle = SdlImage.Load(path);
        if (_imageHandle.IsInvalid)
        {
            throw new InvalidOperationException($"Unable to load image from '{path}': {Sdl.GetError()}");
        }

        IsLoaded = true;
    }

    public void Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        _imageHandle?.Dispose();
        using var br = new BinaryReader(stream, Encoding.Default, leaveOpen: true);
        using Sdl.IoStream io = Sdl.IoFromConstMem(br.ReadBytes((int)stream.Length));
        if (io.IsInvalid)
        {
            throw new InvalidOperationException($"Unable to load image from stream: {Sdl.GetError()}");
        }

        _imageHandle = SdlImage.Load(io, closeIo: false);
        if (_imageHandle.IsInvalid)
        {
            throw new InvalidOperationException($"Unable to load image from stream: {Sdl.GetError()}");
        }

        IsLoaded = true;
    }

    ~Image() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static void ReleaseUnmanagedResources()
    {
        if (Interlocked.Decrement(ref _refCount) != 0)
        {
            return;
        }

        Sdl.QuitSubSystem(Sdl.InitVideo);

        // Only quit SDL entirely if no other subsystems are active
        if (Sdl.WasInit(0) == 0)
        {
            Sdl.Quit();
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        ReleaseUnmanagedResources();
        if (disposing)
        {
            _imageHandle?.Dispose();
            _imageHandle = null;
        }

        IsLoaded = false;
        _disposed = true;
    }
}
