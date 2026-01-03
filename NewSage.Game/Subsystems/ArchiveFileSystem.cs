// -----------------------------------------------------------------------
// <copyright file="ArchiveFileSystem.cs" company="NewSage">
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

using NewSage.ArchiveFiles;
using NewSage.Logging;
using NewSage.Profile;

namespace NewSage.Game.Subsystems;

internal sealed class ArchiveFileSystem(GameOptions options) : SubsystemBase(options)
{
    private readonly List<IArchive> _archives = [];

    private bool _disposed;

    public static ArchiveFileSystem? TheArchiveFileSystem { get; set; }

    public void Mount(string filePath) => _archives.Add(BigArchive.Open(filePath));

    public void MountDirectory(string directory, string searchPattern = "*.big")
    {
        using var profiler = Profiler.Start($"{nameof(MountDirectory)}({directory})", options.EnableProfiling);
        IOrderedEnumerable<string> files = Directory.GetFiles(directory, searchPattern).Order();
        foreach (var file in files)
        {
            Log.Information($"Mounting archive '{file}'.");
            Mount(file);
        }
    }

    public Stream? OpenFile(string path)
    {
        // Search in reverse: Last archive mounted wins (Mod Support)
        for (var i = _archives.Count - 1; i >= 0; i--)
        {
            try
            {
                return _archives[i].OpenFile(path);
            }
            catch (FileNotFoundException)
            {
                // Just continue
            }
        }

        return null;
    }

    public override void Initialize() { }

    public override void Reset() { }

    public override void UpdateCore() { }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (IArchive archive in _archives)
            {
                archive.Dispose();
            }

            _archives.Clear();
        }

        _disposed = true;
        base.Dispose(disposing);
    }
}
