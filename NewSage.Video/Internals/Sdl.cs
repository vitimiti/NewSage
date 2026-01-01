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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace NewSage.Video.Internals;

internal static unsafe partial class Sdl
{
    public const uint InitVideo = 0x00000020;

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

    [NativeMarshalling(typeof(SafeHandleMarshaller<Surface>))]
    public sealed class Surface : SafeHandle
    {
        public Surface()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

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
            SetHandleAsInvalid();
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
            SetHandleAsInvalid();
            return result;
        }
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
