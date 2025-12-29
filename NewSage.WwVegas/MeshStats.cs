// -----------------------------------------------------------------------
// <copyright file="MeshStats.cs" company="NewSage">
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

namespace NewSage.WwVegas;

public class MeshStats
{
    private readonly bool[][] _hasTexture = new bool[MeshBuilder.MaxPasses][];
    private readonly bool[][] _hasPerPolygonTexture = new bool[MeshBuilder.MaxPasses][];
    private readonly bool[][] _hasTextureCoordinates = new bool[MeshBuilder.MaxPasses][];

    public MeshStats()
    {
        for (var i = 0; i < MeshBuilder.MaxPasses; i++)
        {
            _hasTexture[i] = new bool[MeshBuilder.MaxStages];
            _hasPerPolygonTexture[i] = new bool[MeshBuilder.MaxStages];
            _hasTextureCoordinates[i] = new bool[MeshBuilder.MaxStages];
        }
    }

    public IReadOnlyList<IList<bool>> HasTexture => _hasTexture;

    public IList<bool> HasShader { get; } = new bool[MeshBuilder.MaxPasses];

    public IList<bool> HasVertexMaterial { get; } = new bool[MeshBuilder.MaxPasses];

    public IReadOnlyList<IList<bool>> HasPerPolygonTexture => _hasPerPolygonTexture;

    public IList<bool> HasPerPolygonShader { get; } = new bool[MeshBuilder.MaxPasses];

    public IList<bool> HasPerVertexMaterial { get; } = new bool[MeshBuilder.MaxPasses];

    public IList<bool> HasDiffuseColor { get; } = new bool[MeshBuilder.MaxPasses];

    public IList<bool> HasSpecularColor { get; } = new bool[MeshBuilder.MaxPasses];

    public IList<bool> HasDiffuseIllumination { get; } = new bool[MeshBuilder.MaxPasses];

    public IReadOnlyList<IList<bool>> HasTextureCoordinates => _hasTextureCoordinates;

    public int UvSplitCount { get; set; }

    public int StripCount { get; set; }

    public int MaxStripLength { get; set; }

    public float AverageStripLength { get; set; }

    public void Reset()
    {
        for (var pass = 0; pass < MeshBuilder.MaxPasses; pass++)
        {
            HasPerPolygonShader[pass] = false;
            HasPerVertexMaterial[pass] = false;
            HasDiffuseColor[pass] = false;
            HasSpecularColor[pass] = false;
            HasDiffuseIllumination[pass] = false;
            HasVertexMaterial[pass] = false;
            HasShader[pass] = false;

            for (var stage = 0; stage < MeshBuilder.MaxStages; stage++)
            {
                _hasPerPolygonTexture[pass][stage] = false;
                _hasTextureCoordinates[pass][stage] = false;
                _hasTexture[pass][stage] = false;
            }
        }

        UvSplitCount = 0;
        StripCount = 0;
        MaxStripLength = 0;
        AverageStripLength = 0;
    }
}
