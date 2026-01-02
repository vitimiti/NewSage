// -----------------------------------------------------------------------
// <copyright file="TransferService.cs" company="NewSage">
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

using NewSage.Game.SaveLoad;

namespace NewSage.Game.Transfer;

internal abstract class TransferService
{
    public TransferOptions Options { get; set; } = TransferOptions.None;

    public TransferMode Mode { get; protected set; } = TransferMode.Invalid;

    public string Identifier { get; protected set; } = string.Empty;

    public virtual void Open(string identifier) => Identifier = identifier;

    public abstract void Close();

    public abstract int BeginBlock();

    public abstract void EndBlock();

    public abstract void Skip(int dataSize);

    public abstract void TransferSnapshot(ISnapshot snapshot);

    protected abstract void TransferCore(Span<byte> data);
}
