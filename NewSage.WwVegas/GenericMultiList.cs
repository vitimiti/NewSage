// -----------------------------------------------------------------------
// <copyright file="GenericMultiList.cs" company="NewSage">
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

using NewSage.WwVegas.WwDebug;

namespace NewSage.WwVegas;

public abstract class GenericMultiList : IDisposable
{
    private bool _disposed;

    protected GenericMultiList()
    {
        SentinelHead.Next = SentinelHead;
        SentinelHead.Prev = SentinelHead;
    }

    public bool IsEmpty => SentinelHead.Next == SentinelHead;

    protected MultiListNode SentinelHead { get; } = new();

    public int Count()
    {
        var count = 0;
        MultiListNode? cur = SentinelHead.Next;
        while (cur != SentinelHead && cur is not null)
        {
            count++;
            cur = cur.Next;
        }

        return count;
    }

    public bool Contains(MultiListObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        MultiListNode? node = obj.ListNodeHead;
        while (node is not null)
        {
            if (node.List == this)
            {
                return true;
            }

            node = node.NextList;
        }

        return false;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal bool InternalAdd(MultiListObject obj, bool onlyOnce)
    {
        using (new MemorySample(MemoryCategory.GameData))
        {
            ArgumentNullException.ThrowIfNull(obj);

            if (onlyOnce && Contains(obj))
            {
                return false;
            }

            var node = MultiListNode.Create();
            node.Object = obj;
            node.List = this;

            node.NextList = obj.ListNodeHead;
            obj.ListNodeHead = node;

            node.Prev = SentinelHead;
            node.Next = SentinelHead.Next;
            SentinelHead.Next!.Prev = node;
            SentinelHead.Next = node;

            return true;
        }
    }

    internal bool InternalRemove(MultiListObject obj)
    {
        MultiListNode? node = obj.ListNodeHead;
        MultiListNode? prevListNode = null;

        while (node is not null && node.List != this)
        {
            prevListNode = node;
            node = node.NextList;
        }

        if (node is null)
        {
            return false;
        }

        node.Prev!.Next = node.Next;
        node.Next!.Prev = node.Prev;

        if (prevListNode is not null)
        {
            prevListNode.NextList = node.NextList;
        }
        else
        {
            obj.ListNodeHead = node.NextList;
        }

        node.Dispose();
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            while (!IsEmpty)
            {
                MultiListObject? obj = SentinelHead.Next?.Object;
                if (obj is not null)
                {
                    _ = InternalRemove(obj);
                }
            }
        }

        _disposed = true;
    }
}
