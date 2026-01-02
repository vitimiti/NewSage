// -----------------------------------------------------------------------
// <copyright file="SubsystemList.cs" company="NewSage">
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

using NewSage.Game.Ini;
using NewSage.Game.Transfer;

namespace NewSage.Game.Subsystems;

internal sealed class SubsystemList : IDisposable
{
    private readonly List<SubsystemBase> _subsystems = [];

    private bool _disposed;

    public static SubsystemList? TheSubsystemList { get; set; }

    public void InitializeSubsystem(
        SubsystemBase subsystem,
        string? path1,
        string? path2,
        TransferService? transfer,
        string name
    )
    {
        subsystem.Name = name;
        subsystem.Initialize();

        using var ini = new IniParser();
        if (path1 is not null)
        {
            _ = ini.LoadFileDirectory(path1, IniLoadType.Overwrite, transfer);
        }

        if (path2 is not null)
        {
            _ = ini.LoadFileDirectory(path2, IniLoadType.Overwrite, transfer);
        }

        _subsystems.Add(subsystem);
    }

    public void PostProcessLoadAll()
    {
        foreach (SubsystemBase subsystem in _subsystems)
        {
            subsystem.PostProcessLoad();
        }
    }

    public void ResetAll()
    {
        foreach (SubsystemBase subsystem in _subsystems)
        {
            subsystem.Reset();
        }
    }

    public void ShutdownAll()
    {
        foreach (SubsystemBase subsystem in _subsystems)
        {
            subsystem.Dispose();
        }

        _subsystems.Clear();
    }

    ~SubsystemList() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
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
            ShutdownAll();
        }

        _disposed = true;
    }
}
