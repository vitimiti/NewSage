// -----------------------------------------------------------------------
// <copyright file="RealCrc.cs" company="NewSage">
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

namespace NewSage.WwVegas;

public static class RealCrc
{
    // csharpier-ignore
    private static readonly ulong[] Table =
    [
        0x0000_0000UL, 0x7707_3096UL, 0xEE0E_612CUL, 0x9909_51BAUL, 0x076D_C419UL, 0x706A_F48FUL, 0xE963_A535UL,
        0x9E64_95A3UL, 0x0EDB_8832UL, 0x79DC_B8A4UL, 0xE0D5_E91EUL, 0x97D2_D988UL, 0x09B6_4C2BUL, 0x7EB1_7CBDUL,
        0xE7B8_2D07UL, 0x90BF_1D91UL, 0x1DB7_1064UL, 0x6AB0_20F2UL, 0xF3B9_7148UL, 0x84BE_41DEUL, 0x1ADA_D47DUL,
        0x6DDD_E4EBUL, 0xF4D4_B551UL, 0x83D3_85C7UL, 0x136C_9856UL, 0x646B_A8C0UL, 0xFD62_F97AUL, 0x8A65_C9ECUL,
        0x1401_5C4FUL, 0x6306_6CD9UL, 0xFA0F_3D63UL, 0x8D08_0DF5UL, 0x3B6E_20C8UL, 0x4C69_105EUL, 0xD560_41E4UL,
        0xA267_7172UL, 0x3C03_E4D1UL, 0x4B04_D447UL, 0xD20D_85FDUL, 0xA50A_B56BUL, 0x35B5_A8FAUL, 0x42B2_986CUL,
        0xDBBB_C9D6UL, 0xACBC_F940UL, 0x32D8_6CE3UL, 0x45DF_5C75UL, 0xDCD6_0DCFUL, 0xABD1_3D59UL, 0x26D9_30ACUL,
        0x51DE_003AUL, 0xC8D7_5180UL, 0xBFD0_6116UL, 0x21B4_F4B5UL, 0x56B3_C423UL, 0xCFBA_9599UL, 0xB8BD_A50FUL,
        0x2802_B89EUL, 0x5F05_8808UL, 0xC60C_D9B2UL, 0xB10B_E924UL, 0x2F6F_7C87UL, 0x5868_4C11UL, 0xC161_1DABUL,
        0xB666_2D3DUL, 0x76DC_4190UL, 0x01DB_7106UL, 0x98D2_20BCUL, 0xEFD5_102AUL, 0x71B1_8589UL, 0x06B6_B51FUL,
        0x9FBF_E4A5UL, 0xE8B8_D433UL, 0x7807_C9A2UL, 0x0F00_F934UL, 0x9609_A88EUL, 0xE10E_9818UL, 0x7F6A_0DBBUL,
        0x086D_3D2DUL, 0x9164_6C97UL, 0xE663_5C01UL, 0x6B6B_51F4UL, 0x1C6C_6162UL, 0x8565_30D8UL, 0xF262_004EUL,
        0x6C06_95EDUL, 0x1B01_A57BUL, 0x8208_F4C1UL, 0xF50F_C457UL, 0x65B0_D9C6UL, 0x12B7_E950UL, 0x8BBE_B8EAUL,
        0xFCB9_887CUL, 0x62DD_1DDFUL, 0x15DA_2D49UL, 0x8CD3_7CF3UL, 0xFBD4_4C65UL, 0x4DB2_6158UL, 0x3AB5_51CEUL,
        0xA3BC_0074UL, 0xD4BB_30E2UL, 0x4ADF_A541UL, 0x3DD8_95D7UL, 0xA4D1_C46DUL, 0xD3D6_F4FBUL, 0x4369_E96AUL,
        0x346E_D9FCUL, 0xAD67_8846UL, 0xDA60_B8D0UL, 0x4404_2D73UL, 0x3303_1DE5UL, 0xAA0A_4C5FUL, 0xDD0D_7CC9UL,
        0x5005_713CUL, 0x2702_41AAUL, 0xBE0B_1010UL, 0xC90C_2086UL, 0x5768_B525UL, 0x206F_85B3UL, 0xB966_D409UL,
        0xCE61_E49FUL, 0x5EDE_F90EUL, 0x29D9_C998UL, 0xB0D0_9822UL, 0xC7D7_A8B4UL, 0x59B3_3D17UL, 0x2EB4_0D81UL,
        0xB7BD_5C3BUL, 0xC0BA_6CADUL, 0xEDB8_8320UL, 0x9ABF_B3B6UL, 0x03B6_E20CUL, 0x74B1_D29AUL, 0xEAD5_4739UL,
        0x9DD2_77AFUL, 0x04DB_2615UL, 0x73DC_1683UL, 0xE363_0B12UL, 0x9464_3B84UL, 0x0D6D_6A3EUL, 0x7A6A_5AA8UL,
        0xE40E_CF0BUL, 0x9309_FF9DUL, 0x0A00_AE27UL, 0x7D07_9EB1UL, 0xF00F_9344UL, 0x8708_A3D2UL, 0x1E01_F268UL,
        0x6906_C2FEUL, 0xF762_575DUL, 0x8065_67CBUL, 0x196C_3671UL, 0x6E6B_06E7UL, 0xFED4_1B76UL, 0x89D3_2BE0UL,
        0x10DA_7A5AUL, 0x67DD_4ACCUL, 0xF9B9_DF6FUL, 0x8EBE_EFF9UL, 0x17B7_BE43UL, 0x60B0_8ED5UL, 0xD6D6_A3E8UL,
        0xA1D1_937EUL, 0x38D8_C2C4UL, 0x4FDF_F252UL, 0xD1BB_67F1UL, 0xA6BC_5767UL, 0x3FB5_06DDUL, 0x48B2_364BUL,
        0xD80D_2BDAUL, 0xAF0A_1B4CUL, 0x3603_4AF6UL, 0x4104_7A60UL, 0xDF60_EFC3UL, 0xA867_DF55UL, 0x316E_8EEFUL,
        0x4669_BE79UL, 0xCB61_B38CUL, 0xBC66_831AUL, 0x256F_D2A0UL, 0x5268_E236UL, 0xCC0C_7795UL, 0xBB0B_4703UL,
        0x2202_16B9UL, 0x5505_262FUL, 0xC5BA_3BBEUL, 0xB2BD_0B28UL, 0x2BB4_5A92UL, 0x5CB3_6A04UL, 0xC2D7_FFA7UL,
        0xB5D0_CF31UL, 0x2CD9_9E8BUL, 0x5BDE_AE1DUL, 0x9B64_C2B0UL, 0xEC63_F226UL, 0x756A_A39CUL, 0x026D_930AUL,
        0x9C09_06A9UL, 0xEB0E_363FUL, 0x7207_6785UL, 0x0500_5713UL, 0x95BF_4A82UL, 0xE2B8_7A14UL, 0x7BB1_2BAEUL,
        0x0CB6_1B38UL, 0x92D2_8E9BUL, 0xE5D5_BE0DUL, 0x7CDC_EFB7UL, 0x0BDB_DF21UL, 0x86D3_D2D4UL, 0xF1D4_E242UL,
        0x68DD_B3F8UL, 0x1FDA_836EUL, 0x81BE_16CDUL, 0xF6B9_265BUL, 0x6FB0_77E1UL, 0x18B7_4777UL, 0x8808_5AE6UL,
        0xFF0F_6A70UL, 0x6606_3BCAUL, 0x1101_0B5CUL, 0x8F65_9EFFUL, 0xF862_AE69UL, 0x616B_FFD3UL, 0x166C_CF45UL,
        0xA00A_E278UL, 0xD70D_D2EEUL, 0x4E04_8354UL, 0x3903_B3C2UL, 0xA767_2661UL, 0xD060_16F7UL, 0x4969_474DUL,
        0x3E6E_77DBUL, 0xAED1_6A4AUL, 0xD9D6_5ADCUL, 0x40DF_0B66UL, 0x37D8_3BF0UL, 0xA9BC_AE53UL, 0xDEBB_9EC5UL,
        0x47B2_CF7FUL, 0x30B5_FFE9UL, 0xBDBD_F21CUL, 0xCABA_C28AUL, 0x53B3_9330UL, 0x24B4_A3A6UL, 0xBAD0_3605UL,
        0xCDD7_0693UL, 0x54DE_5729UL, 0x23D9_67BFUL, 0xB366_7A2EUL, 0xC461_4AB8UL, 0x5D68_1B02UL, 0x2A6F_2B94UL,
        0xB40B_BE37UL, 0xC30C_8EA1UL, 0x5A05_DF1BUL, 0x2D02_EF8DUL
    ];

    public static ulong Memory(ReadOnlySpan<byte> data, ulong crc = 0)
    {
        crc ^= 0xFFFF_FFFF;
        var length = data.Length;
        var dataIndex = 0;
        while (length-- > 0)
        {
            crc = Crc32(data[dataIndex++], crc);
        }

        return crc ^ 0xFFFF_FFFF;
    }

    public static ulong String(string str, ulong crc = 0)
    {
        crc ^= 0xFFFF_FFFF;
        var bytes = Encoding.ASCII.GetBytes(str);
        crc = bytes.Aggregate(crc, (current, b) => Crc32(b, current));
        return crc ^ 0xFFFF_FFFF;
    }

    public static ulong StringIgnoreCase(string str, ulong crc = 0)
    {
        ArgumentNullException.ThrowIfNull(str);
        return String(str.ToUpperInvariant(), crc);
    }

    private static ulong Crc32(byte c, ulong crc) => Table[(crc ^ c) & 0xFFL] ^ ((crc >> 8) & 0x00FF_FFFFL);
}
