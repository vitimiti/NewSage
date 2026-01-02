// -----------------------------------------------------------------------
// <copyright file="Bucket.cs" company="NewSage">
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

using NewSage.Utilities;

namespace NewSage.Game.Utilities;

[MemoryPooled("NameKeyBucketPool", initialSize: 9_000, overflowSize: 1_024)]
internal sealed partial class Bucket : IPoolable
{
    public Bucket? NextInSocket { get; set; }

    public NameKeyType Key { get; set; } = NameKeyType.Invalid;

    public string Name { get; set; } = string.Empty;

    public void Reset()
    {
        NextInSocket = null;
        Key = NameKeyType.Invalid;
        Name = string.Empty;
    }
}
