// -----------------------------------------------------------------------
// <copyright file="Args.cs" company="NewSage">
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

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace NewSage.WwVegas;

public class Args : IEnumerable<string>
{
    private readonly string[] _argv;
    private readonly HashSet<string> _loadedFiles = new(StringComparer.OrdinalIgnoreCase);

    private int _currentIndex = -1;

    public Args(string[] args, string filePrefix = "@")
    {
        var processed = new System.Collections.Generic.List<string>();
        var stack = new Stack<string>(args.Reverse());

        while (stack.TryPop(out var arg))
        {
            if (!string.IsNullOrEmpty(filePrefix) && arg.StartsWith(filePrefix, StringComparison.Ordinal))
            {
                LoadResponseFile(arg[filePrefix.Length..], stack);
            }
            else
            {
                processed.Add(arg);
            }
        }

        _argv = [.. processed];
    }

    public string? First()
    {
        _currentIndex = 0;
        return Cur();
    }

    public string? Next()
    {
        _currentIndex++;
        return Cur();
    }

    public string? Cur() => (_currentIndex >= 0 && _currentIndex < _argv.Length) ? _argv[_currentIndex] : null;

    public void Reset() => _currentIndex = -1;

    public string? Find(string prefix, bool caseSensitive = false)
    {
        _currentIndex = -1;
        return FindAgain(prefix, caseSensitive);
    }

    public string? FindAgain(string prefix, bool caseSensitive = false)
    {
        StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        for (var i = _currentIndex + 1; i < _argv.Length; i++)
        {
            if (!_argv[i].StartsWith(prefix, comparison))
            {
                continue;
            }

            _currentIndex = i;
            return _argv[i];
        }

        return null;
    }

    public string? GetValue(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        var match = Find(prefix);
        if (match is null)
        {
            return null;
        }

        if (match.Length > prefix.Length)
        {
            return match[prefix.Length..].TrimStart();
        }

        var nextIdx = _currentIndex + 1;
        return nextIdx < _argv.Length && !_argv[nextIdx].StartsWith('-') && !_argv[nextIdx].StartsWith('/')
            ? _argv[nextIdx]
            : null;
    }

    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)_argv).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Silent failure on file errors is intentional."
    )]
    private void LoadResponseFile(string path, Stack<string> stack)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath) || !_loadedFiles.Add(fullPath))
            {
                return;
            }

            foreach (var line in File.ReadLines(fullPath).Reverse())
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
                {
                    continue;
                }

                stack.Push(trimmed);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load response file '{path}': {ex}");
        }
    }
}
