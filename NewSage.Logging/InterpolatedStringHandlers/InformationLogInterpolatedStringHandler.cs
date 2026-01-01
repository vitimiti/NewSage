// -----------------------------------------------------------------------
// <copyright file="InformationLogInterpolatedStringHandler.cs" company="NewSage">
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

namespace NewSage.Logging.InterpolatedStringHandlers;

[InterpolatedStringHandler]
public readonly ref struct InformationLogInterpolatedStringHandler
{
    private readonly LogInterpolatedStringHandler _inner;

    public InformationLogInterpolatedStringHandler(int literalLength, int formattedCount, out bool isEnabled) =>
        _inner = new LogInterpolatedStringHandler(literalLength, formattedCount, LogLevel.Information, out isEnabled);

    public bool IsEnabled => _inner.IsEnabled;

    public void AppendLiteral(string value) => _inner.AppendLiteral(value);

    public void AppendFormatted<T>(T value) => _inner.AppendFormatted(value);

    public override string ToString() => _inner.ToString();
}
