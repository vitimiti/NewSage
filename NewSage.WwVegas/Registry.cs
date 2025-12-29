// -----------------------------------------------------------------------
// <copyright file="Registry.cs" company="NewSage">
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
using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.Win32;

namespace NewSage.WwVegas;

public class Registry(string subKey, bool create = true) : IDisposable
{
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    [SupportedOSPlatform("windows")]
    private RegistryKey? _winKey;

    private Dictionary<string, object>? _fileData;
    private string? _fallbackPath;
    private bool _disposed;

    static Registry() => IsLocked = false;

    public static string ProviderName { get; set; } = "NewSage";

    public bool IsValid { get; private set; }

    public static bool IsLocked { get; set; }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Simply make it invalid."
    )]
    public void Initialize()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                var fullPath = Path.Combine("Software", ProviderName, subKey);

                _winKey =
                    create && !IsLocked
                        ? Microsoft.Win32.Registry.LocalMachine.CreateSubKey(fullPath)
                        : Microsoft.Win32.Registry.LocalMachine.OpenSubKey(fullPath, !IsLocked);

                IsValid = _winKey is not null;
            }
            catch
            {
                IsValid = false;
            }
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _fallbackPath = Path.Combine(home, ProviderName, subKey.Replace('\\', '/'), "settings.json");

            if (File.Exists(_fallbackPath))
            {
                try
                {
                    var json = File.ReadAllText(_fallbackPath);
                    _fileData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                }
                catch
                {
                    _fileData = [];
                }
            }
            else
            {
                _fileData = [];
            }

            IsValid = true;
        }
    }

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Less readable.")]
    public int GetInt(string name, int defaultValue = 0)
    {
        if (OperatingSystem.IsWindows())
        {
            return (int)(_winKey?.GetValue(name, defaultValue) ?? defaultValue);
        }

        if (_fileData is not null && _fileData.TryGetValue(name, out var val) && val is JsonElement je)
        {
            return je.TryGetInt32(out var result) ? result : defaultValue;
        }

        return defaultValue;
    }

    public void SetInt(string name, int value)
    {
        if (IsLocked)
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            _winKey?.SetValue(name, value, RegistryValueKind.DWord);
        }
        else
        {
            _fileData![name] = value;
            SaveChanges();
        }
    }

    [SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "This would be less readable."
    )]
    public string GetString(string name, string defaultValue = "")
    {
        if (OperatingSystem.IsWindows())
        {
            return _winKey?.GetValue(name, defaultValue)?.ToString() ?? defaultValue;
        }

        if (_fileData is not null && _fileData.TryGetValue(name, out var val))
        {
            return val.ToString() ?? defaultValue;
        }

        return defaultValue;
    }

    public void SetString(string name, string value)
    {
        if (IsLocked)
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            _winKey?.SetValue(name, value, RegistryValueKind.String);
        }
        else
        {
            _fileData![name] = value;
            SaveChanges();
        }
    }

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

        if (disposing && OperatingSystem.IsWindows())
        {
            _winKey?.Dispose();
        }

        _disposed = true;
    }

    private void SaveChanges()
    {
        if (OperatingSystem.IsWindows() || _fallbackPath is null || _fileData is null)
        {
            return;
        }

        _ = Directory.CreateDirectory(Path.GetDirectoryName(_fallbackPath)!);
        var json = JsonSerializer.Serialize(_fileData, _jsonOptions);
        File.WriteAllText(_fallbackPath, json);
    }
}
