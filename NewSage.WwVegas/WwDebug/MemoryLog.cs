// -----------------------------------------------------------------------
// <copyright file="MemoryLog.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwDebug;

public static class MemoryLog
{
    private static readonly long[] Allocations = new long[Enum.GetValues<MemoryCategory>().Length];
    private static readonly long[] Deallocations = new long[Enum.GetValues<MemoryCategory>().Length];

    public static MemoryCategory Current => Stack.Count > 0 ? Stack.Peek() : MemoryCategory.Unknown;

    [field: ThreadStatic]
    private static Stack<MemoryCategory> Stack => field ??= new Stack<MemoryCategory>();

    public static void Push(MemoryCategory category) => Stack.Push(category);

    public static void Pop()
    {
        if (Stack.Count > 0)
        {
            _ = Stack.Pop();
        }
    }

    public static void RegisterAlloc(MemoryCategory category, long count = 1) =>
        Interlocked.Add(ref Allocations[(int)category], count);

    public static void RegisterFree(MemoryCategory category, long count = 1) =>
        Interlocked.Add(ref Deallocations[(int)category], count);

    public static long GetActiveCount(MemoryCategory category) =>
        Interlocked.Read(ref Allocations[(int)category]) - Interlocked.Read(ref Deallocations[(int)category]);

    public static (long Total, long Active) GetStats(MemoryCategory category)
    {
        var alloc = Interlocked.Read(ref Allocations[(int)category]);
        var free = Interlocked.Read(ref Deallocations[(int)category]);
        return (alloc, alloc - free);
    }
}
