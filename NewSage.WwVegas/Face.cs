// -----------------------------------------------------------------------
// <copyright file="Face.cs" company="NewSage">
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
using NewSage.WwVegas.WwMath;

namespace NewSage.WwVegas;

public class Face
{
    private readonly int[][] _textureIndex = new int[MeshBuilder.MaxPasses][];

    public Face()
    {
        for (var i = 0; i < MeshBuilder.MaxPasses; i++)
        {
            _textureIndex[i] = new int[MeshBuilder.MaxStages];
        }

        Reset();
    }

    public IList<Vertex> Vertices { get; } = new Vertex[3];

    public int SmoothingGroup { get; set; }

    public int Index { get; set; }

    public int Attributes { get; set; }

    public IReadOnlyList<IList<int>> TextureIndex => _textureIndex;

    public IList<int> ShaderIndex { get; } = new int[MeshBuilder.MaxPasses];

    public uint SurfaceType { get; set; }

    public int AddIndex { get; set; }

    public IList<int> VertexIndices { get; } = new int[3];

    public Vector3 Normal { get; set; } = new(0, 0, 0);

    public float Distance { get; set; }

    public bool IsDegenerate
    {
        get
        {
            for (var v0 = 0; v0 < 3; v0++)
            {
                for (var v1 = v0 + 1; v1 < 3; v1++)
                {
                    if (VertexIndices[v0] == VertexIndices[v1])
                    {
                        return true;
                    }

                    if (
                        float.Abs(Vertices[v0].Position.X - Vertices[v1].Position.X) < float.Epsilon
                        && float.Abs(Vertices[v0].Position.Y - Vertices[v1].Position.Y) < float.Epsilon
                        && float.Abs(Vertices[v0].Position.Z - Vertices[v1].Position.Z) < float.Epsilon
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public void Reset()
    {
        for (var i = 0; i < 3; i++)
        {
            Vertices[i].Reset();
            VertexIndices[i] = 0;
        }

        SmoothingGroup = 0;
        Index = 0;
        Attributes = 0;
        SurfaceType = 0;

        for (var pass = 0; pass < MeshBuilder.MaxPasses; pass++)
        {
            for (var stage = 0; stage < MeshBuilder.MaxStages; stage++)
            {
                _textureIndex[pass][stage] = -1;
            }

            ShaderIndex[pass] = -1;
        }

        AddIndex = 0;
        Normal = new Vector3(0, 0, 0);
        Distance = 0;
    }

    public void ComputePlane()
    {
        Normal = Vector3
            .CrossProduct(Vertices[1].Position - Vertices[0].Position, Vertices[2].Position - Vertices[0].Position)
            .Normalized;

        Distance = Vector3.DotProduct(Normal, Vertices[0].Position);
    }
}
