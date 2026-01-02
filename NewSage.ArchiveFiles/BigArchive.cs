// -----------------------------------------------------------------------
// <copyright file="BigArchive.cs" company="NewSage">
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

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using NewSage.Utilities;

namespace NewSage.ArchiveFiles;

public sealed class BigArchive : IArchive
{
    private readonly FileStream _archiveStream;
    private readonly Dictionary<string, BigArchiveEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    private bool _disposed;

    public string FilePath { get; }

    public IReadOnlyCollection<BigArchiveEntry> Entries => _entries.Values;

    private BigArchive(string filePath, FileStream stream)
    {
        FilePath = filePath;
        _archiveStream = stream;
    }

    public static BigArchive Open(string filePath)
    {
        var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var archive = new BigArchive(filePath, fs);
        try
        {
            archive.ReadHeader();
            return archive;
        }
        catch
        {
            fs.Dispose();
            throw;
        }
    }

    public static void Create(string outputFilePath, [NotNull] IDictionary<string, string> filesToPack)
    {
        using var fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);

        // 1. Calculate the Header Size
        // Magic (4) + TotalSize (4) + FileCount (4) + HeaderSize (4) = 16 bytes base
        var headerSize = filesToPack.Keys.Aggregate<string, uint>(
            16,
            (current, internalPath) => current + 8 + (uint)LegacyEncodings.Ansi.GetByteCount(internalPath) + 1
        );

        // 2. Write Placeholder Header (we will fill in Total Size later)
        Span<byte> buffer = stackalloc byte[16];
        _ = LegacyEncodings.Ansi.GetBytes("BIGF", buffer[..4]);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..8], 0); // Placeholder for Total Size
        BinaryPrimitives.WriteUInt32BigEndian(buffer[8..12], (uint)filesToPack.Count);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[12..16], headerSize);
        fs.Write(buffer);

        // 3. Write Index
        var currentDataOffset = headerSize;
        var fileList = filesToPack.ToList(); // Keep order consistent

        foreach (KeyValuePair<string, string> kvp in fileList)
        {
            var fileInfo = new FileInfo(kvp.Value);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"Source file not found: {kvp.Value}");
            }

            BinaryPrimitives.WriteUInt32BigEndian(buffer[..4], currentDataOffset);
            BinaryPrimitives.WriteUInt32BigEndian(buffer[4..8], (uint)fileInfo.Length);
            fs.Write(buffer[..8]);

            var pathBytes = LegacyEncodings.Ansi.GetBytes(kvp.Key);
            fs.Write(pathBytes);
            fs.WriteByte(0); // Null terminator

            currentDataOffset += (uint)fileInfo.Length;
        }

        // 4. Write File Data
        foreach (KeyValuePair<string, string> kvp in fileList)
        {
            using var sourceFs = new FileStream(kvp.Value, FileMode.Open, FileAccess.Read);
            sourceFs.CopyTo(fs);
        }

        // 5. Update Total Size (Little Endian as per format)
        _ = fs.Seek(4, SeekOrigin.Begin);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], (uint)fs.Length);
        fs.Write(buffer[..4]);
    }

    public Stream OpenFile([NotNull] string internalPath)
    {
        var normalized = internalPath.Replace('/', '\\');
        return !_entries.TryGetValue(normalized, out BigArchiveEntry? entry)
            ? throw new FileNotFoundException($"File '{internalPath}' not found in archive '{FilePath}'")
            : new BigArchiveStream(_archiveStream, entry.Offset, entry.Size);
    }

    ~BigArchive() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void ReadHeader()
    {
        Span<byte> buffer = stackalloc byte[16];
        _archiveStream.ReadExactly(buffer);

        if (LegacyEncodings.Ansi.GetString(buffer[..4]) != "BIGF")
        {
            throw new InvalidDataException($"File {FilePath} is not a valid BIG archive (Magic mismatch).");
        }

        // Header Structure (Original C++ Engine):
        // 00-03: 'BIGF'
        // 04-07: Total File Size (Little Endian)
        // 08-11: Number of Files (Big Endian)
        // 12-15: Total Header/Index Size (Big Endian)
        var fileCount = BinaryPrimitives.ReadUInt32BigEndian(buffer[8..12]);

        // The original engine jumps to 0x10 to begin reading the file index
        _ = _archiveStream.Seek(0x10, SeekOrigin.Begin);

        Span<byte> nameBuffer = stackalloc byte[1024];
        for (var i = 0; i < fileCount; i++)
        {
            // Read Offset (4 bytes) and Size (4 bytes)
            _archiveStream.ReadExactly(buffer[..8]);
            var offset = BinaryPrimitives.ReadUInt32BigEndian(buffer[..4]);
            var size = BinaryPrimitives.ReadUInt32BigEndian(buffer[4..8]);

            // Read the path string (null-terminated)
            var byteCount = 0;
            int b;
            while ((b = _archiveStream.ReadByte()) > 0)
            {
                if (byteCount < nameBuffer.Length)
                {
                    nameBuffer[byteCount++] = (byte)b;
                }
            }

            // Normalize path to use backslashes for consistency
            var path = LegacyEncodings.Ansi.GetString(nameBuffer[..byteCount]).Replace('/', '\\');
            _entries[path] = new BigArchiveEntry(path, offset, size);
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _archiveStream.Dispose();
        }

        _disposed = true;
    }
}
