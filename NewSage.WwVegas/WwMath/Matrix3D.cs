// -----------------------------------------------------------------------
// <copyright file="Matrix3D.cs" company="NewSage">
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
using System.Numerics;
using System.Runtime.InteropServices;

namespace NewSage.WwVegas.WwMath;

[StructLayout(LayoutKind.Sequential)]
public struct Matrix3D : IEquatable<Matrix3D>
{
    public Matrix3D(bool identity)
    {
        if (!identity)
        {
            return;
        }

        MakeIdentity();
    }

    public Matrix3D(ReadOnlySpan<float> values) => Set(values);

    public Matrix3D(Vector3 x, Vector3 y, Vector3 z, Vector3 position) => Set(x, y, z, position);

    public Matrix3D(Vector3 axis, float angle) => Set(axis, angle);

    public Matrix3D(Vector3 axis, float sin, float cos) => Set(axis, sin, cos);

    public Matrix3D(Matrix3X3 rotation, Vector3 position) => Set(rotation, position);

    public Matrix3D(Quaternion rotation, Vector3 position) => Set(rotation, position);

    public Matrix3D(Vector3 position) => Set(position);

    public Matrix3D(Matrix3D other) => (Row0, Row1, Row2) = (other.Row0, other.Row1, other.Row2);

    // csharpier-ignore
    public static Matrix3D Identity => new(
        [
            1F, 0F, 0F, 0F,
            0F, 1F, 0F, 0F,
            0F, 0F, 1F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3D RotateX90 => new(
        [
            1F, 0F, 0F, 0F,
            0F, 0F, -1F, 0F,
            0F, 1F, 0F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3D RotateX180 => new(
        [
            1F, 0F, 0F, 0F,
            0F, -1F, 0F, 0F,
            0F, 0F, -1F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3D RotateX270 => new(
        [
            1F, 0F, 0F, 0F,
            0F, 0F, 1F, 0F,
            0F, -1F, 0F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3D RotateY90 => new(
        [
            0F, 0F, 1F, 0F,
            0F, 1F, 0F, 0F,
            -1F, 0F, 0F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3D RotateY180 => new(
        [
            -1F, 0F, 0F, 0F,
            0F, 1F, 0F, 0F,
            0F, 0F, -1F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3D RotateY270 => new(
        [
            0F, 0F, -1F, 0F,
            0F, 1F, 0F, 0F,
            1F, 0F, 0F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3D RotateZ90 => new
    (
        [
            0F, -1F, 0F, 0F,
            1F, 0F, 0F, 0F,
            0F, 0F, 1F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3D RotateZ180 => new(
        [
            -1F, 0F, 0F, 0F,
            0F, -1F, 0F, 0F,
            0F, 0F, 1F, 0F
        ]
    );

    // csharpier-ignore
    public static Matrix3D RotateZ270 => new
    (
        [
            0F, 1F, 0F, 0F,
            -1F, 0F, 0F, 0F,
            0F, 0F, 1F, 0F
        ]
    );

    public Vector4 Row0;
    public Vector4 Row1;
    public Vector4 Row2;

    public Vector3 Translation
    {
        readonly get => new(this[0][3], this[1][3], this[2][3]);
        set
        {
            Row0[3] = value[0];
            Row1[3] = value[1];
            Row2[3] = value[2];
        }
    }

    public float XTranslation
    {
        readonly get => this[0][3];
        set => Row0[3] = value;
    }

    public float YTranslation
    {
        readonly get => this[1][3];
        set => Row1[3] = value;
    }

    public float ZTranslation
    {
        readonly get => this[2][3];
        set => Row2[3] = value;
    }

    public readonly float XRotation => float.Atan2(this[2][1], this[1][1]);

    public readonly float YRotation => float.Atan2(this[0][2], this[2][2]);

    public readonly float ZRotation => float.Atan2(this[1][0], this[0][0]);

    public readonly Vector3 XVector => new(this[0][0], this[1][0], this[2][0]);

    public readonly Vector3 YVector => new(this[0][1], this[1][1], this[2][1]);

    public readonly Vector3 ZVector => new(this[0][2], this[1][2], this[2][2]);

    public readonly Matrix3D Inverse
    {
        get
        {
            var matrix = new Matrix4x4(
                this[0][0],
                this[0][1],
                this[0][2],
                this[0][3],
                this[1][0],
                this[1][1],
                this[1][2],
                this[1][3],
                this[2][0],
                this[2][1],
                this[2][2],
                this[2][3],
                0,
                0,
                0,
                1
            );

            return Matrix4x4.Invert(matrix, out Matrix4x4 result)
                ? new Matrix3D(
                    [
                        result.M11,
                        result.M12,
                        result.M13,
                        result.M14,
                        result.M21,
                        result.M22,
                        result.M23,
                        result.M24,
                        result.M31,
                        result.M32,
                        result.M33,
                        result.M34,
                    ]
                )
                : new Matrix3D(identity: true);
        }
    }

    public readonly Matrix3D OrthogonalInverse
    {
        get
        {
            Matrix3D result = default;
            result.Row0[0] = this[0][0];
            result.Row0[1] = this[1][0];
            result.Row0[2] = this[2][0];

            result.Row1[0] = this[0][1];
            result.Row1[1] = this[1][1];
            result.Row1[2] = this[2][1];

            result.Row2[0] = this[0][2];
            result.Row2[1] = this[1][2];
            result.Row2[2] = this[2][2];

            Vector3 translation = Translation;
            translation = result.RotateVector(translation);
            translation = -translation;

            result.Row0[3] = translation[0];
            result.Row1[3] = translation[1];
            result.Row2[3] = translation[2];

            return result;
        }
    }

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

            if (float.Abs(x.Length2 - 1F) > float.Epsilon)
            {
                return false;
            }

            if (float.Abs(y.Length2 - 1F) > float.Epsilon)
            {
                return false;
            }

            if (float.Abs(z.Length2 - 1F) > float.Epsilon)
            {
                return false;
            }

            return true;
        }
    }

    public static Vector3 TransformVector(Matrix3D matrix, Vector3 vector) =>
        new(
            (matrix[0][0] * vector.X) + (matrix[0][1] * vector.Y) + (matrix[0][2] * vector.Z) + matrix[0][3],
            (matrix[1][0] * vector.X) + (matrix[1][1] * vector.Y) + (matrix[1][2] * vector.Z) + matrix[1][3],
            (matrix[2][0] * vector.X) + (matrix[2][1] * vector.Y) + (matrix[2][2] * vector.Z) + matrix[2][3]
        );

    public static Vector3 RotateVector(Matrix3D matrix, Vector3 vector) =>
        new(
            (matrix[0][0] * vector.X) + (matrix[0][1] * vector.Y) + (matrix[0][2] * vector.Z),
            (matrix[1][0] * vector.X) + (matrix[1][1] * vector.Y) + (matrix[1][2] * vector.Z),
            (matrix[2][0] * vector.X) + (matrix[2][1] * vector.Y) + (matrix[2][2] * vector.Z)
        );

    public static Vector3 InverseTransformVector(Matrix3D matrix, Vector3 vector)
    {
        var difference = new Vector3(vector.X - matrix[0][3], vector.Y - matrix[1][3], vector.Z - matrix[2][3]);
        return InverseRotateVector(matrix, difference);
    }

    public static Vector3 InverseRotateVector(Matrix3D matrix, Vector3 vector) =>
        new(
            (matrix[0][0] * vector.X) + (matrix[1][0] * vector.Y) + (matrix[2][0] * vector.Z),
            (matrix[0][1] * vector.X) + (matrix[1][1] * vector.Y) + (matrix[2][1] * vector.Z),
            (matrix[0][2] * vector.X) + (matrix[1][2] * vector.Y) + (matrix[2][2] * vector.Z)
        );

    public static bool SolveLinearSystem(ref Matrix3D system)
    {
        if (float.Abs(system[0][0]) < float.Epsilon)
        {
            return false;
        }

        system[0] *= 1F / system[0][0];
        system[1] -= system[1][0] * system[0];
        system[2] -= system[2][0] * system[0];

        if (float.Abs(system[1][1]) < float.Epsilon)
        {
            return false;
        }

        system[1] *= 1F / system[1][1];
        system[2] -= system[2][1] * system[1];

        if (float.Abs(system[2][2]) < float.Epsilon)
        {
            return false;
        }

        system[2] *= 1F / system[2][2];

        system[1] -= system[1][2] * system[2];
        system[0] -= system[0][2] * system[2];

        system[0] -= system[0][1] * system[1];

        return true;
    }

    public static Matrix3D Lerp(Matrix3D x, Matrix3D y, float factor)
    {
        Debug.Assert(factor is >= 0F and <= 1F, "Factor must be between 0 and 1 inclusive.");

        var position = Vector3.Lerp(x.Translation, y.Translation, factor);
        var rotation = Quaternion.Slerp((Quaternion)x, (Quaternion)y, factor);
        return new Matrix3D(rotation, position);
    }

    public static Matrix3D FromMatrix3X3(Matrix3X3 matrix) =>
        new(
            [
                matrix[0][0],
                matrix[0][1],
                matrix[0][2],
                0,
                matrix[1][0],
                matrix[1][1],
                matrix[1][2],
                0,
                matrix[2][0],
                matrix[2][1],
                matrix[2][2],
                0,
            ]
        );

    public void Set(ReadOnlySpan<float> values)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(values.Length, 12);
        Row0.Set(values[0], values[1], values[2], values[3]);
        Row1.Set(values[4], values[5], values[6], values[7]);
        Row2.Set(values[8], values[9], values[10], values[11]);
    }

    public void Set(Vector3 x, Vector3 y, Vector3 z, Vector3 position)
    {
        Row0.Set(x[0], y[0], z[0], position[0]);
        Row1.Set(x[1], y[1], z[1], position[1]);
        Row2.Set(x[2], y[2], z[2], position[2]);
    }

    public void Set(Vector3 axis, float angle) => Set(axis, float.Sin(angle), float.Cos(angle));

    public void Set(Vector3 axis, float sin, float cos)
    {
        Debug.Assert(float.Abs(axis.Length2 - 1F) < float.Epsilon, "Axis must be a unit vector.");

        Row0.Set(
            (axis[0] * axis[0]) + (cos * (1F - (axis[0] * axis[0]))),
            (axis[0] * axis[1] * (1F - cos)) - (axis[2] * sin),
            (axis[2] * axis[0] * (1F - cos)) + (axis[1] * sin),
            0F
        );

        Row1.Set(
            (axis[0] * axis[1] * (1F - cos)) + (axis[2] * sin),
            (axis[1] * axis[1]) + (cos * (1F - (axis[1] * axis[1]))),
            (axis[1] * axis[2] * (1F - cos)) - (axis[0] * sin),
            0F
        );

        Row2.Set(
            (axis[2] * axis[0] * (1F - cos)) - (axis[1] * sin),
            (axis[1] * axis[2] * (1F - cos)) + (axis[0] * sin),
            (axis[2] * axis[2]) + (cos * (1 - (axis[2] * axis[2]))),
            0F
        );
    }

    public void Set(Matrix3X3 rotation, Vector3 position)
    {
        SetRotation(rotation);
        Translation = position;
    }

    public void Set(Quaternion rotation, Vector3 position)
    {
        SetRotation(rotation);
        Translation = position;
    }

    public void Set(Vector3 position)
    {
        Row0.Set(1F, 0F, 0F, position[0]);
        Row1.Set(0F, 1F, 0F, position[1]);
        Row2.Set(0F, 0F, 1F, position[2]);
    }

    public void SetRotation(Matrix3X3 matrix)
    {
        Row0[0] = matrix[0][0];
        Row0[1] = matrix[0][1];
        Row0[2] = matrix[0][2];

        Row1[0] = matrix[1][0];
        Row1[1] = matrix[1][1];
        Row1[2] = matrix[1][2];

        Row2[0] = matrix[2][0];
        Row2[1] = matrix[2][1];
        Row2[2] = matrix[2][2];
    }

    public void SetRotation(Quaternion quaternion)
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

    public void AdjustTranslation(Vector3 translation)
    {
        Row0[3] += translation[0];
        Row1[3] += translation[1];
        Row2[3] += translation[2];
    }

    public void AdjustXTranslation(float translation) => Row0[3] += translation;

    public void AdjustYTranslation(float translation) => Row1[3] += translation;

    public void AdjustZTranslation(float translation) => Row2[3] += translation;

    public void MakeIdentity()
    {
        Row0.Set(1, 0, 0, 0);
        Row1.Set(0, 1, 0, 0);
        Row2.Set(0, 0, 1, 0);
    }

    public void Translate(float x, float y, float z)
    {
        Row0[3] += (Row0[0] * x) + (Row0[1] * y) + (Row0[2] * z);
        Row1[3] += (Row1[0] * x) + (Row1[1] * y) + (Row1[2] * z);
        Row2[3] += (Row2[0] * x) + (Row2[1] * y) + (Row2[2] * z);
    }

    public void Translate(Vector3 translation) => Translate(translation[0], translation[1], translation[2]);

    public void TranslateX(float x)
    {
        Row0[3] += Row0[0] * x;
        Row1[3] += Row1[0] * x;
        Row2[3] += Row2[0] * x;
    }

    public void TranslateY(float y)
    {
        Row0[3] += Row0[1] * y;
        Row1[3] += Row1[1] * y;
        Row2[3] += Row2[1] * y;
    }

    public void TranslateZ(float z)
    {
        Row0[3] += Row0[2] * z;
        Row1[3] += Row1[2] * z;
        Row2[3] += Row2[2] * z;
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
        var temp1 = Row0[0];
        var temp2 = Row0[1];
        Row0[0] = (cos * temp1) + (sin * temp2);
        Row0[1] = (-sin * temp1) + (cos * temp2);

        temp1 = Row1[0];
        temp2 = Row1[1];
        Row1[0] = (cos * temp1) + (sin * temp2);
        Row1[1] = (-sin * temp1) + (cos * temp2);

        temp1 = Row2[0];
        temp2 = Row2[1];
        Row2[0] = (cos * temp1) + (sin * temp2);
        Row2[1] = (-sin * temp1) + (cos * temp2);
    }

    public void Scale(float scale) => Scale(scale, scale, scale);

    public void Scale(float x, float y, float z)
    {
        Row0[0] *= x;
        Row1[0] *= x;
        Row2[0] *= x;

        Row0[1] *= y;
        Row1[1] *= y;
        Row2[1] *= y;

        Row0[2] *= z;
        Row1[2] *= z;
        Row2[2] *= z;
    }

    public void Scale(Vector3 scale) => Scale(scale[0], scale[1], scale[2]);

    public void PreRotateX(float theta) => PreRotateX(float.Sin(theta), float.Cos(theta));

    public void PreRotateX(float sin, float cos)
    {
        var temp1 = Row1[0];
        var temp2 = Row2[0];
        Row1[0] = (cos * temp1) - (sin * temp2);
        Row2[0] = (sin * temp1) + (cos * temp2);

        temp1 = Row1[1];
        temp2 = Row2[1];
        Row1[1] = (cos * temp1) - (sin * temp2);
        Row2[1] = (sin * temp1) + (cos * temp2);

        temp1 = Row1[2];
        temp2 = Row2[2];
        Row1[2] = (cos * temp1) - (sin * temp2);
        Row2[2] = (sin * temp1) + (cos * temp2);

        temp1 = Row1[3];
        temp2 = Row2[3];
        Row1[3] = (cos * temp1) - (sin * temp2);
        Row2[3] = (sin * temp1) + (cos * temp2);
    }

    public void PreRotateY(float theta) => PreRotateY(float.Sin(theta), float.Cos(theta));

    public void PreRotateY(float sin, float cos)
    {
        var temp1 = Row0[0];
        var temp2 = Row2[0];
        Row0[0] = (cos * temp1) + (sin * temp2);
        Row2[0] = (-sin * temp1) + (cos * temp2);

        temp1 = Row0[1];
        temp2 = Row2[1];
        Row0[1] = (cos * temp1) + (sin * temp2);
        Row2[1] = (-sin * temp1) + (cos * temp2);

        temp1 = Row0[2];
        temp2 = Row2[2];
        Row0[2] = (cos * temp1) + (sin * temp2);
        Row2[2] = (-sin * temp1) + (cos * temp2);

        temp1 = Row0[3];
        temp2 = Row2[3];
        Row0[3] = (cos * temp1) + (sin * temp2);
        Row2[3] = (-sin * temp1) + (cos * temp2);
    }

    public void PreRotateZ(float theta) => PreRotateZ(float.Sin(theta), float.Cos(theta));

    public void PreRotateZ(float sin, float cos)
    {
        var temp1 = Row0[0];
        var temp2 = Row1[0];
        Row0[0] = (cos * temp1) - (sin * temp2);
        Row1[0] = (sin * temp1) + (cos * temp2);

        temp1 = Row0[1];
        temp2 = Row1[1];
        Row0[1] = (cos * temp1) - (sin * temp2);
        Row1[1] = (sin * temp1) + (cos * temp2);

        temp1 = Row0[2];
        temp2 = Row1[2];
        Row0[2] = (cos * temp1) - (sin * temp2);
        Row1[2] = (sin * temp1) + (cos * temp2);

        temp1 = Row0[3];
        temp2 = Row1[3];
        Row0[3] = (cos * temp1) - (sin * temp2);
        Row1[3] = (sin * temp1) + (cos * temp2);
    }

    public void InPlacePreRotateX(float theta) => InPlacePreRotateX(float.Sin(theta), float.Cos(theta));

    public void InPlacePreRotateX(float sin, float cos)
    {
        var temp1 = Row1[0];
        var temp2 = Row2[0];
        Row1[0] = (cos * temp1) - (sin * temp2);
        Row2[0] = (sin * temp1) + (cos * temp2);

        temp1 = Row1[1];
        temp2 = Row2[1];
        Row1[1] = (cos * temp1) - (sin * temp2);
        Row2[1] = (sin * temp1) + (cos * temp2);

        temp1 = Row1[2];
        temp2 = Row2[2];
        Row1[2] = (cos * temp1) - (sin * temp2);
        Row2[2] = (sin * temp1) + (cos * temp2);
    }

    public void InPlacePreRotateY(float theta) => InPlacePreRotateY(float.Sin(theta), float.Cos(theta));

    public void InPlacePreRotateY(float sin, float cos)
    {
        var temp1 = Row0[0];
        var temp2 = Row2[0];
        Row0[0] = (cos * temp1) + (sin * temp2);
        Row2[0] = (-sin * temp1) + (cos * temp2);

        temp1 = Row0[1];
        temp2 = Row2[1];
        Row0[1] = (cos * temp1) + (sin * temp2);
        Row2[1] = (-sin * temp1) + (cos * temp2);

        temp1 = Row0[2];
        temp2 = Row2[2];
        Row0[2] = (cos * temp1) + (sin * temp2);
        Row2[2] = (-sin * temp1) + (cos * temp2);
    }

    public void InPlacePreRotateZ(float theta) => InPlacePreRotateZ(float.Sin(theta), float.Cos(theta));

    public void InPlacePreRotateZ(float sin, float cos)
    {
        var temp1 = Row0[0];
        var temp2 = Row1[0];
        Row0[0] = (cos * temp1) - (sin * temp2);
        Row1[0] = (sin * temp1) + (cos * temp2);

        temp1 = Row0[1];
        temp2 = Row1[1];
        Row0[1] = (cos * temp1) - (sin * temp2);
        Row1[1] = (sin * temp1) + (cos * temp2);

        temp1 = Row0[2];
        temp2 = Row1[2];
        Row0[2] = (cos * temp1) - (sin * temp2);
        Row1[2] = (sin * temp1) + (cos * temp2);
    }

    public void LookAt(Vector3 p, Vector3 t, float roll)
    {
        var dx = t[0] - p[0];
        var dy = t[1] - p[1];
        var dz = t[2] - p[2];

        var length1 = float.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        var length2 = float.Sqrt((dx * dx) + (dy * dy));

        float sinP;
        float cosP;
        if (float.Abs(length1) > float.Epsilon)
        {
            sinP = dz / length1;
            cosP = length2 / length1;
        }
        else
        {
            sinP = 0F;
            cosP = 1F;
        }

        float sinY;
        float cosY;
        if (float.Abs(length2) > float.Epsilon)
        {
            sinY = dy / length2;
            cosY = dx / length2;
        }
        else
        {
            sinY = 0F;
            cosY = 1F;
        }

        Row0.Set(0, 0, -1, p.X);
        Row1.Set(-1, 0, 0, p.Y);
        Row2.Set(0, 1, 0, p.Z);

        RotateY(sinY, cosY);
        RotateX(sinP, cosP);
        RotateZ(-roll);
    }

    public void ObjectLookAt(Vector3 p, Vector3 t, float roll)
    {
        var dx = t[0] - p[0];
        var dy = t[1] - p[1];
        var dz = t[2] - p[2];

        var length1 = float.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        var length2 = float.Sqrt((dx * dx) + (dy * dy));

        float sinP;
        float cosP;
        if (float.Abs(length1) > float.Epsilon)
        {
            sinP = dz / length1;
            cosP = length2 / length1;
        }
        else
        {
            sinP = 0F;
            cosP = 1F;
        }

        float sinY;
        float cosY;
        if (float.Abs(length2) > float.Epsilon)
        {
            sinY = dy / length2;
            cosY = dx / length2;
        }
        else
        {
            sinY = 0F;
            cosY = 1F;
        }

        MakeIdentity();
        Translate(p);

        RotateZ(sinY, cosY);
        RotateY(-sinP, cosP);
        RotateX(roll);
    }

    public void BuildTransformMatrix(Vector3 position, Vector3 direction)
    {
        Debug.Assert(float.Abs(direction.Length2 - 1F) < float.Epsilon, "Direction must be a unit vector.");

        var length2 = float.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));

        var sinP = direction.Z;
        var cosP = length2;

        float sinY;
        float cosY;
        if (float.Abs(length2) > float.Epsilon)
        {
            sinY = direction.Y / length2;
            cosY = direction.X / length2;
        }
        else
        {
            sinY = 0F;
            cosY = 1F;
        }

        MakeIdentity();
        Translate(position);

        RotateZ(sinY, cosY);
        RotateY(-sinP, cosP);
    }

    public readonly Vector3 RotateVector(Vector3 vector) =>
        new(
            (this[0][0] * vector[0]) + (this[0][1] * vector[1]) + (this[0][2] * vector[2]),
            (this[1][0] * vector[0]) + (this[1][1] * vector[1]) + (this[1][2] * vector[2]),
            (this[2][0] * vector[0]) + (this[2][1] * vector[1]) + (this[2][2] * vector[2])
        );

    public readonly Vector3 InverseRotateVector(Vector3 vector) =>
        new(
            (this[0][0] * vector[0]) + (this[1][0] * vector[1]) + (this[2][0] * vector[2]),
            (this[0][1] * vector[0]) + (this[1][1] * vector[1]) + (this[2][1] * vector[2]),
            (this[0][2] * vector[0]) + (this[1][2] * vector[1]) + (this[2][2] * vector[2])
        );

    public (Vector3 Min, Vector3 Max) TransformMinMaxAxisAlignedBox(Vector3 min, Vector3 max)
    {
        Vector3 minResult = default;
        Vector3 maxResult = default;

        minResult.X = maxResult.X = this[0][3];
        minResult.Y = maxResult.Y = this[1][3];
        minResult.Z = maxResult.Z = this[2][3];

        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                var temp0 = this[i][j] * min[j];
                var temp1 = this[i][j] * max[j];
                if (temp0 < temp1)
                {
                    minResult[i] += temp0;
                    maxResult[i] += temp1;
                }
                else
                {
                    minResult[i] += temp1;
                    maxResult[i] += temp0;
                }
            }
        }

        return (minResult, maxResult);
    }

    public (Vector3 Center, Vector3 Extent) TransformCenterExtentAxisAlignedBox(Vector3 center, Vector3 extent)
    {
        Vector3 centerResult = default;
        Vector3 extentResult = default;

        for (var i = 0; i < 3; i++)
        {
            centerResult[i] = this[i][3];
            extentResult[i] = 0F;
            for (var j = 0; j < 3; j++)
            {
                centerResult[i] += this[i][j] * center[j];
                extentResult[i] += float.Abs(this[i][j] * extent[j]);
            }
        }

        return (centerResult, extentResult);
    }

    public void ReOrthogonalize()
    {
        Vector3 x = XVector;
        Vector3 y = YVector;
        var z = Vector3.CrossProduct(Vector3.CrossProduct(x, y), y);

        var length = x.Length;
        if (length < float.Epsilon)
        {
            MakeIdentity();
            return;
        }

        x *= 1F / length;

        length = y.Length;
        if (length < float.Epsilon)
        {
            MakeIdentity();
            return;
        }

        y *= 1F / length;

        length = z.Length;
        if (length < float.Epsilon)
        {
            MakeIdentity();
            return;
        }

        z *= 1F / length;

        Row0[0] = x.X;
        Row0[1] = x.Y;
        Row0[2] = x.Z;

        Row1[0] = y.X;
        Row1[1] = y.Y;
        Row1[2] = y.Z;

        Row2[0] = z.X;
        Row2[1] = z.Y;
        Row2[2] = z.Z;
    }

    public readonly Matrix3D Multiply(Matrix3D other)
    {
        Matrix3D result = default;

        result.Row0[0] = (Row0[0] * other.Row0[0]) + (Row0[1] * other.Row1[0]) + (Row0[2] * other.Row2[0]);
        result.Row1[0] = (Row1[0] * other.Row0[0]) + (Row1[1] * other.Row1[0]) + (Row1[2] * other.Row2[0]);
        result.Row2[0] = (Row2[0] * other.Row0[0]) + (Row2[1] * other.Row1[0]) + (Row2[2] * other.Row2[0]);

        result.Row0[1] = (Row0[0] * other.Row0[1]) + (Row0[1] * other.Row1[1]) + (Row0[2] * other.Row2[1]);
        result.Row1[1] = (Row1[0] * other.Row0[1]) + (Row1[1] * other.Row1[1]) + (Row1[2] * other.Row2[1]);
        result.Row2[1] = (Row2[0] * other.Row0[1]) + (Row2[1] * other.Row1[1]) + (Row2[2] * other.Row2[1]);

        result.Row0[2] = (Row0[0] * other.Row0[2]) + (Row0[1] * other.Row1[2]) + (Row0[2] * other.Row2[2]);
        result.Row1[2] = (Row1[0] * other.Row0[2]) + (Row1[1] * other.Row1[2]) + (Row1[2] * other.Row2[2]);
        result.Row2[2] = (Row2[0] * other.Row0[2]) + (Row2[1] * other.Row1[2]) + (Row2[2] * other.Row2[2]);

        result.Row0[3] = (Row0[0] * other.Row0[3]) + (Row0[1] * other.Row1[3]) + (Row0[2] * other.Row2[3]) + Row0[3];
        result.Row1[3] = (Row1[0] * other.Row0[3]) + (Row1[1] * other.Row1[3]) + (Row1[2] * other.Row2[3]) + Row1[3];
        result.Row2[3] = (Row2[0] * other.Row0[3]) + (Row2[1] * other.Row1[3]) + (Row2[2] * other.Row2[3]) + Row2[3];

        return result;
    }

    public IList<Vector3> Multiply(IReadOnlyList<Vector3> vectors)
    {
        ArgumentNullException.ThrowIfNull(vectors);
        var result = new Vector3[vectors.Count];

        for (var i = 0; i < vectors.Count; i++)
        {
            result[i] = new Vector3(
                (Row0.X * vectors[i].X) + (Row0.Y * vectors[i].Y) + (Row0.Z * vectors[i].Z) + Row0.W,
                (Row1.X * vectors[i].X) + (Row1.Y * vectors[i].Y) + (Row1.Z * vectors[i].Z) + Row1.W,
                (Row2.X * vectors[i].X) + (Row2.Y * vectors[i].Y) + (Row2.Z * vectors[i].Z) + Row2.W
            );
        }

        return result;
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Matrix3D other && Equals(other);

    public readonly bool Equals(Matrix3D other) => Row0 == other.Row0 && Row1 == other.Row1 && Row2 == other.Row2;

    public override readonly int GetHashCode() => HashCode.Combine(Row0, Row1, Row2);

    public override readonly string ToString() => $"Matrix3D({Row0}, {Row1}, {Row2})";

    public static Matrix3D operator *(Matrix3D x, Matrix3D y) => x.Multiply(y);

    public static IList<Vector3> operator *(Matrix3D matrix, IReadOnlyList<Vector3> vectors) =>
        matrix.Multiply(vectors);

    public static bool operator ==(Matrix3D x, Matrix3D y) => x.Equals(y);

    public static bool operator !=(Matrix3D x, Matrix3D y) => !x.Equals(y);

    public static explicit operator Matrix3D(Matrix3X3 matrix) => FromMatrix3X3(matrix);

    public Vector4 this[int index]
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
