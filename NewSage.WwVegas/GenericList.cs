// -----------------------------------------------------------------------
// <copyright file="GenericList.cs" company="NewSage">
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

using System.Collections;

namespace NewSage.WwVegas;

public class GenericList : IDisposable, IEnumerable<GenericNode>
{
    private bool _disposed;

    public GenericList() => FirstNode.Link(LastNode);

    public GenericNode? First => FirstNode.Next;

    public GenericNode? FirstValid
    {
        get
        {
            GenericNode? node = FirstNode.Next;
            return node?.Next is not null ? node : null;
        }
    }

    public GenericNode? Last => LastNode.Previous;

    public GenericNode? LastValid
    {
        get
        {
            GenericNode? node = LastNode.Previous;
            return node?.Previous is not null ? node : null;
        }
    }

    public bool IsEmpty => !FirstNode.Next!.IsValid;

    public int ValidCount
    {
        get
        {
            GenericNode? node = FirstValid;
            var counter = 0;
            while (node is not null)
            {
                counter++;
                node = node.NextValid;
            }

            return counter;
        }
    }

    protected GenericNode FirstNode { get; set; } = new();

    protected GenericNode LastNode { get; set; } = new();

    public void AddHead(GenericNode node) => FirstNode.Link(node);

    public void AddTail(GenericNode node) => LastNode.Previous!.Link(node);

    public IEnumerator<GenericNode> GetEnumerator()
    {
        GenericNode? current = FirstValid;
        while (current is not null)
        {
            yield return current;
            current = current.NextValid;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
            while (!IsEmpty)
            {
                First?.Unlink();
            }
        }

        _disposed = true;
    }
}
