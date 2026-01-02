// -----------------------------------------------------------------------
// <copyright file="GlobalData.cs" company="NewSage">
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

namespace NewSage.Game.Subsystems;

internal class GlobalData : SubsystemBase, ICloneable
{
    private readonly GameOptions _options;

    private GlobalData? _next;
    private bool _disposed;

    public GlobalData(GameOptions options)
        : base(options)
    {
        _options = options;
        if (TheOriginal is null)
        {
            TheOriginal = this;
        }
    }

    public static GlobalData? TheWritableGlobalData { get; set; }

    public static GlobalData? TheGlobalData => TheWritableGlobalData;

    public float ViewportHeightScale { get; set; } = .8F;

    private static GlobalData? TheOriginal { get; set; }

    public override void Initialize() { }

    public override void Reset() { }

    public override void UpdateCore() { }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "No need for exceptions here."
    )]
    public void ParseCustomDefinition()
    {
        var path = Path.Combine(_options.GameDirectory, "GenTool", "fullviewport.dat");
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            if (stream.ReadByte() != (byte)'0')
            {
                ViewportHeightScale = 1F;
            }
        }
        catch (Exception)
        {
            // Keep going
        }
    }

    object ICloneable.Clone() => Clone();

    public object Clone()
    {
        var clone = (GlobalData)MemberwiseClone();
        clone._next = null;
        return clone;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Debug.Assert(TheWritableGlobalData?._next is null, "The original is not original.");

            if (ReferenceEquals(TheOriginal, this))
            {
                TheOriginal = null;
                TheWritableGlobalData = null;
            }
        }

        _disposed = true;
        base.Dispose(disposing);
    }

    private static GlobalData NewOverride()
    {
        Debug.Assert(TheWritableGlobalData is not null, "GlobalData.NewOverride() - no existing data.");

        var overrideData = (GlobalData)TheWritableGlobalData.Clone();
        overrideData._next = TheWritableGlobalData;
        TheWritableGlobalData = overrideData;

        return overrideData;
    }
}
