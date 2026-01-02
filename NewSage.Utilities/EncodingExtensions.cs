// -----------------------------------------------------------------------
// <copyright file="EncodingExtensions.cs" company="NewSage">
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

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NewSage.Utilities;

[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Extension method.")]
[SuppressMessage(
    "csharpsquid",
    "S1144:Unused private types or members should be removed",
    Justification = "Extension method."
)]
public static class EncodingExtensions
{
    extension(Encoding encoding)
    {
        public string? GetNullTerminatedString([NotNull] byte[] bytes)
        {
            var length = Array.IndexOf(bytes, (byte)0);
            if (length < 0)
            {
                length = bytes.Length;
            }

            return encoding.GetString(bytes[..length]);
        }
    }
}
