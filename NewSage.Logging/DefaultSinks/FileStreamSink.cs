// -----------------------------------------------------------------------
// <copyright file="FileStreamSink.cs" company="NewSage">
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

namespace NewSage.Logging.DefaultSinks;

public sealed class FileStreamSink : ILogSink, IDisposable, IAsyncDisposable
{
    private readonly StreamWriter? _writer;
    private readonly Lock _lock = new();

    private bool _disposed;

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Do not throw during logging, just give up."
    )]
    public FileStreamSink(string gameId)
    {
        try
        {
            var baseFolder = OperatingSystem.IsWindows()
                ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var logDir = Path.Combine(baseFolder, gameId, "Logs");
            _ = Directory.CreateDirectory(logDir);

            var fileName = $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var logFilePath = Path.Combine(logDir, fileName);

            var stream = new FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(stream) { AutoFlush = true };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[FileStreamSink] Failed to initialize: {ex.Message}");
        }
    }

    public void Write(LogLevel level, string message)
    {
        lock (_lock)
        {
            if (_writer is null)
            {
                return;
            }

            _writer.WriteLine(message);
        }
    }

    ~FileStreamSink() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(continueOnCapturedContext: false);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _writer?.Dispose();
        }

        _disposed = true;
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
        {
            return;
        }

        if (_writer is not null)
        {
            await _writer.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        _disposed = true;
    }
}
