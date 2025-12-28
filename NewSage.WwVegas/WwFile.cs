// -----------------------------------------------------------------------
// <copyright file="WwFile.cs" company="NewSage">
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

using System.Text;
using Microsoft.Win32.SafeHandles;

namespace NewSage.WwVegas;

public abstract class WwFile
{
    public abstract string FileName { get; set; }

    public abstract int Size { get; }

    public virtual WwDateTime GetDateTime { get; set; } = new(0, 0);

    public virtual SafeFileHandle FileHandle { get; } = new(nint.Zero, ownsHandle: true);

    public abstract bool TryCreate();

    public abstract bool TryDelete();

    public abstract bool IsAvailable(bool forced = false);

    public abstract bool IsOpen();

    public abstract bool TryOpen(string fileName, FileAccess access);

    public abstract bool TryOpen(FileAccess access);

    public abstract int Read(Span<byte> buffer);

    public abstract int Seek(int position, SeekOrigin origin = SeekOrigin.Current);

    public virtual int Tell() => Seek(0);

    public abstract int Write(ReadOnlySpan<byte> buffer);

    public abstract void Close();

    public int Print(string text) => Write(Encoding.UTF8.GetBytes(text));

    public int Print(ReadOnlySpan<byte> buffer) => Write(buffer);

    public int PrintIndented(uint depth, string text)
    {
        depth = uint.Clamp(depth, 0, 1024);
        var sb = new StringBuilder();
        for (var i = 0; i < depth; i++)
        {
            _ = sb.Append('\t');
        }

        _ = sb.Append(text);

        return Print(sb.ToString());
    }
}
