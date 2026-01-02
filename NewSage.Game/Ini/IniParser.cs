// -----------------------------------------------------------------------
// <copyright file="IniParser.cs" company="NewSage">
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
using System.Text;
using NewSage.Game.Exceptions;
using NewSage.Game.Exceptions.IniExceptions;
using NewSage.Game.Transfer;
using NewSage.Utilities;

namespace NewSage.Game.Ini;

internal delegate void IniFieldParseProcess(
    [NotNull] IniParser ini,
    ref object? instance,
    ref object? store,
    object? userData
);

internal delegate void IniBlockParse([NotNull] IniParser ini);

internal delegate void BuildMultiIniFieldProcess([NotNull] MultiIniFieldParse parse);

internal class IniParser : IDisposable, IAsyncDisposable
{
    public const int MaxCharsPerLine = 1_028;

    protected const int ReadBufferSize = 8_192;

    private static readonly BlockParse[] TheTypeTable = [];

    private readonly byte[] _readBuffer = new byte[ReadBufferSize];
    private readonly byte[] _buffer = new byte[MaxCharsPerLine + 1];

#if DEBUG
    private readonly byte[] _currentBlockStart = new byte[MaxCharsPerLine + 1];
#endif

    private bool _disposed;

    public IniParser() => _buffer[0] = 0;

    public string FileName { get; protected set; } = "None";

    public IniLoadType LoadType { get; protected set; } = IniLoadType.Invalid;

    public uint LineNumber { get; protected set; }

    public bool EndOfFile { get; protected set; }

    protected static IList<byte> Separators { get; set; } = " \n\r\t="u8.ToArray();

    protected static IList<byte> SeparatorsPercent { get; set; } = " \n\r\t=%"u8.ToArray();

    protected static IList<byte> SeparatorsColor { get; set; } = " \n\r\t=:"u8.ToArray();

    protected static IList<byte> SeparatorsQuote { get; set; } = "\"\n="u8.ToArray();

    protected static string BlockEndToken { get; set; } = "END";

    protected Stream? IniFile { get; set; }

    protected IList<byte> ReadBuffer => _readBuffer;

    protected uint ReadBufferNext { get; set; }

    protected uint ReadBufferUsed { get; set; }

    protected IList<byte> Buffer => _buffer;

#if DEBUG
    protected IList<byte> CurrentBlockStart => _currentBlockStart;
#endif

    private static TransferService? Transfer { get; set; }

    public uint LoadFileDirectory(
        string fileDirectoryName,
        IniLoadType loadType,
        TransferService? transfer,
        bool recursive = true
    )
    {
        var filesRead = 0U;
        var iniDir = fileDirectoryName;
        var iniFile = fileDirectoryName;
        const string ext = ".ini";
        if (iniDir.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
        {
            iniDir = iniDir[..^ext.Length];
        }

        if (!iniFile.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
        {
            iniFile += ext;
        }

        if (File.Exists(iniFile))
        {
            filesRead += Load(iniFile, loadType, transfer);
        }

        filesRead += LoadDirectory(iniDir, loadType, transfer, recursive);

        return filesRead == 0
            ? throw new IniCantOpenFileException($"Cannot open INI file directory '{fileDirectoryName}'")
            : filesRead;
    }

    public uint LoadDirectory(
        [NotNull] string directory,
        IniLoadType loadType,
        TransferService? transfer,
        bool recursive = true
    )
    {
        if (string.IsNullOrEmpty(directory))
        {
            throw new IniInvalidDirectoryException("Directory name is empty.");
        }

        var rootPath = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var files = Directory.GetFiles(
            rootPath,
            "*.ini",
            new EnumerationOptions
            {
                RecurseSubdirectories = recursive,
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive,
            }
        );

        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        // Files in directory
        var filesRead = (
            from file in files
            let fileDir = Path.GetDirectoryName(file)
                ?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            where string.Equals(fileDir, rootPath, StringComparison.OrdinalIgnoreCase)
            select file
        ).Aggregate(0U, (current, file) => current + Load(file, loadType, transfer));

        // Files in subdirectories
        return (
            from file in files
            let fileDir = Path.GetDirectoryName(file)
                ?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            where !string.Equals(fileDir, rootPath, StringComparison.OrdinalIgnoreCase)
            select file
        ).Aggregate(filesRead, (current, file) => current + Load(file, loadType, transfer));
    }

    public uint Load(string fileName, IniLoadType loadType, TransferService? transfer)
    {
        Transfer = transfer;
        PrepareFile(fileName, loadType);

        try
        {
            Debug.Assert(!EndOfFile, $"Ini file '{FileName}' is empty.");
            while (!EndOfFile)
            {
                ReadLine();
                var currentLine = LegacyEncodings.Ansi.GetNullTerminatedString(_buffer);
                var token = _buffer.GetFirstToken((byte[])Separators);
                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                IniBlockParse? parse = FindBlockParse(token);
                if (parse is not null)
                {
#if DEBUG
                    LegacyEncodings.Ansi.GetBytes(currentLine, _currentBlockStart);
#endif
                    try
                    {
                        parse(this);
                    }
                    catch (Exception ex)
                    {
                        throw new IniException($"Error parsing INI file '{FileName}' (Line: '{LineNumber}').", ex);
                    }

#if DEBUG
                    LegacyEncodings.Ansi.GetBytes("NO_BLOCK", _currentBlockStart);
#endif
                }
                else
                {
                    throw new IniUnknownTokenException(
                        $"[LINE: {LineNumber} - File: '{FileName}'] Unknown block '{token}'"
                    );
                }
            }
        }
        finally
        {
            UnprepareFile();
        }

        return 1;
    }

    ~IniParser() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(continueOnCapturedContext: false);
        GC.SuppressFinalize(this);
    }

    protected static bool IsValidIniFileName(string? fileName) =>
        fileName?.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) == true;

    protected void PrepareFile(string fileName, IniLoadType loadType)
    {
        if (IniFile is not null)
        {
            throw new IniFileAlreadyOpenException($"Ini file '{fileName}' is already open.");
        }

        try
        {
            IniFile = File.OpenRead(fileName);
            var ms = new MemoryStream();
            IniFile.CopyTo(ms);
            IniFile.Dispose();
            IniFile = ms;
        }
        catch (Exception ex)
        {
            throw new IniCantOpenFileException($"Failed to open ini file '{fileName}'.", ex);
        }

        FileName = fileName;
        LoadType = loadType;
    }

    protected void UnprepareFile()
    {
        IniFile?.Dispose();
        IniFile = null;
        ReadBufferUsed = 0;
        ReadBufferNext = 0;
        FileName = "None";
        LoadType = IniLoadType.Invalid;
        LineNumber = 0;
        EndOfFile = false;
        Transfer = null;
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "Separating this algorithm may make it more unreadable."
    )]
    protected void ReadLine()
    {
        Debug.Assert(IniFile is not null, "Ini file is not open.");

        if (EndOfFile)
        {
            _buffer[0] = 0;
        }
        else
        {
            var p = 0;
            while (p < MaxCharsPerLine)
            {
                if (ReadBufferNext == ReadBufferUsed)
                {
                    ReadBufferNext = 0;
                    ReadBufferUsed = (uint)IniFile.Read(_readBuffer, 0, ReadBufferSize);

                    if (ReadBufferUsed == 0)
                    {
                        EndOfFile = true;
                        _buffer[p] = 0;
                        break;
                    }
                }

                var currentChar = _readBuffer[(int)ReadBufferNext++];
                _buffer[p] = currentChar;

                if (currentChar == (byte)'\n')
                {
                    _buffer[p] = 0;
                    break;
                }

                Debug.Assert(
                    currentChar != (byte)'\t',
                    $"Tab characters are not allowed in INI files ({FileName}). Please check your editor settings. Line Number {LineNumber}."
                );

                if (currentChar == (byte)';')
                {
                    _buffer[p] = 0;
                    while (!EndOfFile)
                    {
                        if (ReadBufferNext == ReadBufferUsed)
                        {
                            ReadBufferNext = 0;
                            ReadBufferUsed = (uint)IniFile.Read(_readBuffer, 0, ReadBufferSize);
                            if (ReadBufferUsed == 0)
                            {
                                EndOfFile = true;
                                break;
                            }
                        }

                        if (_readBuffer[(int)ReadBufferNext] == (byte)'\n')
                        {
                            ReadBufferNext++;
                            break;
                        }

                        ReadBufferNext++;
                    }

                    break;
                }

                if (currentChar is > 0 and < 32)
                {
                    _buffer[p] = (byte)' ';
                }

                p++;
            }

            if (p < _buffer.Length)
            {
                _buffer[p] = 0;
            }

            LineNumber++;

            if (p == MaxCharsPerLine)
            {
                Debug.Fail(
                    $"Buffer too small ({MaxCharsPerLine}) and was truncated, increase {nameof(MaxCharsPerLine)}."
                );
            }
        }

        Transfer?.TransferUserData(_buffer.AsSpan(0, _buffer.GetNullTerminatedStringLength()));
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
        {
            return;
        }

        if (IniFile is not null)
        {
            await IniFile.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        _disposed = true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            IniFile?.Dispose();
        }

        _disposed = true;
    }

    private static IniBlockParse? FindBlockParse(string token) =>
        (
            from blockParse in TheTypeTable
            where blockParse.Token.Equals(token, StringComparison.Ordinal)
            select blockParse.Parse
        ).FirstOrDefault();

    private static IniFieldParseProcess? FindFieldParse(
        ReadOnlySpan<IniFieldParse> parseTable,
        string? token,
        out int offset,
        out object? userData
    )
    {
        offset = 0;
        userData = null;

        foreach (IniFieldParse parse in parseTable)
        {
            if (!parse.Token.Equals(token, StringComparison.Ordinal))
            {
                continue;
            }

            offset = parse.Offset;
            userData = parse.UserData;
            return parse.Process;
        }

        if (parseTable.Length <= 0)
        {
            return null;
        }

        IniFieldParse last = parseTable[^1];
        if (!string.IsNullOrEmpty(last.Token))
        {
            return null;
        }

        offset = last.Offset;
        userData = token;
        return last.Process;
    }

    private sealed record BlockParse(string Token, IniBlockParse Parse);
}
