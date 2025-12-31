// -----------------------------------------------------------------------
// <copyright file="Sphere.cs" company="NewSage">
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
public struct Sphere
{
    public Vector3 Center;
    public float Radius;

    public Sphere() { }

    public Sphere(Vector3 center, float radius) => Initialize(center, radius);

    public Sphere(Matrix3D matrix, Vector3 center, float radius) => Initialize(matrix, center, radius);

    public Sphere(Vector3 center, Sphere s0)
    {
        var distance = (s0.Center - center).Length;
        Center = center;
        Radius = s0.Radius + distance;
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "Splitting this algorithm may make it more difficult to read."
    )]
    public Sphere(ReadOnlySpan<Vector3> positions)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(positions.Length, 1);

        var xMin = new Vector3(positions[0].X, positions[0].Y, positions[0].Z);
        var xMax = new Vector3(positions[0].X, positions[0].Y, positions[0].Z);
        var yMin = new Vector3(positions[0].X, positions[0].Y, positions[0].Z);
        var yMax = new Vector3(positions[0].X, positions[0].Y, positions[0].Z);
        var zMin = new Vector3(positions[0].X, positions[0].Y, positions[0].Z);
        var zMax = new Vector3(positions[0].X, positions[0].Y, positions[0].Z);

        for (var i = 1; i < positions.Length; i++)
        {
            if (positions[i].X < xMin.X)
            {
                xMin.Set(positions[i].X, positions[i].Y, positions[i].Z);
            }

            if (positions[i].X > xMax.X)
            {
                xMax.Set(positions[i].X, positions[i].Y, positions[i].Z);
            }

            if (positions[i].Y < yMin.Y)
            {
                yMin.Set(positions[i].X, positions[i].Y, positions[i].Z);
            }

            if (positions[i].Y > yMax.Y)
            {
                yMax.Set(positions[i].X, positions[i].Y, positions[i].Z);
            }

            if (positions[i].Z < zMin.Z)
            {
                zMin.Set(positions[i].X, positions[i].Y, positions[i].Z);
            }

            if (positions[i].Z > zMax.Z)
            {
                zMax.Set(positions[i].X, positions[i].Y, positions[i].Z);
            }
        }

        var dx = xMax.X - xMin.X;
        var dy = yMax.Y - yMin.Y;
        var dz = zMax.Z - zMin.Z;
        double xSpan = (dx * dx) + (dy * dy) + (dz * dz);

        dx = yMax.X - xMin.X;
        dy = zMax.Y - yMin.Y;
        dz = xMax.Z - zMin.Z;
        double ySpan = (dx * dx) + (dy * dy) + (dz * dz);

        dx = zMax.X - xMin.X;
        dy = yMax.Y - yMin.Y;
        dz = xMax.Z - zMin.Z;
        double zSpan = (dx * dx) + (dy * dy) + (dz * dz);

        Vector3 dia1 = xMin;
        Vector3 dia2 = xMax;
        var maxSpan = xSpan;

        if (ySpan > maxSpan)
        {
            maxSpan = ySpan;
            dia1 = yMin;
            dia2 = yMax;
        }

        if (zSpan > maxSpan)
        {
            dia1 = zMin;
            dia2 = zMax;
        }

        var center = new Vector3((dia1.X + dia2.X) / 2F, (dia1.Y + dia2.Y) / 2F, (dia1.Z + dia2.Z) / 2F);

        dx = dia2.X - center.X;
        dy = dia2.Y - center.Y;
        dz = dia2.Z - center.Z;

        double radSqr = (dx * dx) + (dy * dy) + (dz * dz);
        var radius = double.Sqrt(radSqr);

        for (var i = 0; i < positions.Length; i++)
        {
            dx = positions[i].X - center.X;
            dy = positions[i].Y - center.Y;
            dz = positions[i].Z - center.Z;

            double testRad2 = (dx * dx) + (dy * dy) + (dz * dz);
            if (testRad2 > radSqr)
            {
                var testRad = double.Sqrt(testRad2);

                radius = (radius + testRad) / 2D;
                radSqr = radius * radius;

                var oldToNew = testRad - radius;
                center.Set(
                    (float)(((radius * center.X) + (oldToNew * positions[i].X)) / testRad),
                    (float)(((radius * center.Y) + (oldToNew * positions[i].Y)) / testRad),
                    (float)(((radius * center.Z) + (oldToNew * positions[i].Z)) / testRad)
                );
            }
        }

        Center = center;
        Radius = (float)radius;
    }

    public readonly float Volume => 4F / 3F * float.Pi * (Radius * Radius * Radius);

    public void Initialize(Vector3 position, float radius) => (Center, Radius) = (position, radius);

    public void Initialize(Matrix3D matrix, Vector3 position, float radius) =>
        (Center, Radius) = (matrix * position, radius);

    public void ReCenter(Vector3 center)
    {
        var distance = (Center - center).Length;
        Center = center;
        Radius += distance;
    }

    public void AddSphere(Sphere sphere)
    {
        if (float.Abs(sphere.Radius) < float.Epsilon)
        {
            return;
        }

        var distance = (sphere.Center - Center).Length;
        if (float.Abs(distance) < float.Epsilon)
        {
            Radius = Radius > sphere.Radius ? Radius : sphere.Radius;
            return;
        }

        var rNew = (distance + Radius + sphere.Radius) / 2F;
        if (rNew < Radius)
        {
            return;
        }

        if (rNew < sphere.Radius)
        {
            Initialize(sphere.Center, sphere.Radius);
        }
        else
        {
            var lerp = (rNew - Radius) / distance;
            Vector3 center = ((sphere.Center - Center) * lerp) + Center;
            Initialize(center, rNew);
        }
    }

    public readonly Sphere Add(Sphere other)
    {
        Sphere self = this;
        self.AddSphere(other);
        return self;
    }

    public readonly Sphere Multiply(Matrix3D matrix)
    {
        Sphere self = this;
        self.Initialize(matrix, self.Center, self.Radius);
        return self;
    }

    public void Transform(Matrix3D transform) => Center = transform * Center;

    public static Sphere operator +(Sphere x, Sphere y) => x.Add(y);

    public static Sphere operator *(Sphere x, Matrix3D y) => x.Multiply(y);
}
