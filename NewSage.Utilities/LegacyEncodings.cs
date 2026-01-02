// -----------------------------------------------------------------------
// <copyright file="LegacyEncodings.cs" company="NewSage">
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

using System.Text;

namespace NewSage.Utilities;

public static class LegacyEncodings
{
    static LegacyEncodings()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Cache the ANSI encoding for later use
        Ansi = Encoding.GetEncoding(1252);
    }

    public static Encoding Ansi { get; }
}
