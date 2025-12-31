// -----------------------------------------------------------------------
// <copyright file="Vertex.cs" company="NewSage">
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

using NewSage.WwVegas.WwMath;

namespace NewSage.WwVegas.Ww3D2;

public class Vertex
{
    private readonly Vector2[][] _textureCoordinates = new Vector2[MeshBuilder.MaxPasses][];

    public Vertex()
    {
        for (var i = 0; i < MeshBuilder.MaxPasses; i++)
        {
            _textureCoordinates[i] = new Vector2[MeshBuilder.MaxStages];
        }

        Reset();
    }

    public Vector3 Position { get; set; } = new(0, 0, 0);

    public Vector3 Normal { get; set; } = new(0, 0, 0);

    public int SmoothingGroup { get; set; }

    public int Id { get; set; }

    public int BoneIndex { get; set; }

    public int MaxVertColIndex { get; set; }

    public IReadOnlyList<IList<Vector2>> TextureCoordinates => _textureCoordinates;

    public IList<Vector3> DiffuseColor { get; } = new Vector3[MeshBuilder.MaxPasses];

    public IList<Vector3> SpecularColor { get; } = new Vector3[MeshBuilder.MaxPasses];

    public IList<Vector3> DiffuseIllumination { get; } = new Vector3[MeshBuilder.MaxPasses];

    public IList<float> Alpha { get; } = new float[MeshBuilder.MaxPasses];

    public IList<int> VertexMaterialIndex { get; } = new int[MeshBuilder.MaxPasses];

    public int Attribute0 { get; set; }

    public int Attribute1 { get; set; }

    public int SharedSmoothingGroup { get; set; }

    public int UniqueIndex { get; set; }

    public int ShadeIndex { get; set; }

    public Vertex? NextHash { get; set; }

    public void Reset()
    {
        Position = new Vector3(0, 0, 0);
        Normal = new Vector3(0, 0, 0);
        SmoothingGroup = 0;
        Id = 0;
        MaxVertColIndex = 0;

        for (var pass = 0; pass < MeshBuilder.MaxPasses; pass++)
        {
            DiffuseColor[pass] = new Vector3(1, 1, 1);
            SpecularColor[pass] = new Vector3(1, 1, 1);
            DiffuseIllumination[pass] = new Vector3(0, 0, 0);
            Alpha[pass] = 1;
            VertexMaterialIndex[pass] = -1;

            for (var stage = 0; stage < MeshBuilder.MaxStages; stage++)
            {
                _textureCoordinates[pass][stage] = new Vector2(0, 0);
            }
        }

        BoneIndex = 0;
        Attribute0 = 0;
        Attribute1 = 0;
        UniqueIndex = 0;
        ShadeIndex = 0;
        NextHash = null;
    }
}
