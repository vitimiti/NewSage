// -----------------------------------------------------------------------
// <copyright file="SubsystemBase.cs" company="NewSage">
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
using NewSage.Logging;
using NewSage.Profile;

namespace NewSage.Game.Subsystems;

internal abstract class SubsystemBase(GameOptions options) : IDisposable
{
    private bool _disposed;

    public string Name { get; set; } = "<Unknown Subsystem>";

    public abstract void Initialize();

    public virtual void PostProcessLoad() { }

    public abstract void Reset();

    public abstract void UpdateCore();

    public virtual void DrawCore() => Debug.Fail("Shouldn't call from the base class directly.");

    public void Update()
    {
        using var profiler = Profiler.Start($"Subsystem {Name} - Update", options.EnableProfiling, LogLevel.Trace);
        UpdateCore();
    }

    public void Draw()
    {
        using var profiler = Profiler.Start($"Subsystem {Name} - Draw", options.EnableProfiling, LogLevel.Trace);
        DrawCore();
    }

    ~SubsystemBase() => Dispose(disposing: false);

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

        if (disposing)
        {
            // Nothing to dispose of in the base class.
        }

        _disposed = true;
    }
}
