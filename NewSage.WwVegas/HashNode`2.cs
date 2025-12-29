// -----------------------------------------------------------------------
// <copyright file="HashNode`2.cs" company="NewSage">
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

namespace NewSage.WwVegas;

public class HashNode<T, TUser> : DataNode<HashNode<T, TUser>>
    where TUser : class, new()
{
    private Flags _flags;

    public HashNode(T record, uint key)
    {
        Record = record;
        Key = key;
        Value = this;
        User = new TUser();
        _flags = Flags.NewRecord | Flags.KeySet;
    }

    public HashNode()
    {
        Value = this;
        User = new TUser();
        _flags = Flags.NewRecord;
        Key = uint.MaxValue;
    }

    public T Record { get; set; }

    public uint Key { get; private set; }

    public TUser User { get; }

    public new HashNode<T, TUser>? Next => base.Next as HashNode<T, TUser>;

    public new HashNode<T, TUser>? NextValid => base.NextValid as HashNode<T, TUser>;

    public new HashNode<T, TUser>? Previous => base.Previous as HashNode<T, TUser>;

    public new HashNode<T, TUser>? PreviousValid => base.PreviousValid as HashNode<T, TUser>;

    public bool IsInList => _flags.HasFlag(Flags.InList);

    public bool IsNewRecord => _flags.HasFlag(Flags.NewRecord);

    public bool IsNewInList => _flags.HasFlag(Flags.NewInList);

    internal bool FirstInTable
    {
        get => _flags.HasFlag(Flags.FirstInTable);
        set => SetFlag(Flags.FirstInTable, value);
    }

    internal bool LastInTable
    {
        get => _flags.HasFlag(Flags.LastInTable);
        set => SetFlag(Flags.LastInTable, value);
    }

    internal bool ListCreated
    {
        get => _flags.HasFlag(Flags.ListCreated);
        set => SetFlag(Flags.ListCreated, value);
    }

    internal bool InList
    {
        get => _flags.HasFlag(Flags.InList);
        set => SetFlag(Flags.InList, value);
    }

    public void SetKey(uint key)
    {
        Debug.Assert(!IsInList, "Cannot change key once in a list.");
        Key = key;
        _flags |= Flags.KeySet;
    }

    public void ClearNewRecord() => _flags &= ~Flags.NewRecord;

    public void ClearNewInList() => _flags &= ~Flags.NewInList;

    internal void SetNewInList(bool value) => SetFlag(Flags.NewInList, value);

    protected override void Dispose(bool disposing)
    {
        Debug.Assert(!IsInList, "Node still in list during disposal.");
        base.Dispose(disposing);
    }

    private void SetFlag(Flags flag, bool value)
    {
        if (value)
        {
            _flags |= flag;
        }
        else
        {
            _flags &= ~flag;
        }
    }

    [Flags]
    private enum Flags : uint
    {
        FirstInTable = 1 << 0,
        LastInTable = 1 << 1,
        NewRecord = 1 << 2,
        NewInList = 1 << 3,
        InList = 1 << 4,
        ListCreated = 1 << 5,
        KeySet = 1 << 6,
    }
}
