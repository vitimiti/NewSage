// -----------------------------------------------------------------------
// <copyright file="Matrix3X3.cs" company="NewSage">
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
public struct Matrix3X3 : IEquatable<Matrix3X3>
{
    public Matrix3X3() { }

    public Matrix3X3(Matrix3X3 other) => Set(other.Row0, other.Row1, other.Row2);

    public Matrix3X3(bool identity)
    {
        if (!identity)
        {
            return;
        }

        Row0.Set(1, 0, 0);
        Row1.Set(0, 1, 0);
        Row2.Set(0, 0, 1);
    }

    public Matrix3X3(Vector3 r0, Vector3 r1, Vector3 r2) => Set(r0, r1, r2);

    public Matrix3X3(Matrix3D matrix) => Set(matrix);

    public Matrix3X3(Matrix4X4 matrix) => Set(matrix);

    public Matrix3X3(ReadOnlySpan<float> values) => Set(values);

    public Matrix3X3(Vector3 axis, float angle) => Set(axis, angle);

    public Matrix3X3(Vector3 axis, float sin, float cos) => Set(axis, sin, cos);

    public Matrix3X3(Quaternion quaternion) => Set(quaternion);

    // csharpier-ignore
    public static Matrix3X3 Identity => new(
        [
            1F, 0F, 0F,
            0F, 1F, 0F,
            0F, 0F, 1F
        ]
    );

    // csharpier-ignore
    public static Matrix3X3 RotateX90F => new(
        [
            1F, 0F, 0F,
            0F, 0F, -1F,
            0F, 1F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3X3 RotateX1F80F => new(
        [
            1F, 0F, 0F,
            0F, -1F, 0F,
            0F, 0F, -1F
        ]
    );

    // csharpier-ignore
    public static Matrix3X3 RotateX270F => new(
        [
            1F, 0F, 0F,
            0F, 0F, 1F,
            0F, -1F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3X3 RotateY90F => new(
        [
            0F, 0F, 1F,
            0F, 1F, 0F,
            -1F, 0F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3X3 RotateY1F80F => new(
        [
            -1F, 0F, 0F,
            0F, 1F, 0F,
            0F, 0F, -1F
        ]
    );

    // csharpier-ignore
    public static Matrix3X3 RotateY270F => new(
        [
            0F, 0F, -1F,
            0F, 1F, 0F,
            1F, 0F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3X3 RotateZ90F => new(
        [
            0F, -1F, 0F,
            1F, 0F, 0F,
            0F, 0F, 1F
        ]
    );

    // csharpier-ignore
    public static Matrix3X3 RotateZ1F80F => new(
        [
            -1F, 0F, 0F,
            0F, -1F, 0F,
            0F, 0F, 1F
        ]
    );

    // csharpier-ignore
    public static Matrix3X3 RotateZ270F => new(
        [
            0F, 1F, 0F,
            -1F, 0F, 0F,
            0F, 0F, 1F
        ]
    );

    public Vector3 Row0;
    public Vector3 Row1;
    public Vector3 Row2;

    public readonly Matrix3X3 Transpose =>
        new(
            new Vector3(Row0[0], Row1[0], Row2[0]),
            new Vector3(Row0[1], Row1[1], Row2[1]),
            new Vector3(Row0[2], Row1[2], Row2[2])
        );

    public readonly Matrix3X3 Inverse
    {
        get
        {
            Matrix3X3 a = this;
            Matrix3X3 b = new(identity: true);

            for (var j = 0; j < 3; j++)
            {
                var i1 = j;
                for (var i = j + 1; i < 3; i++)
                {
                    if (float.Abs(a[i][j]) > float.Abs(a[i1][j]))
                    {
                        i1 = i;
                    }
                }

                if (i1 != j)
                {
                    (a[j], a[i1]) = (a[i1], a[j]);
                    (b[j], b[i1]) = (b[i1], b[j]);
                }

                var pivot = a[j][j];
                if (float.Abs(pivot) < float.Epsilon)
                {
                    continue;
                }

                var oneOverPivot = 1.0f / pivot;
                a[j] *= oneOverPivot;
                b[j] *= oneOverPivot;

                for (var i = 0; i < 3; i++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var factor = a[i][j];
                    b[i] -= factor * b[j];
                    a[i] -= factor * a[j];
                }
            }

            return b;
        }
    }

    public readonly float Determinant =>
        (Row0[0] * ((Row1[1] * Row2[2]) - (Row1[2] * Row2[1])))
        - (Row0[1] * ((Row1[0] * Row2[2]) - (Row1[2] * Row2[0])))
        - (Row0[2] * ((Row1[0] * Row2[1]) - (Row1[1] * Row2[0])));

    public readonly float XRotation
    {
        get
        {
            Vector3 v = this * new Vector3(0, 1, 0);
            return float.Atan2(v[2], v[1]);
        }
    }

    public readonly float YRotation
    {
        get
        {
            Vector3 v = this * new Vector3(0, 0, 1);
            return float.Atan2(v[0], v[2]);
        }
    }

    public readonly float ZRotation
    {
        get
        {
            Vector3 v = this * new Vector3(1, 0, 0);
            return float.Atan2(v[1], v[0]);
        }
    }

    public readonly Vector3 XVector => new(Row0);

    public readonly Vector3 YVector => new(Row1);

    public readonly Vector3 ZVector => new(Row2);

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Worse readability.")]
    public readonly bool IsOrthogonal
    {
        get
        {
            Vector3 x = XVector;
            Vector3 y = YVector;
            Vector3 z = ZVector;

            if (Vector3.DotProduct(x, y) > float.Epsilon)
            {
                return false;
            }

            if (Vector3.DotProduct(y, z) > float.Epsilon)
            {
                return false;
            }

            if (Vector3.DotProduct(z, x) > float.Epsilon)
            {
                return false;
            }

            if (float.Abs(x.Length - 1F) > float.Epsilon)
            {
                return false;
            }

            if (float.Abs(y.Length - 1F) > float.Epsilon)
            {
                return false;
            }

            if (float.Abs(z.Length - 1F) > float.Epsilon)
            {
                return false;
            }

            return true;
        }
    }

    public static Matrix3X3 CreateXRotationMatrix3(float sin, float cos)
    {
        Matrix3X3 result = default;
        result.Row0.Set(1, 0, 0);
        result.Row1.Set(0, cos, -sin);
        result.Row2.Set(0, sin, cos);
        return result;
    }

    public static Matrix3X3 CreateXRotationMatrix3(float rad) => CreateXRotationMatrix3(float.Sin(rad), float.Cos(rad));

    public static Matrix3X3 CreateYRotationMatrix3(float sin, float c)
    {
        Matrix3X3 result = default;
        result.Row0.Set(c, 0, sin);
        result.Row1.Set(0, 1, 0);
        result.Row2.Set(-sin, 0, c);
        return result;
    }

    public static Matrix3X3 CreateYRotationMatrix3(float rad) => CreateYRotationMatrix3(float.Sin(rad), float.Cos(rad));

    public static Matrix3X3 CreateZRotationMatrix3(float sin, float c)
    {
        Matrix3X3 result = default;
        result.Row0.Set(c, -sin, 0);
        result.Row1.Set(sin, c, 0);
        result.Row2.Set(0, 0, 1);
        return result;
    }

    public static Matrix3X3 CreateZRotationMatrix3(float rad) => CreateZRotationMatrix3(float.Sin(rad), float.Cos(rad));

    public static Vector3 RotateVector(Matrix3X3 matrix, Vector3 vector) =>
        new(
            (matrix[0][0] * vector.X) + (matrix[0][1] * vector.Y) + (matrix[0][2] * vector.Z),
            (matrix[1][0] * vector.X) + (matrix[1][1] * vector.Y) + (matrix[1][2] * vector.Z),
            (matrix[2][0] * vector.X) + (matrix[2][1] * vector.Y) + (matrix[2][2] * vector.Z)
        );

    public static Vector3 RotateTransposeVector(Matrix3X3 matrix, Vector3 vector) =>
        new(
            (matrix[0][0] * vector.X) + (matrix[1][0] * vector.Y) + (matrix[2][0] * vector.Z),
            (matrix[0][1] * vector.X) + (matrix[1][1] * vector.Y) + (matrix[2][1] * vector.Z),
            (matrix[0][2] * vector.X) + (matrix[1][2] * vector.Y) + (matrix[2][2] * vector.Z)
        );

    public static Matrix3X3 FromMatrix3D(Matrix3D matrix)
    {
        Matrix3X3 result = default;
        result.Set(matrix);
        return result;
    }

    public static Matrix3X3 FromMatrix4X4(Matrix4X4 matrix)
    {
        Matrix3X3 result = default;
        result.Set(matrix);
        return result;
    }

    public void Set(Matrix3D matrix)
    {
        Row0.Set(matrix[0][0], matrix[0][1], matrix[0][2]);
        Row1.Set(matrix[1][0], matrix[1][1], matrix[1][2]);
        Row2.Set(matrix[2][0], matrix[2][1], matrix[2][2]);
    }

    public void Set(Matrix4X4 matrix)
    {
        Row0.Set(matrix[0][0], matrix[0][1], matrix[0][2]);
        Row1.Set(matrix[1][0], matrix[1][1], matrix[1][2]);
        Row2.Set(matrix[2][0], matrix[2][1], matrix[2][2]);
    }

    public void Set(Vector3 r0, Vector3 r1, Vector3 r2) => (Row0, Row1, Row2) = (r0, r1, r2);

    public void Set(ReadOnlySpan<float> values)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(values.Length, 9);
        Row0.Set(values[0], values[1], values[2]);
        Row1.Set(values[3], values[4], values[5]);
        Row2.Set(values[6], values[7], values[8]);
    }

    public void Set(Vector3 axis, float angle) => Set(axis, float.Sin(angle), float.Cos(angle));

    public void Set(Vector3 axis, float sin, float cos)
    {
        Debug.Assert(float.Abs(axis.Length2 - 1) < float.Epsilon, "Axis must be a unit vector.");

        Row0.Set(
            (axis[0] * axis[0]) + (cos * (1F - (axis[0] * axis[0]))),
            (axis[0] * axis[1] * (1F - cos)) - (axis[2] * sin),
            (axis[2] * axis[0] * (1F - cos)) + (axis[1] * sin)
        );

        Row1.Set(
            (axis[0] * axis[1] * (1F - cos)) + (axis[2] * sin),
            (axis[1] * axis[1]) + (cos * (1F - (axis[1] * axis[1]))),
            (axis[1] * axis[2] * (1F - cos)) - (axis[0] * sin)
        );

        Row2.Set(
            (axis[2] * axis[0] * (1F - cos)) - (axis[1] * sin),
            (axis[1] * axis[2] * (1F - cos)) + (axis[0] * sin),
            (axis[2] * axis[2]) + (cos * (1 - (axis[2] * axis[2])))
        );
    }

    public void Set(Quaternion quaternion)
    {
        Row0[0] = 1F - (2F * ((quaternion[1] * quaternion[1]) + (quaternion[2] * quaternion[2])));
        Row0[1] = 2F * ((quaternion[0] * quaternion[1]) - (quaternion[2] * quaternion[3]));
        Row0[2] = 2F * ((quaternion[2] * quaternion[0]) + (quaternion[1] * quaternion[3]));

        Row1[0] = 2F * ((quaternion[0] * quaternion[1]) + (quaternion[2] * quaternion[3]));
        Row1[1] = 1F - (2F * ((quaternion[2] * quaternion[2]) + (quaternion[0] * quaternion[0])));
        Row1[2] = 2F * ((quaternion[1] * quaternion[2]) - (quaternion[0] * quaternion[3]));

        Row2[0] = 2F * ((quaternion[2] * quaternion[0]) - (quaternion[1] * quaternion[3]));
        Row2[1] = 2F * ((quaternion[1] * quaternion[2]) + (quaternion[0] * quaternion[3]));
        Row2[2] = 1F - (2F * ((quaternion[1] * quaternion[1]) + (quaternion[0] * quaternion[0])));
    }

    public void MakeIdentity()
    {
        Row0.Set(1, 0, 0);
        Row1.Set(0, 1, 0);
        Row2.Set(0, 0, 1);
    }

    public void RotateX(float theta) => RotateX(float.Sin(theta), float.Cos(theta));

    public void RotateX(float sin, float cos)
    {
        var temp1 = Row0[1];
        var temp2 = Row0[2];
        Row0[1] = (cos * temp1) + (sin * temp2);
        Row0[2] = (-sin * temp1) + (cos * temp2);

        temp1 = Row1[1];
        temp2 = Row1[2];
        Row1[1] = (cos * temp1) + (sin * temp2);
        Row1[2] = (-sin * temp1) + (cos * temp2);

        temp1 = Row2[1];
        temp2 = Row2[2];
        Row2[1] = (cos * temp1) + (sin * temp2);
        Row2[2] = (-sin * temp1) + (cos * temp2);
    }

    public void RotateY(float theta) => RotateY(float.Sin(theta), float.Cos(theta));

    public void RotateY(float sin, float cos)
    {
        var temp1 = Row0[0];
        var temp2 = Row0[2];
        Row0[0] = (cos * temp1) - (sin * temp2);
        Row0[2] = (sin * temp1) + (cos * temp2);

        temp1 = Row1[0];
        temp2 = Row1[2];
        Row1[0] = (cos * temp1) - (sin * temp2);
        Row1[2] = (sin * temp1) + (cos * temp2);

        temp1 = Row2[0];
        temp2 = Row2[2];
        Row2[0] = (cos * temp1) - (sin * temp2);
        Row2[2] = (sin * temp1) + (cos * temp2);
    }

    public void RotateZ(float theta) => RotateZ(float.Sin(theta), float.Cos(theta));

    public void RotateZ(float sin, float cos)
    {
        var tmp1 = Row0[0];
        var tmp2 = Row0[1];
        Row0[0] = (cos * tmp1) + (sin * tmp2);
        Row0[1] = (-sin * tmp1) + (cos * tmp2);

        tmp1 = Row1[0];
        tmp2 = Row1[1];
        Row1[0] = (cos * tmp1) + (sin * tmp2);
        Row1[1] = (-sin * tmp1) + (cos * tmp2);

        tmp1 = Row2[0];
        tmp2 = Row2[1];
        Row2[0] = (cos * tmp1) + (sin * tmp2);
        Row2[1] = (-sin * tmp1) + (cos * tmp2);
    }

    public void ReOrthogonalize()
    {
        Vector3 x = XVector;
        Vector3 y = YVector;
        var z = Vector3.CrossProduct(Vector3.CrossProduct(x, y), x);

        var length = x.Length;
        if (length < float.Epsilon)
        {
            MakeIdentity();
            return;
        }

        x /= length;

        length = y.Length;
        if (length < float.Epsilon)
        {
            MakeIdentity();
            return;
        }

        y /= length;

        length = z.Length;
        if (length < float.Epsilon)
        {
            MakeIdentity();
            return;
        }

        z /= length;

        Row0.Set(x.X, x.Y, x.Z);
        Row1.Set(y.X, y.Y, y.Z);
        Row2.Set(z.X, z.Y, z.Z);
    }

    public readonly Vector3 RotateAxisAlignedBoxExtent(Vector3 extent)
    {
        Vector3 result = default;
        for (var i = 0; i < 3; i++)
        {
            result[i] = 0F;
            for (var j = 0; j < 3; j++)
            {
                result[i] += float.Abs(this[i][j] * extent[j]);
            }
        }

        return result;
    }

    public readonly Matrix3X3 Add(Matrix3X3 other) => new(Row0 + other.Row0, Row1 + other.Row1, Row2 + other.Row2);

    public readonly Matrix3X3 Subtract(Matrix3X3 other) => new(Row0 - other.Row0, Row1 - other.Row1, Row2 - other.Row2);

    public readonly Matrix3X3 Multiply(float scalar) => new(Row0 * scalar, Row1 * scalar, Row2 * scalar);

    public readonly Matrix3X3 Multiply(Matrix3X3 other)
    {
        Matrix3X3 current = this;
        return new Matrix3X3
        {
            Row0 = new Vector3(RowCol(0, 0), RowCol(0, 1), RowCol(0, 2)),
            Row1 = new Vector3(RowCol(1, 0), RowCol(1, 1), RowCol(1, 2)),
            Row2 = new Vector3(RowCol(2, 0), RowCol(2, 1), RowCol(2, 2)),
        };

        float RowCol(int i, int j) =>
            (current[i][0] * other[0][j]) + (current[i][1] * other[1][j]) + (current[i][2] * other[2][j]);
    }

    public readonly Matrix3X3 Multiply(Matrix3D other)
    {
        Matrix3X3 current = this;
        return new Matrix3X3
        {
            Row0 = new Vector3(RowCol(0, 0), RowCol(0, 1), RowCol(0, 2)),
            Row1 = new Vector3(RowCol(1, 0), RowCol(1, 1), RowCol(1, 2)),
            Row2 = new Vector3(RowCol(2, 0), RowCol(2, 1), RowCol(2, 2)),
        };

        float RowCol(int i, int j) =>
            (current[i][0] * other[0][j]) + (current[i][1] * other[1][j]) + (current[i][2] * other[2][j]);
    }

    public readonly Vector3 Multiply(Vector3 vector) =>
        new(
            (this[0][0] * vector[0]) + (this[0][1] * vector[1]) + (this[0][2] * vector[2]),
            (this[1][0] * vector[0]) + (this[1][1] * vector[1]) + (this[1][2] * vector[2]),
            (this[2][0] * vector[0]) + (this[2][1] * vector[1]) + (this[2][2] * vector[2])
        );

    public readonly Matrix3X3 Divide(float scalar) => new(Row0 / scalar, Row1 / scalar, Row2 / scalar);

    public readonly Matrix3X3 Negate() => new(-Row0, -Row1, -Row2);

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Matrix3X3 other && Equals(other);

    public readonly bool Equals(Matrix3X3 other) => Row0 == other.Row0 && Row1 == other.Row1 && Row2 == other.Row2;

    public override readonly int GetHashCode() => HashCode.Combine(Row0, Row1, Row2);

    public override readonly string ToString() => $"Matrix3X3({Row0}, {Row1}, {Row2})";

    public static Matrix3X3 operator +(Matrix3X3 x, Matrix3X3 y) => x.Add(y);

    public static Matrix3X3 operator -(Matrix3X3 x, Matrix3X3 y) => x.Subtract(y);

    public static Matrix3X3 operator *(Matrix3X3 matrix, float scalar) => matrix.Multiply(scalar);

    public static Matrix3X3 operator *(float scalar, Matrix3X3 matrix) => matrix.Multiply(scalar);

    public static Matrix3X3 operator *(Matrix3X3 x, Matrix3X3 y) => x.Multiply(y);

    public static Matrix3X3 operator *(Matrix3D x, Matrix3X3 y) => y.Multiply(x);

    public static Matrix3X3 operator *(Matrix3X3 x, Matrix3D y) => x.Multiply(y);

    public static Vector3 operator *(Matrix3X3 matrix, Vector3 vector) => matrix.Multiply(vector);

    public static Matrix3X3 operator /(Matrix3X3 matrix, float scalar) => matrix.Divide(scalar);

    public static Matrix3X3 operator -(Matrix3X3 matrix) => matrix.Negate();

    public static bool operator ==(Matrix3X3 x, Matrix3X3 y) => x.Equals(y);

    public static bool operator !=(Matrix3X3 x, Matrix3X3 y) => !x.Equals(y);

    public static explicit operator Matrix3X3(Matrix3D matrix) => FromMatrix3D(matrix);

    public static explicit operator Matrix3X3(Matrix4X4 matrix) => FromMatrix4X4(matrix);

    public Vector3 this[int index]
    {
        readonly get =>
            index switch
            {
                0 => Row0,
                1 => Row1,
                2 => Row2,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index must be between 0 and 2 inclusive."
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
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        index,
                        "Index must be between 0 and 2 inclusive."
                    );
            }
        }
    }
}
