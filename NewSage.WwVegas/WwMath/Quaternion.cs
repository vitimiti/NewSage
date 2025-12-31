// -----------------------------------------------------------------------
// <copyright file="Quaternion.cs" company="NewSage">
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
using System.Runtime.InteropServices;

namespace NewSage.WwVegas.WwMath;

[StructLayout(LayoutKind.Sequential)]
public struct Quaternion : IEquatable<Quaternion>
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public static Quaternion Slerp(Quaternion x, Quaternion y, float alpha)
    {
        var cosT = (x.X * y.X) + (x.Y * y.Y) + (x.Z * y.Z) + (x.W * y.W);

        bool qFlip;
        if (cosT < 0F)
        {
            cosT = -cosT;
            qFlip = true;
        }
        else
        {
            qFlip = false;
        }

        float beta;
        if (1F - cosT < float.Epsilon * float.Epsilon)
        {
            beta = 1F - alpha;
        }
        else
        {
            var theta = float.Acos(cosT);
            var sinT = float.Sin(theta);
            var oneOverSinT = 1F / sinT;
            beta = float.Sin(theta - (alpha * theta)) * oneOverSinT;
            alpha = float.Sin(alpha * theta) * oneOverSinT;
        }

        if (qFlip)
        {
            alpha = -alpha;
        }

        Quaternion result = default;
        result.X = (beta * x.X) + (alpha * y.X);
        result.Y = (beta * x.Y) + (alpha * y.Y);
        result.Z = (beta * x.Z) + (alpha * y.Z);
        result.W = (beta * x.W) + (alpha * y.W);
        return result;
    }

    public static Quaternion FromMatrix3D(Matrix3D matrix)
    {
        Quaternion result = default;
        var tr = matrix[0][0] + matrix[1][1] + matrix[2][2];
        if (tr > 0F)
        {
            var s = float.Sqrt(tr + 1);
            result[3] = s * .5F;
            s = .5F / s;

            result[0] = (matrix[2][1] - matrix[1][2]) * s;
            result[1] = (matrix[0][2] - matrix[2][0]) * s;
            result[2] = (matrix[1][0] - matrix[0][1]) * s;
        }
        else
        {
            var i = 0;
            if (matrix[1][1] > matrix[0][0])
            {
                i = 1;
            }

            if (matrix[2][2] > matrix[i][i])
            {
                i = 2;
            }

            var j = Next(i);
            var k = Next(j);

            var s = float.Sqrt(matrix[i][i] - (matrix[j][j] + matrix[k][k]) + 1F);

            result[i] = s * .5F;
            if (float.Abs(s) > float.Epsilon)
            {
                s = .5F / s;
            }

            result[3] = (matrix[k][j] - matrix[j][k]) * s;
            result[j] = (matrix[j][i] + matrix[i][j]) * s;
            result[k] = (matrix[k][i] + matrix[i][k]) * s;
        }

        return result;
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Quaternion other && Equals(other);

    public readonly bool Equals(Quaternion other) =>
        float.Abs(X - other.X) < float.Epsilon
        && float.Abs(Y - other.Y) < float.Epsilon
        && float.Abs(Z - other.Z) < float.Epsilon
        && float.Abs(W - other.W) < float.Epsilon;

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    public static bool operator ==(Quaternion x, Quaternion y) => x.Equals(y);

    public static bool operator !=(Quaternion x, Quaternion y) => !x.Equals(y);

    public static explicit operator Quaternion(Matrix3D matrix) => FromMatrix3D(matrix);

    public float this[int index]
    {
        readonly get =>
            index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index must be between 0 and 3 inclusive."
                ),
            };
        set
        {
            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                case 3:
                    W = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        index,
                        "Index must be between 0 and 3 inclusive."
                    );
            }
        }
    }

    private static int Next(int index) =>
        index switch
        {
            0 => 1,
            1 => 2,
            2 => 0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                "Index must be between 0 and 2 inclusive."
            ),
        };
}
