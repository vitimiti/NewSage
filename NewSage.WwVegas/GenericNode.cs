// -----------------------------------------------------------------------
// <copyright file="GenericNode.cs" company="NewSage">
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

namespace NewSage.WwVegas;

public class GenericNode : IDisposable
{
    private bool _disposed;

    public GenericNode? Next => NextNode;

    public GenericNode? NextValid => NextNode?.NextNode != null ? NextNode : null;

    public GenericNode? Previous => PrevNode;

    public GenericNode? PreviousValid => PrevNode?.PrevNode != null ? PrevNode : null;

    public bool IsValid => NextNode != null && PrevNode != null;

    protected GenericNode? NextNode { get; set; }

    protected GenericNode? PrevNode { get; set; }

    public void Unlink()
    {
        if (IsValid)
        {
            PrevNode!.NextNode = NextNode;
            NextNode!.PrevNode = PrevNode;
            PrevNode = null;
            NextNode = null;
        }
    }

    public void Link(GenericNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        node.Unlink();
        node.NextNode = NextNode;
        node.PrevNode = this;
        _ = NextNode?.PrevNode = node;

        NextNode = node;
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
            Unlink();
        }

        _disposed = true;
    }
}
