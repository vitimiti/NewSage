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
    public Vector3 BoundMin;
    public Vector3 BoundMax;

    private Plane _plane0;
    private Plane _plane1;
    private Plane _plane2;
    private Plane _plane3;
    private Plane _plane4;
    private Plane _plane5;
    private Vector3 _corner0;
    private Vector3 _corner1;
    private Vector3 _corner2;
    private Vector3 _corner3;
    private Vector3 _corner4;
    private Vector3 _corner5;
    private Vector3 _corner6;
    private Vector3 _corner7;

    public readonly IList<Plane> Planes => [_plane0, _plane1, _plane2, _plane3, _plane4, _plane5];

    public readonly IList<Vector3> Corners =>
        [_corner0, _corner1, _corner2, _corner3, _corner4, _corner5, _corner6, _corner7];

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
            _corner1.Set(viewportMin.X, viewportMax.Y, 1);
            _corner5 = _corner1;
            _corner1 *= zNear;
            _corner5 *= zFar;

            _corner0.Set(viewportMax.X, viewportMin.Y, 1);
            _corner4 = _corner0;
            _corner0 *= zNear;
            _corner4 *= zFar;

            _corner3.Set(viewportMin.X, viewportMin.Y, 1);
            _corner7 = _corner3;
            _corner3 *= zNear;
            _corner7 *= zFar;

            _corner2.Set(viewportMax.X, viewportMax.Y, 1);
            _corner6 = _corner2;
            _corner2 *= zNear;
            _corner6 *= zFar;
        }
        else
        {
            _corner0.Set(viewportMin.X, viewportMax.Y, 1);
            _corner4 = _corner0;
            _corner0 *= zNear;
            _corner4 *= zFar;

            _corner1.Set(viewportMax.X, viewportMin.Y, 1);
            _corner5 = _corner1;
            _corner1 *= zNear;
            _corner5 *= zFar;

            _corner2.Set(viewportMin.X, viewportMin.Y, 1);
            _corner6 = _corner2;
            _corner2 *= zNear;
            _corner6 *= zFar;

            _corner3.Set(viewportMax.X, viewportMax.Y, 1);
            _corner7 = _corner3;
            _corner3 *= zNear;
            _corner7 *= zFar;
        }

        for (var i = 0; i < 8; i++)
        {
            Corners[i] = Matrix3D.TransformVector(CameraTransform, Corners[i]);
        }

        _plane0.Set(_corner0, _corner3, _corner1);
        _plane1.Set(_corner0, _corner5, _corner4);
        _plane2.Set(_corner0, _corner6, _corner2);
        _plane3.Set(_corner2, _corner7, _corner3);
        _plane4.Set(_corner1, _corner7, _corner5);
        _plane5.Set(_corner4, _corner7, _corner6);

        BoundMin = BoundMax = _corner0;

        for (var i = 1; i < 8; i++)
        {
            BoundMin.X = float.Min(BoundMin.X, Corners[i].X);
            BoundMax.X = float.Max(BoundMax.X, Corners[i].X);

            BoundMin.Y = float.Min(BoundMin.Y, Corners[i].Y);
            BoundMax.Y = float.Max(BoundMax.Y, Corners[i].Y);

            BoundMin.Z = float.Min(BoundMin.Z, Corners[i].Z);
            BoundMax.Z = float.Max(BoundMax.Z, Corners[i].Z);
        }
    }
}
