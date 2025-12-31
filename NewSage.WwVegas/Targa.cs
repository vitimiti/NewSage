// -----------------------------------------------------------------------
// <copyright file="Targa.cs" company="NewSage">
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
using System.Runtime.InteropServices;
using System.Text;
using NewSage.WwVegas.Exceptions;

namespace NewSage.WwVegas;

public class Targa : IDisposable
{
    public const string Tga2Signature = "TRUEVISION-XFILE";

    private bool _disposed;

    public TgaHeader Header { get; set; }

    public bool IsCompressed => Header.ImageType > 8;

    protected FileStream? TgaFile { get; set; }

    protected FileAccess Access { get; set; }

    protected TgaOptions Flags { get; set; }

    protected Tga2Extension Extension { get; set; }

    protected Memory<byte> Image { get; set; }

    protected Memory<byte> Palette { get; set; }

    public Targa()
    {
        ClearFile();
        Access = FileAccess.Read;
        Flags = 0;
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "The opening of TGA files is complex, but I don't see splitting its implementation to be a benefit."
    )]
    public void Open(string name, FileAccess mode)
    {
        if (IsFileOpen() && Access == mode)
        {
            return;
        }

        Close();

        Access = mode;
        Flags &= ~TgaOptions.Tga2;

        try
        {
            switch (mode)
            {
                case FileAccess.Read:
                    if (FileOpenRead(name))
                    {
                        if (FileSeek(-26, SeekOrigin.End) != -1)
                        {
                            var footerBytes = new byte[26];
                            if (FileRead(footerBytes) == 26)
                            {
                                var sig = Encoding.ASCII.GetString(footerBytes, 8, 16);
                                if (sig.Contains(Tga2Signature, StringComparison.Ordinal))
                                {
                                    var extOffset = BitConverter.ToInt32(footerBytes, 0);
                                    if (extOffset != 0)
                                    {
                                        Flags |= TgaOptions.Tga2;
                                    }

                                    if ((Flags & TgaOptions.Tga2) != 0)
                                    {
                                        _ = FileSeek(extOffset, SeekOrigin.Begin);
                                        Span<byte> extBytes = stackalloc byte[Marshal.SizeOf<Tga2Extension>()];
                                        _ = FileRead(extBytes);
                                        Extension = MemoryMarshal.AsRef<Tga2Extension>(extBytes);
                                    }
                                }
                            }
                        }

                        _ = FileSeek(0, SeekOrigin.Begin);
                        Span<byte> headerBytes = stackalloc byte[Marshal.SizeOf<TgaHeader>()];
                        if (FileRead(headerBytes) != headerBytes.Length)
                        {
                            throw new TgaReadException("Unable to read TGA header");
                        }

                        Header = MemoryMarshal.AsRef<TgaHeader>(headerBytes);
                        if (Header.IDLength != 0)
                        {
                            _ = FileSeek(Header.IDLength, SeekOrigin.Current);
                        }
                    }
                    else
                    {
                        throw new TgaOpenException($"Unable to open '{name}' for reading");
                    }

                    break;

                case FileAccess.Write:
                    if (!FileOpenWrite(name))
                    {
                        throw new TgaOpenException($"Unable to open '{name}' for writing");
                    }

                    break;

                case FileAccess.ReadWrite:
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            throw new TgaOpenException($"Unable to open '{name}' for {mode}", ex);
        }
        finally
        {
            Close();
        }
    }

    public void Close()
    {
        TgaFile?.Dispose();
        TgaFile = null;
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "The loading of TGA files is complex, but I don't see splitting its implementation to be a benefit."
    )]
    public void Load(string name, byte[]? paletteBuf, byte[]? imageBuf, bool invertImage = true)
    {
        try
        {
            Open(name, FileAccess.Read);
        }
        catch (TgaException ex)
        {
            throw new TgaLoadException($"Unable to load '{name}'", ex);
        }

        try
        {
            int size;
            if (Header.ColorMapType == 1)
            {
                var depth = Header.CMapDepth >> 3;
                size = Header.CMapLength * depth;
                if (paletteBuf is not null && Header.CMapLength > 0)
                {
                    if (FileRead(paletteBuf.AsSpan(0, size)) != size)
                    {
                        throw new TgaReadException("Unable to read palette");
                    }
                }
                else
                {
                    _ = FileSeek(size, SeekOrigin.Current);
                }
            }

            if (imageBuf is null)
            {
                return;
            }

            var bpp = (Header.PixelDepth + 7) >> 3;
            size = Header.Width * Header.Height * bpp;
            Image = imageBuf;

            switch (Header.ImageType)
            {
                case 1: // CMAPPED
                case 3: // MONO
                    if (FileRead(imageBuf) != size)
                    {
                        throw new TgaReadException("Unable to read cmapped/mono image");
                    }

                    break;
                case 2: // TRUECOLOR
                    if (FileRead(imageBuf) == size)
                    {
                        if (invertImage)
                        {
                            InvertImage();
                        }
                    }
                    else
                    {
                        throw new TgaReadException("Unable to read true color image");
                    }

                    break;
                case 9: // CMAPPED_ENCODED
                    try
                    {
                        DecodeImage();
                    }
                    catch (TgaException ex)
                    {
                        throw new TgaDecodeException("Unable to decode cmapped encoded image", ex);
                    }

                    break;
                case 10: // TRUECOLOR_ENCODED
                    try
                    {
                        DecodeImage();
                    }
                    catch (TgaException ex)
                    {
                        throw new TgaDecodeException("Unable to decode true color encoded image", ex);
                    }

                    if (invertImage)
                    {
                        InvertImage();
                    }

                    break;
                default:
                    throw new TgaNotSupportedException($"Unsupported image type {Header.ImageType}");
            }

            if ((Header.ImageDescriptor & 0x10) != 0)
            {
                XFlip();
            }

            if ((Header.ImageDescriptor & 0x20) != 0)
            {
                YFlip();
            }
        }
        finally
        {
            Close();
        }
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "The loading of TGA files is complex, but I don't see splitting its implementation to be a benefit."
    )]
    public void Load(string name, TgaOptions flags, bool invertImage = true)
    {
        try
        {
            Open(name, FileAccess.Read);
        }
        catch (TgaException ex)
        {
            throw new TgaLoadException($"Unable to load '{name}'", ex);
        }

        try
        {
            if (flags.HasFlag(TgaOptions.Pal) && Header.ColorMapType == 1)
            {
                if (!Palette.IsEmpty && (Flags & TgaOptions.Pal) != 0)
                {
                    Palette = default;
                }

                var size = Header.CMapLength * (Header.CMapDepth >> 3);
                if (size > 0)
                {
                    Palette = new byte[size];
                    Flags |= TgaOptions.Pal;
                }
            }

            if (flags.HasFlag(TgaOptions.Image))
            {
                if (!Image.IsEmpty && (Flags & TgaOptions.Image) != 0)
                {
                    Image = default;
                }

                var bpp = (Header.PixelDepth + 7) >> 3;
                var size = Header.Width * Header.Height * bpp;
                if (size > 0)
                {
                    Image = new byte[size];
                    Flags |= TgaOptions.Image;
                }
            }

            try
            {
                Load(
                    name,
                    Palette.IsEmpty ? null : Palette.ToArray(),
                    Image.IsEmpty ? null : Image.ToArray(),
                    invertImage
                );
            }
            catch (TgaException ex)
            {
                throw new TgaLoadException($"Unable to load '{name}'", ex);
            }
        }
        finally
        {
            Close();
        }
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "The saving of TGA files is complex, but I don't see splitting its implementation to be a benefit."
    )]
    public void Save(string name, TgaOptions flags, bool addExtension = false)
    {
        try
        {
            Open(name, FileAccess.Write);
        }
        catch (TgaException ex)
        {
            throw new TgaSaveException($"Unable to save '{name}'", ex);
        }

        TgaHeader header = Header;

        try
        {
            header.IDLength = 0;
            if (flags.HasFlag(TgaOptions.Compress) && Header.ImageType is 1 or 2 or 3)
            {
                header.ImageType += 8;
            }

            Span<byte> hBuf = stackalloc byte[Marshal.SizeOf<TgaHeader>()];
            MemoryMarshal.Write(hBuf, header);
            if (FileWrite(hBuf) != hBuf.Length)
            {
                throw new TgaWriteException("Unable to write TGA header");
            }

            if (flags.HasFlag(TgaOptions.Pal) && !Palette.IsEmpty && Header.CMapLength > 0)
            {
                var size = Header.CMapLength * (Header.CMapDepth >> 3);
                if (FileWrite(Palette.ToArray().AsSpan(0, size)) != size)
                {
                    throw new TgaWriteException("Unable to write palette");
                }
            }

            if (flags.HasFlag(TgaOptions.Image) && !Image.IsEmpty)
            {
                var wasInverted = false;
                if (Header.ImageType is 2 or 10)
                {
                    InvertImage();
                    wasInverted = true;
                }

                if (flags.HasFlag(TgaOptions.Compress))
                {
                    try
                    {
                        EncodeImage();
                    }
                    catch (TgaException ex)
                    {
                        throw new TgaEncodeException("Unable to encode image", ex);
                    }
                }
                else
                {
                    var bpp = (Header.PixelDepth + 7) >> 3;
                    var size = Header.Width * Header.Height * bpp;
                    if (FileWrite(Image.ToArray().AsSpan(0, size)) != size)
                    {
                        throw new TgaWriteException("Unable to write image");
                    }
                }

                if (wasInverted)
                {
                    InvertImage();
                }
            }

            Tga2Footer footer = new()
            {
                Signature = Encoding.ASCII.GetBytes(Tga2Signature),
                RsvdChar = (byte)'.',
                BinaryZeroTerminator = 0,
            };

            if (addExtension)
            {
                footer.Extension = (int)TgaFile!.Position;
                Span<byte> extBytes = stackalloc byte[Marshal.SizeOf<Tga2Extension>()];
                MemoryMarshal.Write(extBytes, Extension);
                _ = FileWrite(extBytes);
            }

            Span<byte> footerBytes = stackalloc byte[Marshal.SizeOf<Tga2Footer>()];
            MemoryMarshal.Write(footerBytes, footer);
            _ = FileWrite(footerBytes);
        }
        finally
        {
            Header = header;
            Close();
        }
    }

    public void XFlip()
    {
        if (Image.IsEmpty)
        {
            return;
        }

        var bpp = (Header.PixelDepth + 7) >> 3;
        var stride = Header.Width * bpp;
        for (var y = 0; y < Header.Height; y++)
        {
            Span<byte> row = Image.Span.Slice(y * stride, stride);
            for (var x = 0; x < Header.Width / 2; x++)
            {
                Span<byte> left = row.Slice(x * bpp, bpp);
                Span<byte> right = row.Slice((Header.Width - 1 - x) * bpp, bpp);
                for (var b = 0; b < bpp; b++)
                {
                    (left[b], right[b]) = (right[b], left[b]);
                }
            }
        }
    }

    public void YFlip()
    {
        if (Image.IsEmpty)
        {
            return;
        }

        var stride = Header.Width * ((Header.PixelDepth + 7) >> 3);
        var temp = new byte[stride];
        for (var y = 0; y < Header.Height / 2; y++)
        {
            Span<byte> top = Image.Span.Slice(y * stride, stride);
            Span<byte> bottom = Image.Span.Slice((Header.Height - 1 - y) * stride, stride);
            top.CopyTo(temp);
            bottom.CopyTo(top);
            temp.CopyTo(bottom);
        }
    }

    public byte[]? SetImage(byte[]? buffer)
    {
        var old = Image.ToArray();
        Image = buffer;
        return old;
    }

    public byte[]? SetPalette(byte[]? buffer)
    {
        var old = Palette.ToArray();
        Palette = buffer;
        return old;
    }

    public Tga2Extension? TryGetExtension() => Flags.HasFlag(TgaOptions.Tga2) ? Extension : null;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Close();
            Image = default;
            Palette = default;
        }

        _disposed = true;
    }

    private void DecodeImage()
    {
        if (Image.IsEmpty)
        {
            throw new TgaNoDataException("Image buffer is empty");
        }

        var bpp = (Header.PixelDepth + 7) >> 3;
        var pixelCount = Header.Width * Header.Height;
        var current = 0;
        Span<byte> pixel = stackalloc byte[bpp];
        Span<byte> imageSpan = Image.Span;

        while (current < pixelCount)
        {
            var control = TgaFile!.ReadByte();
            if (control == -1)
            {
                throw new TgaReadException("Unable to read TGA image data");
            }

            var count = (control & 0x7F) + 1;
            if ((control & 0x80) != 0)
            {
                _ = FileRead(pixel);
                for (var i = 0; i < count; i++)
                {
                    pixel.CopyTo(imageSpan.Slice((current + i) * bpp, bpp));
                }
            }
            else
            {
                _ = FileRead(imageSpan.Slice(current * bpp, count * bpp));
            }

            current += count;
        }
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "The encoding of TGA files is complex, but I don't see splitting its implementation to be a benefit."
    )]
    private void EncodeImage()
    {
        if (Image.IsEmpty)
        {
            throw new TgaNoDataException("Image buffer is empty");
        }

        var bpp = (Header.PixelDepth + 7) >> 3;
        var totalPixels = Header.Width * Header.Height;
        var currentPixel = 0;
        ReadOnlySpan<byte> imageSpan = Image.Span;

        var packet = new byte[(128 * bpp) + 1];
        while (currentPixel < totalPixels)
        {
            var runCount = 1;
            var isRun = false;
            if (currentPixel + 1 < totalPixels)
            {
                ReadOnlySpan<byte> firstPixel = imageSpan.Slice(currentPixel * bpp, bpp);
                while (currentPixel + runCount < totalPixels && runCount < 128)
                {
                    ReadOnlySpan<byte> nextPixel = imageSpan.Slice((currentPixel + runCount) * bpp, bpp);
                    if (!firstPixel.SequenceEqual(nextPixel))
                    {
                        break;
                    }

                    runCount++;
                }

                if (runCount > 1)
                {
                    isRun = true;
                }
            }

            if (isRun)
            {
                packet[0] = (byte)(0x80 | (runCount - 1));
                imageSpan.Slice(currentPixel * bpp, bpp).CopyTo(packet.AsSpan(1));

                if (FileWrite(packet.AsSpan(0, bpp + 1)) != bpp + 1)
                {
                    throw new TgaWriteException("Unable to write TGA image data");
                }
            }
            else
            {
                runCount = 1;
                while (currentPixel + runCount < totalPixels && runCount < 128)
                {
                    if (currentPixel + runCount + 1 < totalPixels)
                    {
                        ReadOnlySpan<byte> p1 = imageSpan.Slice((currentPixel + runCount) * bpp, bpp);
                        ReadOnlySpan<byte> p2 = imageSpan.Slice((currentPixel + runCount + 1) * bpp, bpp);
                        if (p1.SequenceEqual(p2))
                        {
                            break;
                        }
                    }

                    runCount++;
                }

                packet[0] = (byte)(runCount - 1);
                imageSpan.Slice(currentPixel * bpp, runCount * bpp).CopyTo(packet.AsSpan(1));

                if (FileWrite(packet.AsSpan(0, (runCount * bpp) + 1)) != (runCount * bpp) + 1)
                {
                    throw new TgaWriteException("Unable to write TGA image data");
                }
            }

            currentPixel += runCount;
        }
    }

    private void InvertImage()
    {
        if (Image.IsEmpty || Header.PixelDepth < 24)
        {
            return;
        }

        Span<byte> imageData = Image.Span;
        var bpp = Header.PixelDepth >> 3;
        for (var i = 0; i < Image.Length; i += bpp)
        {
            (imageData[i], imageData[i + 2]) = (imageData[i + 2], imageData[i]);
        }
    }

    private void ClearFile() => TgaFile = null;

    private bool IsFileOpen() => TgaFile is not null;

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Silent failure on file errors is intentional."
    )]
    private bool FileOpenRead(string n)
    {
        try
        {
            TgaFile = new FileStream(n, FileMode.Open, FileAccess.Read);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Silent failure on file errors is intentional."
    )]
    private bool FileOpenWrite(string n)
    {
        try
        {
            TgaFile = new FileStream(n, FileMode.Create, FileAccess.Write);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private long FileSeek(long p, SeekOrigin d) => TgaFile?.Seek(p, d) ?? -1;

    private int FileRead(Span<byte> b) => TgaFile?.Read(b) ?? 0;

    private int FileWrite(ReadOnlySpan<byte> b)
    {
        TgaFile?.Write(b);
        return b.Length;
    }
}
