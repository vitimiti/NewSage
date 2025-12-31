// -----------------------------------------------------------------------
// <copyright file="SaveLoadIds.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwSaveLoad;

public static class SaveLoadIds
{
    public const uint ChunkIdSaveLoadBegin = 0x0000_0100;
    public const uint ChunkIdSaveLoadDefMgr = ChunkIdSaveLoadBegin + 1;
    public const uint ChunkIdTwiddler = ChunkIdSaveLoadBegin + 2;
    public const uint ChunkIdWw3DBegin = 0x0001_0000;
    public const uint ChunkIdWwPhysBegin = 0x0002_0000;
    public const uint ChunkIdWwAudioBegin = 0x0003_0000;
    public const uint ChunkIdCombatBegin = 0x0004_0000;
    public const uint ChunkIdCommandoEditorBegin = 0x0005_0000;
    public const uint ChunkIdPhysTestBegin = 0x0006_0000;
    public const uint ChunkIdCommandoBegin = 0x0007_0000;
    public const uint ChunkIdWwMathBegin = 0x0008_0000;
    public const uint ChunkIdWwTranslateDbBegin = 0x0009_0000;
}
