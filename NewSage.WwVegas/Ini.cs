// -----------------------------------------------------------------------
// <copyright file="Ini.cs" company="NewSage">
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
using System.Globalization;
using System.Text;

namespace NewSage.WwVegas;

public class Ini : IDisposable
{
    private const int MaxLineLength = 4096;

    private bool _disposed;

    public static bool KeepBlankEntries { get; set; }

    public List<IniSection> SectionList { get; } = new();

    public Index<int, IniSection> SectionIndex { get; } = new();

    public string FileName { get; private set; } = "<unknown>";

    public bool IsLoaded => !SectionList.IsEmpty;

    public Ini() { }

    public Ini(FileStream file) => _ = Load(file);

    public Ini(string fileName) => _ = Load(fileName);

    public int Load(FileStream file)
    {
        ArgumentNullException.ThrowIfNull(file);

        FileName = file.Name;
        using var fs = new FileStraw(file);
        return Load(fs);
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "This is a complex algorithm and breaking into chunks may make it difficult to follow."
    )]
    public int Load(Straw straw)
    {
        var eof = false;
        var buffer = new byte[MaxLineLength];
        IniSection? currentSection = null;
        var linesProcessed = 0;

        while (!eof)
        {
            var len = Line.Read(straw, buffer, out eof);
            if (len <= 0 && !eof)
            {
                continue;
            }

            linesProcessed++;

            var rawLine = Encoding.ASCII.GetString(buffer, 0, len);
            var commentIdx = rawLine.IndexOf(';', StringComparison.OrdinalIgnoreCase);
            if (commentIdx != -1)
            {
                rawLine = rawLine[..commentIdx];
            }

            var trimmed = rawLine.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            if (trimmed.StartsWith('[') && trimmed.Contains(']', StringComparison.OrdinalIgnoreCase))
            {
                var sectionName = trimmed[1..trimmed.IndexOf(']', StringComparison.OrdinalIgnoreCase)].Trim();
                currentSection = FindSection(sectionName);
                if (currentSection is null)
                {
                    currentSection = new IniSection(sectionName);
                    SectionList.AddTail(currentSection);
                    _ = SectionIndex.AddIndex(currentSection.IndexId, currentSection);
                }

                continue;
            }

            if (currentSection is null)
            {
                continue;
            }

            var divider = trimmed.IndexOf('=', StringComparison.OrdinalIgnoreCase);
            if (divider == -1)
            {
                continue;
            }

            var key = trimmed[..divider].Trim();
            var val = trimmed[(divider + 1)..].Trim();

            if (string.IsNullOrEmpty(val) && !KeepBlankEntries)
            {
                continue;
            }

            IniEntry? entry = currentSection.FindEntry(key);
            if (entry is not null)
            {
                entry.Value = val;
            }
            else
            {
                entry = new IniEntry(key, val);
                currentSection.EntryList.AddTail(entry);
                _ = currentSection.EntryIndex.AddIndex(entry.IndexId, entry);
            }
        }

        return linesProcessed;
    }

    public int Load(string fileName)
    {
        FileName = fileName;
        if (!File.Exists(fileName))
        {
            return 0;
        }

        using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        return Load(fs);
    }

    public int Save(FileStream file)
    {
        using var fp = new FilePipe(file);
        return Save(fp);
    }

    public int Save(Pipe pipe)
    {
        ArgumentNullException.ThrowIfNull(pipe);

        var total = 0;
        var eol = Encoding.ASCII.GetBytes(Environment.NewLine);

        foreach (IniSection section in SectionList)
        {
            total += pipe.Put(Encoding.ASCII.GetBytes($"[{section.Section}]"));
            total += pipe.Put(eol);

            foreach (IniEntry entry in section.EntryList)
            {
                total += pipe.Put(Encoding.ASCII.GetBytes($"{entry.Entry}={entry.Value}"));
                total += pipe.Put(eol);
            }

            total += pipe.Put(eol);
        }

        total += pipe.End();
        return total;
    }

    public int Save(string fileName)
    {
        FileName = fileName;
        using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        return Save(fs);
    }

    public void Clear(string? section = null, string? entry = null)
    {
        if (section is null)
        {
            while (!SectionList.IsEmpty)
            {
                if (SectionList.FirstValid is not { } sec)
                {
                    break;
                }

                _ = SectionIndex.RemoveIndex(sec.IndexId);
                sec.Unlink();
            }

            FileName = "<unknown>";
        }
        else
        {
            IniSection? sec = FindSection(section);
            if (sec is null)
            {
                return;
            }

            if (entry is null)
            {
                _ = SectionIndex.RemoveIndex(sec.IndexId);
                sec.Unlink();
            }
            else
            {
                IniEntry? ent = sec.FindEntry(entry);
                if (ent is null)
                {
                    return;
                }

                _ = sec.EntryIndex.RemoveIndex(ent.IndexId);
                ent.Unlink();
            }
        }
    }

    public IniSection? FindSection(string section)
    {
        var crc = (int)Crc.String(section);
        return SectionIndex.IsPresent(crc) ? SectionIndex[crc] : null;
    }

    public IniEntry? FindEntry(string section, string entry) => FindSection(section)?.FindEntry(entry);

    public bool IsPresent(string section, string? entry = null) =>
        entry is null ? FindSection(section) is not null : FindEntry(section, entry) is not null;

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Worse readability.")]
    public int GetInt(string section, string entry, int defaultValue = 0)
    {
        var val = FindEntry(section, entry)?.Value;
        if (string.IsNullOrEmpty(val))
        {
            return defaultValue;
        }

        if (val.StartsWith('$'))
        {
            return Convert.ToInt32(val[1..], 16);
        }

        if (val.EndsWith('h'))
        {
            return Convert.ToInt32(val[..^1], 16);
        }

        return int.TryParse(val, out var result) ? result : defaultValue;
    }

    public bool GetBool(string section, string entry, bool defaultValue = false)
    {
        var val = FindEntry(section, entry)?.Value?.ToUpperInvariant();
        return val switch
        {
            "YES" or "TRUE" or "1" or "T" or "Y" => true,
            "NO" or "FALSE" or "0" or "F" or "N" => false,
            _ => defaultValue,
        };
    }

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Worse readability.")]
    public float GetFloat(string section, string entry, float defaultValue = 0.0f)
    {
        var val = FindEntry(section, entry)?.Value;
        if (string.IsNullOrEmpty(val))
        {
            return defaultValue;
        }

        var isPercent = val.EndsWith('%');
        if (isPercent)
        {
            val = val[..^1];
        }

        if (float.TryParse(val, out var result))
        {
            return isPercent ? result / 100.0f : result;
        }

        return defaultValue;
    }

    public string GetString(string section, string entry, string defaultValue = "") =>
        FindEntry(section, entry)?.Value ?? defaultValue;

    public Point2D<int> GetPoint(string section, string entry, Point2D<int> defaultValue)
    {
        var val = FindEntry(section, entry)?.Value;
        if (string.IsNullOrEmpty(val))
        {
            return defaultValue;
        }

        var parts = val.Split(',');
        return parts.Length >= 2 && int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y)
            ? new Point2D<int>(x, y)
            : defaultValue;
    }

    public bool PutUUBlock(string section, ReadOnlySpan<byte> block)
    {
        if (string.IsNullOrEmpty(section) || block.IsEmpty)
        {
            return false;
        }

        Clear(section);
        using var source = new BufferStraw(block);
        using var encoder = new Base64Straw(CodeControl.Encode);
        encoder.GetFrom(source);
        var counter = 1;
        var buffer = new byte[71];
        while (true)
        {
            var len = encoder.Get(buffer);
            if (len == 0)
            {
                break;
            }

            PutString(
                section,
                counter++.ToString(CultureInfo.InvariantCulture),
                Encoding.ASCII.GetString(buffer, 0, len)
            );
        }

        return true;
    }

    public int GetUUBlock(string section, Span<byte> destination)
    {
        IniSection? sec = FindSection(section);
        if (sec is null)
        {
            return 0;
        }

        using var bPipe = new BufferPipe(destination);
        using var decoder = new Base64Pipe(CodeControl.Decode);
        decoder.PutTo(bPipe);
        foreach (IniEntry entry in sec.EntryList.Where<IniEntry>(e => e.Value is not null))
        {
            _ = decoder.Put(Encoding.ASCII.GetBytes(entry.Value!));
        }

        _ = decoder.End();
        bPipe.Buffer.Span.CopyTo(destination);
        return bPipe.Index;
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "This object is owned by the class and disposed through the section index disposal."
    )]
    public void PutString(string section, string entry, string value)
    {
        IniSection sec = FindSection(section) ?? new IniSection(section);

        if (!SectionIndex.IsPresent(sec.IndexId))
        {
            SectionList.AddTail(sec);
            _ = SectionIndex.AddIndex(sec.IndexId, sec);
        }

        IniEntry? ent = sec.FindEntry(entry);
        if (ent is null)
        {
            ent = new IniEntry(entry, value);
            sec.EntryList.AddTail(ent);
            _ = sec.EntryIndex.AddIndex(ent.IndexId, ent);
        }
        else
        {
            ent.Value = value;
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

        if (disposing)
        {
            SectionList.Dispose();
            SectionIndex.Dispose();
        }

        _disposed = true;
    }
}
