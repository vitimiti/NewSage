// -----------------------------------------------------------------------
// <copyright file="VersionInformation.cs" company="NewSage">
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

using System.Reflection;
using NewSage.Logging;

namespace NewSage.Utilities;

public static class VersionInformation
{
    private static readonly Dictionary<string, string?> Metadata = typeof(VersionInformation)
        .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .ToDictionary(a => a.Key, a => a.Value);

    public static string AsciiVersion =>
        $"{GetValue("Major") ?? "?"}.{GetValue("Minor") ?? "?"}.{GetValue("Build") ?? "?"}";

    public static string BuildDate => GetValue("BuildDate") ?? "?";

    public static string BuildTime => GetValue("BuildTime") ?? "?";

    public static string BuildLocation => GetValue("BuildLocation") ?? "?";

    public static string BuildUser => GetValue("BuildUser") ?? "?";

    public static string GitRevision => GetValue("GitRevision") ?? "?";

    public static string GitVersion => GetValue("GitVersion") ?? "?";

    public static string GitCommitUnixTime => GetValue("GitCommitUnixTime") ?? "?";

    public static string GitAuthor => GetValue("GitAuthor") ?? "?";

    public static void LogVersionHeader()
    {
        Log.Debug("================================================================================");
        Log.Debug($"Generals version {AsciiVersion}");
        Log.Debug($"Build date: {BuildDate} {BuildTime}");
        Log.Debug($"Build location: {BuildLocation}");
        Log.Debug($"Build user: {BuildUser}");
        Log.Debug($"Build git revision: {GitRevision}");
        Log.Debug($"Build git version: {GitVersion}");
        Log.Debug($"Build git commit time: {GitCommitUnixTime}");
        Log.Debug($"Build git commit author: {GitAuthor}");
        Log.Debug("================================================================================");
    }

    private static string? GetValue(string key) => Metadata.GetValueOrDefault(key);
}
