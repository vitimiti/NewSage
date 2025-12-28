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

namespace NewSage.WwVegas;

public class Args
{
    private readonly Dictionary<string, string?> _commands = [];

    public Args(string[] args) => Initialize(args);

    public IReadOnlyDictionary<string, string?> Commands => _commands;

    public bool CommandPassed(string command) => _commands.TryGetValue(command, out _);

    public bool ParseCommandOptionAsBoolean(string command, bool defaultValue = false) =>
        _commands.TryGetValue(command, out var value) && bool.TryParse(value, out var result) ? result : defaultValue;

    public string? ParseCommandOptionAsString(string command, string? defaultValue = null) =>
        _commands.GetValueOrDefault(command, defaultValue);

    public int ParseCommandOptionAsInt(string command, int defaultValue = -1) =>
        _commands.TryGetValue(command, out var value) && int.TryParse(value, out var result) ? result : defaultValue;

    private void Initialize(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var index = 0;
        while (index < args.Length)
        {
            string? command;

            if (args[index].StartsWith('-') || args[index].StartsWith('/'))
            {
                command = args[index][1..];
            }
            else
            {
                // If it's not a command, skip it or handle as a positional arg
                index++;
                continue;
            }

            string? value = null;

            // Check if there is a next argument and if it's a value (not a new command)
            if (index + 1 < args.Length && !args[index + 1].StartsWith('-') && !args[index + 1].StartsWith('/'))
            {
                value = args[index + 1];
                index++;
            }

            index++;
            _ = _commands.TryAdd(command, value);
        }
    }
}
