// -----------------------------------------------------------------------
// <copyright file="Frustum.cs" company="NewSage">
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
[SuppressMessage(
    "Performance",
    "CA1815:Override equals and operator equals on value types",
    Justification = "Not used in this type."
)]
public struct Frustum
{
    public Matrix3D CameraTransform;
    public Plane Plane0;
    public Plane Plane1;
    public Plane Plane2;
    public Plane Plane3;
    public Plane Plane4;
    public Plane Plane5;
    public Vector3 Corner0;
    public Vector3 Corner1;
    public Vector3 Corner2;
    public Vector3 Corner3;
    public Vector3 Corner4;
    public Vector3 Corner5;
    public Vector3 Corner6;
    public Vector3 Corner7;
    public Vector3 BoundMin;
    public Vector3 BoundMax;

    public void Initialize(Matrix3D camera, Vector2 viewportMin, Vector2 viewportMax, (float Near, float Far) z)
    {
        CameraTransform = camera;

        var zNear = z.Near;
        var zFar = z.Far;
        if (zNear > 0F && zFar > 0F)
        {
            zNear = -zNear;
            zFar = -zFar;
        }

        var zv = Vector3.CrossProduct(CameraTransform.XVector, CameraTransform.YVector);

        if (Vector3.DotProduct(CameraTransform.ZVector, zv) < 0F)
        {
            Corner1.Set(viewportMin.X, viewportMax.Y, 1);
            Corner5 = Corner1;
            Corner1 *= zNear;
            Corner5 *= zFar;

            Corner0.Set(viewportMax.X, viewportMin.Y, 1);
            Corner4 = Corner0;
            Corner0 *= zNear;
            Corner4 *= zFar;

            Corner3.Set(viewportMin.X, viewportMin.Y, 1);
            Corner7 = Corner3;
            Corner3 *= zNear;
            Corner7 *= zFar;

            Corner2.Set(viewportMax.X, viewportMax.Y, 1);
            Corner6 = Corner2;
            Corner2 *= zNear;
            Corner6 *= zFar;
        }
        else
        {
            Corner0.Set(viewportMin.X, viewportMax.Y, 1);
            Corner4 = Corner0;
            Corner0 *= zNear;
            Corner4 *= zFar;

            Corner1.Set(viewportMax.X, viewportMin.Y, 1);
            Corner5 = Corner1;
            Corner1 *= zNear;
            Corner5 *= zFar;

            Corner2.Set(viewportMin.X, viewportMin.Y, 1);
            Corner6 = Corner2;
            Corner2 *= zNear;
            Corner6 *= zFar;

            Corner3.Set(viewportMax.X, viewportMax.Y, 1);
            Corner7 = Corner3;
            Corner3 *= zNear;
            Corner7 *= zFar;
        }

        Corner0 = Matrix3D.TransformVector(CameraTransform, Corner0);
        Corner1 = Matrix3D.TransformVector(CameraTransform, Corner1);
        Corner2 = Matrix3D.TransformVector(CameraTransform, Corner2);
        Corner3 = Matrix3D.TransformVector(CameraTransform, Corner3);
        Corner4 = Matrix3D.TransformVector(CameraTransform, Corner4);
        Corner5 = Matrix3D.TransformVector(CameraTransform, Corner5);
        Corner6 = Matrix3D.TransformVector(CameraTransform, Corner6);
        Corner7 = Matrix3D.TransformVector(CameraTransform, Corner7);

        Plane0.Set(Corner0, Corner3, Corner1);
        Plane1.Set(Corner0, Corner5, Corner4);
        Plane2.Set(Corner0, Corner6, Corner2);
        Plane3.Set(Corner2, Corner7, Corner3);
        Plane4.Set(Corner1, Corner7, Corner5);
        Plane5.Set(Corner4, Corner7, Corner6);

        BoundMin = BoundMax = Corner0;

        for (var i = 1; i < 8; i++)
        {
            BoundMin.X = float.Min(BoundMin.X, GetCornerAt(i).X);
            BoundMax.X = float.Max(BoundMax.X, GetCornerAt(i).X);

            BoundMin.Y = float.Min(BoundMin.Y, GetCornerAt(i).Y);
            BoundMax.Y = float.Max(BoundMax.Y, GetCornerAt(i).Y);

            BoundMin.Z = float.Min(BoundMin.Z, GetCornerAt(i).Z);
            BoundMax.Z = float.Max(BoundMax.Z, GetCornerAt(i).Z);
        }
    }

    internal readonly Plane GetPlaneAt(int index) =>
        index switch
        {
            0 => Plane0,
            1 => Plane1,
            2 => Plane2,
            3 => Plane3,
            4 => Plane4,
            5 => Plane5,
            _ => throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                "Index must be between 0 and 5 inclusive."
            ),
        };

    internal readonly Vector3 GetCornerAt(int index) =>
        index switch
        {
            0 => Corner0,
            1 => Corner1,
            2 => Corner2,
            3 => Corner3,
            4 => Corner4,
            5 => Corner5,
            6 => Corner6,
            7 => Corner7,
            _ => throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                "Index must be between 0 and 7 inclusive."
            ),
        };
}
