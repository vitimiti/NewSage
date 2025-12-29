// -----------------------------------------------------------------------
// <copyright file="DoubleNode`2.cs" company="NewSage">
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

public class DoubleNode<TPrimary, TSecondary> : IDisposable
{
    private bool _disposed;

    public DoubleNode()
    {
        Primary = new DataNode<DoubleNode<TPrimary, TSecondary>>(this);
        Secondary = new DataNode<DoubleNode<TPrimary, TSecondary>>(this);
    }

    public DoubleNode(TPrimary primary, TSecondary secondary)
        : this()
    {
        PrimaryValue = primary;
        SecondaryValue = secondary;
    }

    public TPrimary? PrimaryValue { get; set; }

    public TSecondary? SecondaryValue { get; set; }

    public DataNode<DoubleNode<TPrimary, TSecondary>> Primary { get; }

    public DataNode<DoubleNode<TPrimary, TSecondary>> Secondary { get; }

    public void Unlink()
    {
        Primary.Unlink();
        Secondary.Unlink();
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
            Primary.Dispose();
            Secondary.Dispose();
        }

        _disposed = true;
    }
}
