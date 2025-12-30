// -----------------------------------------------------------------------
// <copyright file="VersionCheck.cs" company="NewSage">
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
using System.Reflection;

namespace NewSage.WwVegas;

public static class VersionCheck
{
    public static FileVersionInfo? GetVersionInfo(string fileName) =>
        !File.Exists(fileName) ? null : FileVersionInfo.GetVersionInfo(fileName);

    public static DateTime GetFileCreationTime(string fileName) =>
        File.Exists(fileName) ? File.GetCreationTime(fileName) : DateTime.MinValue;

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Worse readability.")]
    public static int CompareVersion(string otherFile)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        Version? current = entryAssembly?.GetName().Version;
        FileVersionInfo? otherInfo = GetVersionInfo(otherFile);

        if (otherInfo?.FileVersion is null)
        {
            return 1;
        }

        if (Version.TryParse(otherInfo.FileVersion, out Version? other))
        {
            return current?.CompareTo(other) ?? 0;
        }

        return string.CompareOrdinal(current?.ToString(), otherInfo.FileVersion);
    }
}
