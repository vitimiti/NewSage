// -----------------------------------------------------------------------
// <copyright file="SaveLoadStatus.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwSaveLoad;

public static class SaveLoadStatus
{
    private static readonly Lock TextLock = new();
    private static readonly string[] StatusText = [string.Empty, string.Empty];
    private static int _statusCount;

    public static int StatusCount => _statusCount;

    public static void SetStatusText(string text, int id)
    {
        lock (TextLock)
        {
            if (id >= 0 && id < StatusText.Length)
            {
                StatusText[id] = text;
                if (id == 0)
                {
                    StatusText[1] = string.Empty;
                }
            }
        }
    }

    public static string GetStatusText(int id)
    {
        lock (TextLock)
        {
            return (id >= 0 && id < StatusText.Length) ? StatusText[id] : string.Empty;
        }
    }

    public static void ResetStatusCount() => _statusCount = 0;

    public static void IncStatusCount() => Interlocked.Increment(ref _statusCount);
}
