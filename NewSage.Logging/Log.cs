// -----------------------------------------------------------------------
// <copyright file="Log.cs" company="NewSage">
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

using System.Collections.Concurrent;
using System.Globalization;

namespace NewSage.Logging;

public static class Log
{
    private static readonly ConcurrentBag<ILogSink> Sinks = [];

    public static LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    public static void AddSink(ILogSink sink) => Sinks.Add(sink);

    public static void Trace(string message) => Write(LogLevel.Trace, message);

    public static void Trace(ref LogInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            Write(LogLevel.Trace, handler.ToString());
        }
    }

    public static void Debug(string message) => Write(LogLevel.Debug, message);

    public static void Debug(ref LogInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            Write(LogLevel.Debug, handler.ToString());
        }
    }

    public static void Information(string message) => Write(LogLevel.Information, message);

    public static void Information(ref LogInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            Write(LogLevel.Information, handler.ToString());
        }
    }

    public static void Warning(string message) => Write(LogLevel.Warning, message);

    public static void Warning(ref LogInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            Write(LogLevel.Warning, handler.ToString());
        }
    }

    public static void Error(string message) => Write(LogLevel.Error, message);

    public static void Error(ref LogInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            Write(LogLevel.Error, handler.ToString());
        }
    }

    public static void Critical(string message) => Write(LogLevel.Critical, message);

    public static void Critical(ref LogInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            Write(LogLevel.Critical, handler.ToString());
        }
    }

    private static void Write(LogLevel level, string message)
    {
        if (level < MinimumLevel)
        {
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        var formatted = $"[{timestamp}] [{level}] {message}";

        foreach (ILogSink sink in Sinks)
        {
            sink.Write(level, formatted);
        }
    }
}
