// -----------------------------------------------------------------------
// <copyright file="NameKeyGenerator.cs" company="NewSage">
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
using System.Text;
using NewSage.Game.Subsystems;
using NewSage.Profile;

namespace NewSage.Game.Utilities;

internal sealed class NameKeyGenerator(GameOptions options) : SubsystemBase(options), IDisposable
{
    private const int SocketCount = 6_473;

    private readonly Bucket?[] _sockets = new Bucket?[SocketCount];

    private bool _disposed;

    private NameKeyType _nextId = NameKeyType.Invalid;

    public static NameKeyGenerator? TheNameKeyGenerator { get; set; }

    public static void ParseStringAsNameKeyType() => throw new NotImplementedException();

    public override void Initialize()
    {
        using var profiler = Profiler.Start($"{nameof(NameKeyGenerator)} Initialization", options.EnableProfiling);
        Debug.Assert(_nextId == NameKeyType.Invalid, $"{nameof(NameKeyGenerator)} was already initialized.");
        Reset();
    }

    public override void Reset()
    {
        FreeSockets();
        _nextId = 1;
    }

    public override void UpdateCore() { }

    public NameKeyType NameToKey(string name)
    {
        var hash = CalculateHashForString(name) % SocketCount;
        Bucket? bucket;
        for (bucket = _sockets[hash]; bucket is not null; bucket = bucket.NextInSocket)
        {
            if (name.Equals(bucket.Name, StringComparison.Ordinal))
            {
                return bucket.Key;
            }
        }

        return NameToKeyCore(name, hash);
    }

    public NameKeyType NameToLowerCaseKey(string name)
    {
        var hash = CalculateHashForLowerCaseString(name) % SocketCount;

        Bucket? bucket;
        for (bucket = _sockets[hash]; bucket is not null; bucket = bucket.NextInSocket)
        {
            if (name.Equals(bucket.Name, StringComparison.OrdinalIgnoreCase))
            {
                return bucket.Key;
            }
        }

        return NameToKeyCore(name, hash);
    }

    public string KeyToName(NameKeyType key)
    {
        for (var i = 0; i < SocketCount; i++)
        {
            for (Bucket? bucket = _sockets[i]; bucket is not null; bucket = bucket.NextInSocket)
            {
                if (key == bucket.Key)
                {
                    return bucket.Name;
                }
            }
        }

        return string.Empty;
    }

    ~NameKeyGenerator() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static uint CalculateHashForString(string value)
    {
        uint hash = 0;
        foreach (var c in value)
        {
            // (uint)(value[i] & 0x7F) ensures we only use the ASCII part
            hash = (hash << 5) + hash + (uint)(c & 0x7F);
        }

        return hash;
    }

    private static uint CalculateHashForLowerCaseString(string value)
    {
        uint hash = 0;
        foreach (var c in value)
        {
            // Manual lower-case to avoid ToLowerInvariant() allocation
            var lowerC = c is >= 'A' and <= 'Z' ? (char)(c + 32) : c;
            hash = (hash << 5) + hash + (uint)(lowerC & 0x7F);
        }

        return hash;
    }

    private void FreeSockets()
    {
        for (var i = 0; i < SocketCount; i++)
        {
            Bucket? next;
            for (Bucket? bucket = _sockets[i]; bucket is not null; bucket = next)
            {
                next = bucket.NextInSocket;
                bucket.Delete();
            }

            _sockets[i] = null;
        }
    }

    private NameKeyType NameToKeyCore(string name, uint hash)
    {
        var bucket = Bucket.New();
        bucket.Key = _nextId++;
        bucket.Name = name;
        bucket.NextInSocket = _sockets[hash];
        _sockets[hash] = bucket;

        NameKeyType result = bucket.Key;
#if DEBUG
        const int maxThreshold = 2;
        var overThresholdCount = 0;
        for (var i = 0; i < SocketCount; i++)
        {
            var inThisSocketCount = 0;
            for (bucket = _sockets[i]; bucket is not null; bucket = bucket.NextInSocket)
            {
                inThisSocketCount++;
            }

            if (inThisSocketCount > maxThreshold)
            {
                overThresholdCount++;
            }
        }

        const int dividend = 20;
        if (overThresholdCount > SocketCount / dividend)
        {
            Debug.Fail(
                $"Might need to increase the number of bucket-sockets for {nameof(NameKeyGenerator)} ({nameof(overThresholdCount)} {overThresholdCount} = {overThresholdCount / (SocketCount / (float)dividend):F2})."
            );
        }
#endif
        return result;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            FreeSockets();
        }

        _disposed = true;
    }
}
