// -----------------------------------------------------------------------
// <copyright file="LogInterpolatedStringHandler.cs" company="NewSage">
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
using System.Text;

namespace NewSage.Logging.InterpolatedStringHandlers;

[InterpolatedStringHandler]
public readonly ref struct LogInterpolatedStringHandler
{
    private readonly StringBuilder? _builder;

    public LogInterpolatedStringHandler(int literalLength, int formattedCount, LogLevel level, out bool isEnabled)
    {
        isEnabled = level >= Log.MinimumLevel;
        IsEnabled = isEnabled;
        if (isEnabled)
        {
            _builder = new StringBuilder(literalLength + (formattedCount * 16));
        }
    }

    internal LogInterpolatedStringHandler(int literalLength, int formattedCount, LogLevel level)
    {
        IsEnabled = level >= Log.MinimumLevel;
        if (IsEnabled)
        {
            _builder = new StringBuilder(literalLength + (formattedCount * 16));
        }
    }

    public bool IsEnabled { get; }

    public readonly void AppendLiteral(string value) => _builder?.Append(value);

    public readonly void AppendFormatted<T>(T value) => _builder?.Append(value);

    public override readonly string ToString() => _builder?.ToString() ?? string.Empty;
}
