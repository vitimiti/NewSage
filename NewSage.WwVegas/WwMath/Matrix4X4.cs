// -----------------------------------------------------------------------
// <copyright file="Matrix4X4.cs" company="NewSage">
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NewSage.WwVegas.WwMath;

[StructLayout(LayoutKind.Sequential)]
public struct Matrix4X4 : IEquatable<Matrix4X4>
{
    public Vector4 Row0;
    public Vector4 Row1;
    public Vector4 Row2;
    public Vector4 Row3;

    public Matrix4X4(Matrix4X4 other) => (Row0, Row1, Row2, Row3) = (other.Row0, other.Row1, other.Row2, other.Row3);

    public Matrix4X4(bool identity)
    {
        if (!identity)
        {
            return;
        }

        MakeIdentity();
    }

    public Matrix4X4(Matrix3D matrix) => Initialize(matrix);

    public Matrix4X4(Vector4 v0, Vector4 v1, Vector4 v2, Vector4 v3) => Initialize(v0, v1, v2, v3);

    public Matrix4X4(ReadOnlySpan<float> values) => Initialize(values);

    public readonly Matrix4X4 Transpose =>
        new(
            new Vector4(this[0][0], this[1][0], this[2][0], this[3][0]),
            new Vector4(this[0][1], this[1][1], this[2][1], this[3][1]),
            new Vector4(this[0][2], this[1][2], this[2][2], this[3][2]),
            new Vector4(this[0][3], this[1][3], this[2][3], this[3][3])
        );

    public readonly Matrix4X4 Inverse
    {
        get
        {
            Debug.Fail($"{nameof(Matrix4X4)}.{nameof(Inverse)} does not work, re-implement!");

            Matrix4X4 a = this;
            var b = new Matrix4X4(identity: true);

            int j;
            var i1 = 0;
            for (j = 0; j < 4; j++)
            {
                i1 = j;
                for (var i = j + 1; i < 4; i++)
                {
                    if (float.Abs(a[i][j]) > float.Abs(a[i1][j]))
                    {
                        i1 = i;
                    }
                }
            }

            (a[i1], a[j]) = (a[j], a[i1]);
            (b[i1], b[j]) = (b[j], b[i1]);

            Debug.Assert(
                float.Abs(a[j][j]) > float.Epsilon,
                $"{nameof(Matrix4X4)}.{nameof(Inverse)}: singular matrix; can't invert."
            );

            b[j] /= a[j][j];
            a[j] /= a[j][j];

            for (var i = 0; i < 4; i++)
            {
                if (i == j)
                {
                    continue;
                }

                b[i] -= a[i][j] * b[j];
                a[i] -= a[i][j] * a[j];
            }

            return b;
        }
    }

    public static void TransformVector(Matrix4X4 matrix, Vector3 vector, out Vector3 result) =>
        result = new Vector3(
            (matrix[0][0] * vector.X) + (matrix[0][1] * vector.Y) + (matrix[0][2] * vector.Z) + matrix[0][3],
            (matrix[1][0] * vector.X) + (matrix[1][1] * vector.Y) + (matrix[1][2] * vector.Z) + matrix[1][3],
            (matrix[2][0] * vector.X) + (matrix[2][1] * vector.Y) + (matrix[2][2] * vector.Z) + matrix[2][3]
        );

    public static void TransformVector(Matrix4X4 matrix, Vector3 vector, out Vector4 result) =>
        result = new Vector4(
            (matrix[0][0] * vector.X) + (matrix[0][1] * vector.Y) + (matrix[0][2] * vector.Z) + matrix[0][3],
            (matrix[1][0] * vector.X) + (matrix[1][1] * vector.Y) + (matrix[1][2] * vector.Z) + matrix[1][3],
            (matrix[2][0] * vector.X) + (matrix[2][1] * vector.Y) + (matrix[2][2] * vector.Z) + matrix[2][3],
            1F
        );

    public static void TransformVector(Matrix4X4 matrix, Vector4 vector, out Vector4 result)
    {
        var x =
            (matrix[0][0] * vector.X)
            + (matrix[0][1] * vector.Y)
            + (matrix[0][2] * vector.Z)
            + (matrix[0][3] * vector.W);

        var y =
            (matrix[1][0] * vector.X)
            + (matrix[1][1] * vector.Y)
            + (matrix[1][2] * vector.Z)
            + (matrix[1][3] * vector.W);

        var z =
            (matrix[2][0] * vector.X)
            + (matrix[2][1] * vector.Y)
            + (matrix[2][2] * vector.Z)
            + (matrix[2][3] * vector.W);

        var w =
            (matrix[3][0] * vector.X)
            + (matrix[3][1] * vector.Y)
            + (matrix[3][2] * vector.Z)
            + (matrix[3][3] * vector.W);

        result = new Vector4(x, y, z, w);
    }

    public void MakeIdentity()
    {
        Row0.Set(1, 0, 0, 0);
        Row1.Set(0, 1, 0, 0);
        Row2.Set(0, 0, 1, 0);
        Row3.Set(0, 0, 0, 1);
    }

    public void Initialize(Matrix3D matrix) =>
        (Row0, Row1, Row2, Row3) = (matrix.Row0, matrix.Row1, matrix.Row2, new Vector4(0, 0, 0, 1));

    public void Initialize(Vector4 v0, Vector4 v1, Vector4 v2, Vector4 v3) =>
        (Row0, Row1, Row2, Row3) = (v0, v1, v2, v3);

    public void Initialize(ReadOnlySpan<float> values)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(values.Length, 16);

        Row0.Set(values[0], values[1], values[2], values[3]);
        Row1.Set(values[4], values[5], values[6], values[7]);
        Row2.Set(values[8], values[9], values[10], values[11]);
        Row3.Set(values[12], values[13], values[14], values[15]);
    }

    public void InitializeOrthogonal(
        (float Left, float Right, float Bottom, float Top) sides,
        (float Near, float Far) depth
    )
    {
        Debug.Assert(depth.Near >= 0F, "Near must be non-negative.");
        Debug.Assert(depth.Far > depth.Near, "Far must be greater than near.");

        MakeIdentity();
        Row0[0] = 2F / (sides.Right - sides.Left);
        Row0[3] = -(sides.Right + sides.Left) / (sides.Right - sides.Left);
        Row1[1] = 2F / (sides.Top - sides.Bottom);
        Row1[3] = -(sides.Top + sides.Bottom) / (sides.Top - sides.Bottom);
        Row2[2] = -2F / (depth.Far - depth.Near);
        Row2[3] = -(depth.Far + depth.Near) / (depth.Far - depth.Near);
    }

    public void InitializePerspective((float Horizontal, float Vertical) fov, (float Near, float Far) depth)
    {
        Debug.Assert(depth.Near >= 0F, "Near must be non-negative.");
        Debug.Assert(depth.Far > depth.Near, "Far must be greater than near.");

        MakeIdentity();
        Row0[0] = 1F / float.Tan(fov.Horizontal * .5F);
        Row1[1] = 1F / float.Tan(fov.Vertical * .5F);
        Row2[2] = -(depth.Far + depth.Near) / (depth.Far - depth.Near);
        Row2[3] = -(2F * depth.Far * depth.Near) / (depth.Far - depth.Near);
        Row3[2] = -1F;
        Row3[3] = 0F;
    }

    public void InitializePerspective(
        (float Left, float Right, float Bottom, float Top) sides,
        (float Near, float Far) depth
    )
    {
        Debug.Assert(depth.Near >= 0F, "Near must be non-negative.");
        Debug.Assert(depth.Far > depth.Near, "Far must be greater than near.");

        MakeIdentity();
        Row0[0] = 2F * depth.Near / (sides.Right - sides.Left);
        Row0[2] = (sides.Right + sides.Left) / (sides.Right - sides.Left);
        Row1[1] = 2F * depth.Near / (sides.Top - sides.Bottom);
        Row1[2] = (sides.Top + sides.Bottom) / (sides.Top - sides.Bottom);
        Row2[2] = -(depth.Far + depth.Near) / (depth.Far - depth.Near);
        Row2[3] = -(2F * depth.Far * depth.Near) / (depth.Far - depth.Near);
        Row3[2] = -1F;
        Row3[3] = 0F;
    }

    public readonly Matrix4X4 Add(Matrix4X4 other) =>
        new(Row0 + other.Row0, Row1 + other.Row1, Row2 + other.Row2, Row3 + other.Row3);

    public readonly Matrix4X4 Subtract(Matrix4X4 other) =>
        new(Row0 - other.Row0, Row1 - other.Row1, Row2 - other.Row2, Row3 - other.Row3);

    public readonly Matrix4X4 Multiply(float scalar) => new(Row0 * scalar, Row1 * scalar, Row2 * scalar, Row3 * scalar);

    public readonly Matrix4X4 Multiply(Matrix4X4 other)
    {
        return new Matrix4X4(
            new Vector4(RowCol(this, 0, 0), RowCol(this, 0, 1), RowCol(this, 0, 2), RowCol(this, 0, 3)),
            new Vector4(RowCol(this, 1, 0), RowCol(this, 1, 1), RowCol(this, 1, 2), RowCol(this, 1, 3)),
            new Vector4(RowCol(this, 2, 0), RowCol(this, 2, 1), RowCol(this, 2, 2), RowCol(this, 2, 3)),
            new Vector4(RowCol(this, 3, 0), RowCol(this, 3, 1), RowCol(this, 3, 2), RowCol(this, 3, 3))
        );

        float RowCol(Matrix4X4 self, int i, int j) =>
            (self[i][0] * other[0][j])
            + (self[i][1] * other[1][j])
            + (self[i][2] * other[2][j])
            + (self[i][3] * other[3][j]);
    }

    public readonly Matrix4X4 Multiply(Matrix3D other)
    {
        return new Matrix4X4(
            new Vector4(RowCol(this, 0, 0), RowCol(this, 0, 1), RowCol(this, 0, 2), RowColLast(this, 0, 3)),
            new Vector4(RowCol(this, 1, 0), RowCol(this, 1, 1), RowCol(this, 1, 2), RowColLast(this, 1, 3)),
            new Vector4(RowCol(this, 2, 0), RowCol(this, 2, 1), RowCol(this, 2, 2), RowColLast(this, 2, 3)),
            new Vector4(RowCol(this, 3, 0), RowCol(this, 3, 1), RowCol(this, 3, 2), RowColLast(this, 3, 3))
        );

        float RowCol(Matrix4X4 self, int i, int j) =>
            (self[i][0] * other[0][j]) + (self[i][1] * other[1][j]) + (self[i][2] * other[2][j]);

        float RowColLast(Matrix4X4 self, int i, int j) =>
            (self[i][0] * other[0][j]) + (self[i][1] * other[1][j]) + (self[i][2] * other[2][j]) + self[i][3];
    }

    public readonly Vector4 Multiply(Vector4 vector) =>
        new(
            (this[0][0] * vector[0]) + (this[0][1] * vector[1]) + (this[0][2] * vector[2]) + (this[0][3] * vector[3]),
            (this[1][0] * vector[0]) + (this[1][1] * vector[1]) + (this[1][2] * vector[2]) + (this[1][3] * vector[3]),
            (this[2][0] * vector[0]) + (this[2][1] * vector[1]) + (this[2][2] * vector[2]) + (this[2][3] * vector[3]),
            (this[3][0] * vector[0]) + (this[3][1] * vector[1]) + (this[3][2] * vector[2]) + (this[3][3] * vector[3])
        );

    public readonly Vector4 Multiply(Vector3 vector) =>
        new(
            (this[0][0] * vector[0]) + (this[0][1] * vector[1]) + (this[0][2] * vector[2]) + (this[0][3] * 1F),
            (this[1][0] * vector[0]) + (this[1][1] * vector[1]) + (this[1][2] * vector[2]) + (this[1][3] * 1F),
            (this[2][0] * vector[0]) + (this[2][1] * vector[1]) + (this[2][2] * vector[2]) + (this[2][3] * 1F),
            (this[3][0] * vector[0]) + (this[3][1] * vector[1]) + (this[3][2] * vector[2]) + (this[3][3] * 1F)
        );

    public readonly Matrix4X4 Divide(float scalar)
    {
        // FIX: this is different, in the original code they do `float ood = d`, which is a bug!
        var oneOverScalar = 1F / scalar;
        return Multiply(oneOverScalar);
    }

    public readonly Matrix4X4 Negate() => new(-Row0, -Row1, -Row2, -Row3);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Matrix4X4 other && Equals(other);

    public readonly bool Equals(Matrix4X4 other) =>
        Row0 == other.Row0 && Row1 == other.Row1 && Row2 == other.Row2 && Row3 == other.Row3;

    public override readonly int GetHashCode() => HashCode.Combine(Row0, Row1, Row2, Row3);

    public override readonly string ToString() => $"Matrix4X4({Row0}, {Row1}, {Row2}, {Row3})";

    public static Matrix4X4 operator +(Matrix4X4 x, Matrix4X4 y) => x.Add(y);

    public static Matrix4X4 operator -(Matrix4X4 x, Matrix4X4 y) => x.Subtract(y);

    public static Matrix4X4 operator *(Matrix4X4 x, float scalar) => x.Multiply(scalar);

    public static Matrix4X4 operator *(float scalar, Matrix4X4 x) => x.Multiply(scalar);

    public static Matrix4X4 operator *(Matrix4X4 x, Matrix4X4 y) => x.Multiply(y);

    public static Matrix4X4 operator *(Matrix4X4 x, Matrix3D y) => x.Multiply(y);

    public static Matrix4X4 operator *(Matrix3D x, Matrix4X4 y)
    {
        return new Matrix4X4(
            new Vector4(RowCol(x, 0, 0), RowCol(x, 0, 1), RowCol(x, 0, 2), RowCol(x, 0, 3)),
            new Vector4(RowCol(x, 1, 0), RowCol(x, 1, 1), RowCol(x, 1, 2), RowCol(x, 1, 3)),
            new Vector4(RowCol(x, 2, 0), RowCol(x, 2, 1), RowCol(x, 2, 2), RowCol(x, 2, 3)),
            new Vector4(y[3][0], y[3][1], y[3][2], y[3][3])
        );

        float RowCol(Matrix3D self, int i, int j) =>
            (self[i][0] * y[0][j]) + (self[i][1] * y[1][j]) + (self[i][2] * y[2][j]) + (self[i][3] * y[3][j]);
    }

    public static Vector4 operator *(Matrix4X4 matrix, Vector4 vector) => matrix.Multiply(vector);

    public static Vector4 operator *(Matrix4X4 matrix, Vector3 vector) => matrix.Multiply(vector);

    public static Matrix4X4 operator /(Matrix4X4 x, float scalar) => x.Divide(scalar);

    public static Matrix4X4 operator -(Matrix4X4 x) => x.Negate();

    public static bool operator ==(Matrix4X4 x, Matrix4X4 y) => x.Equals(y);

    public static bool operator !=(Matrix4X4 x, Matrix4X4 y) => !x.Equals(y);

    public Vector4 this[int index]
    {
        readonly get =>
            index switch
            {
                0 => Row0,
                1 => Row1,
                2 => Row2,
                3 => Row3,
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
                    Row0 = value;
                    break;
                case 1:
                    Row1 = value;
                    break;
                case 2:
                    Row2 = value;
                    break;
                case 3:
                    Row3 = value;
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
}
