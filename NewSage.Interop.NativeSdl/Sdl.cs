// -----------------------------------------------------------------------
// <copyright file="Sdl.cs" company="NewSage">
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace NewSage.Interop.NativeSdl;

[SuppressMessage(
    "csharpsquid",
    "S4200:Native methods should be wrapped",
    Justification = "This is an unsafe, interop library for internal use."
)]
public static unsafe partial class Sdl
{
    public const uint InitVideo = 0x0000_0020;

    public const ulong WindowBorderless = 0x0000_0000_0000_0010;

    [LibraryImport("SDL3", EntryPoint = "SDL_Init")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static partial bool Init(uint flags);

    [LibraryImport("SDL3", EntryPoint = "SDL_QuitSubSystem")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static partial void QuitSubSystem(uint flags);

    public static string GetError() => Utf8StringMarshaller.ConvertToManaged(UnsafeGetError()) ?? string.Empty;

    [LibraryImport("SDL3", EntryPoint = "SDL_WasInit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static partial uint WasInit(uint flags);

    [LibraryImport("SDL3", EntryPoint = "SDL_Quit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static partial void Quit();

    public static IoStream IoFromConstMem(ReadOnlySpan<byte> data) =>
        UnsafeIoFromConstMem((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)), (nuint)data.Length);

    [LibraryImport("SDL3", EntryPoint = "SDL_CreateWindow", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static partial Window CreateWindow(string title, int width, int height, ulong flags);

    [LibraryImport("SDL3", EntryPoint = "SDL_PollEvent")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool PollEvent(out Event ev);

    public static Surface GetWindowSurface(Window window)
    {
        var handle = UnsafeGetWindowSurface(window);
        return new Surface(handle, ownsHandle: false);
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_UpdateWindowSurface")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UpdateWindowSurface(Window window);

    public static bool BlitSurface(Surface src, Surface dst)
    {
        Rect* srcRect = null;
        Rect* dstRect = null;
        return UnsafeBlitSurface(src, srcRect, dst, dstRect);
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_ShowWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ShowWindow(Window window);

    [LibraryImport("SDL3", EntryPoint = "SDL_GetError")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial byte* UnsafeGetError();

    [LibraryImport("SDL3", EntryPoint = "SDL_DestroySurface")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial void DestroySurface(nint surface);

    [LibraryImport("SDL3", EntryPoint = "SDL_CloseIO")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool CloseIo(nint io);

    [LibraryImport("SDL3", EntryPoint = "SDL_IOFromConstMem")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial IoStream UnsafeIoFromConstMem(byte* data, nuint size);

    [LibraryImport("SDL3", EntryPoint = "SDL_DestroyWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial void DestroyWindow(nint window);

    [LibraryImport("SDL3", EntryPoint = "SDL_GetWindowSurface")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial nint UnsafeGetWindowSurface(Window window);

    [LibraryImport("SDL3", EntryPoint = "SDL_BlitSurface")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I4)]
    private static partial bool UnsafeBlitSurface(Surface src, Rect* srcRect, Surface dst, Rect* dstRect);

    public enum EventType : uint
    {
        First,
        Quit = 0x0100,
    }

    [NativeMarshalling(typeof(SafeHandleMarshaller<Surface>))]
    public sealed class Surface : SafeHandle
    {
        public Surface()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

        public Surface(nint preexistingHandle, bool ownsHandle = true)
            : base(invalidHandleValue: nint.Zero, ownsHandle) => SetHandle(preexistingHandle);

        public override bool IsInvalid => handle == nint.Zero;

        public int Width => IsInvalid ? -1 : UnsafeHandle->W;

        public int Height => IsInvalid ? -1 : UnsafeHandle->H;

        private UnsafeSurface* UnsafeHandle => (UnsafeSurface*)handle;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            DestroySurface(handle);
            return true;
        }
    }

    [NativeMarshalling(typeof(SafeHandleMarshaller<IoStream>))]
    public sealed class IoStream : SafeHandle
    {
        public IoStream()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

        public override bool IsInvalid => handle == nint.Zero;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            var result = CloseIo(handle);
            return result;
        }
    }

    [NativeMarshalling(typeof(SafeHandleMarshaller<Window>))]
    public sealed class Window : SafeHandle
    {
        public Window()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

        public override bool IsInvalid => handle == nint.Zero;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            DestroyWindow(handle);
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect : IEquatable<Rect>
    {
        public int X;
        public int Y;
        public int W;
        public int H;

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Rect other && Equals(other);

        public readonly bool Equals(Rect other) => X == other.X && Y == other.Y && W == other.W && H == other.H;

        public override readonly int GetHashCode() => HashCode.Combine(X, Y, W, H);

        public static bool operator ==(Rect left, Rect right) => left.Equals(right);

        public static bool operator !=(Rect left, Rect right) => !left.Equals(right);
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Event : IEquatable<Event>
    {
        [FieldOffset(0)]
        public EventType Type;

        [FieldOffset(0)]
        public fixed byte Padding[128];

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Event other && Equals(other);

        public readonly bool Equals(Event other) => Type == other.Type;

        public override readonly int GetHashCode() => HashCode.Combine(Type);

        public static bool operator ==(Event left, Event right) => left.Equals(right);

        public static bool operator !=(Event left, Event right) => !left.Equals(right);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UnsafeSurface
    {
        public uint Flags;
        public uint Format;
        public int W;
        public int H;
        public int Pitch;
        public void* Pixels;
        public int RefCount;
        public void* Reserved;
    }
}
