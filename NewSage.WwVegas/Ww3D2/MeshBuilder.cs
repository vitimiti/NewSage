// -----------------------------------------------------------------------
// <copyright file="MeshBuilder.cs" company="NewSage">
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
using NewSage.WwVegas.WwMath;

namespace NewSage.WwVegas.Ww3D2;

public sealed class MeshBuilder : IDisposable
{
    public const int MaxPasses = 4;
    public const int MaxStages = 2;

    private static readonly Func<object, object, int>[][] TextureCompareFunctions =
    [
        [Pass0Stage0Compare, Pass0Stage1Compare],
        [Pass1Stage0Compare, Pass1Stage1Compare],
        [Pass2Stage0Compare, Pass2Stage1Compare],
        [Pass3Stage0Compare, Pass3Stage1Compare],
    ];

    private readonly int _inputVertexCount;
    private readonly MeshStats _stats = new();

    private MeshBuilderState _state;
    private int _faceCount;
    private Face[]? _faces;
    private int _vertexCount;
    private Vertex[]? _vertices;
    private int _currentFace;
    private int _polygonOrderPass;
    private int _polygonOrderStage;
    private int _allocatedFaceCount;
    private int _allocatedFaceGrowth;
    private bool _disposed;

    public MeshBuilder(int passCount = 1, int faceCountGuess = 255, int faceCountGrowthRate = 64)
    {
        _state = MeshBuilderState.AcceptingInput;
        PassCount = passCount;
        _faceCount = 0;
        _faces = null;
        _inputVertexCount = 0;
        _vertexCount = 0;
        _vertices = null;
        _currentFace = 0;
        _allocatedFaceCount = 0;
        _allocatedFaceGrowth = 0;
        _polygonOrderPass = 0;
        _polygonOrderStage = 0;
        WorldInformation = null;

        Reset(passCount, faceCountGuess, faceCountGrowthRate);
    }

    public int PassCount { get; private set; }

    public int VertexCount
    {
        get
        {
            Debug.Assert(_state is MeshBuilderState.Processed, $"Cannot get vertex count when state is {_state}");
            return _vertexCount;
        }
    }

    public int FaceCount
    {
        get
        {
            Debug.Assert(_state is MeshBuilderState.Processed, $"Cannot get face count when state is {_state}");
            return _faceCount;
        }
    }

    public WorldInformation? WorldInformation { get; set; }

    public MeshStats MeshStats
    {
        get
        {
            Debug.Assert(_state is MeshBuilderState.Processed, $"Cannot get mesh stats when state is {_state}");
            return _stats;
        }
    }

    public void Reset(int passCount, int faceCountGuess, int faceCountGrowthRate)
    {
        Free();

        _state = MeshBuilderState.AcceptingInput;
        PassCount = passCount;
        _allocatedFaceCount = faceCountGuess;
        _allocatedFaceGrowth = faceCountGrowthRate;

        _faces = new Face[_allocatedFaceCount];
        _currentFace = 0;
        _stats.Reset();
    }

    public int AddFace(Face face)
    {
        ArgumentNullException.ThrowIfNull(face);
        Debug.Assert(_state is MeshBuilderState.AcceptingInput, $"Cannot add face when state is {_state}");
        Debug.Assert(_currentFace <= _allocatedFaceCount, $"Cannot add face when face count is {_currentFace}");
        if (_currentFace == _allocatedFaceCount)
        {
            GrowFaceArray();
        }

        _faces![_currentFace] = face;
        _faces![_currentFace].ComputePlane();
        _faces![_currentFace].AddIndex = _currentFace;

        for (var i = 0; i < 3; i++)
        {
            _faces![_currentFace].Vertices[i].SmoothingGroup = _faces![_currentFace].SmoothingGroup;
        }

        _currentFace++;
        return _currentFace - 1;
    }

    public void BuildMesh(bool computeNormals)
    {
        Debug.Assert(_state is MeshBuilderState.AcceptingInput, $"Cannot build mesh when state is {_state}");

        _state = MeshBuilderState.Processed;
        _faceCount = _currentFace;
        OptimizeMesh(computeNormals);
    }

    public void SetPolygonOrderingChannel(int pass, int textureStage)
    {
        Debug.Assert(pass >= 0, $"Pass must be non-negative, was {pass}");
        Debug.Assert(pass < MaxPasses, $"Pass must be less than {MaxPasses}, was {pass}");
        Debug.Assert(textureStage >= 0, $"Texture stage must be non-negative, was {textureStage}");
        Debug.Assert(textureStage < MaxStages, $"Texture stage must be less than {MaxStages}, was {textureStage}");

        _polygonOrderPass = pass;
        _polygonOrderStage = textureStage;
    }

    public Vertex GetVertex(int index)
    {
        Debug.Assert(_state is MeshBuilderState.Processed, $"Cannot get vertex when state is {_state}");
        Debug.Assert(index >= 0, $"Vertex index must be non-negative, was {index}");
        Debug.Assert(index < _vertexCount, $"Vertex index must be less than the vertex count, was {index}");

        return _vertices![index];
    }

    public Face GetFace(int index)
    {
        Debug.Assert(_state is MeshBuilderState.Processed, $"Cannot get vertex when state is {_state}");
        Debug.Assert(index >= 0, $"Vertex index must be non-negative, was {index}");
        Debug.Assert(index < _faceCount, $"Vertex index must be less than the face count, was {index}");

        return _faces![index];
    }

    public void SetVertex(int index, Vertex vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);
        Debug.Assert(_state is MeshBuilderState.AcceptingInput, $"Cannot set vertex when state is {_state}");
        Debug.Assert(index >= 0, $"Vertex index must be non-negative, was {index}");
        Debug.Assert(index < _inputVertexCount, $"Vertex index must be less than the input vertex count, was {index}");

        _vertices![index] = vertex;
    }

    public void SetFace(int index, Face face)
    {
        ArgumentNullException.ThrowIfNull(face);
        Debug.Assert(_state is MeshBuilderState.AcceptingInput, $"Cannot set face when state is {_state}");
        Debug.Assert(index >= 0, $"Face index must be non-negative, was {index}");
        Debug.Assert(index < _faceCount, $"Face index must be less than the face count, was {index}");

        _faces![index] = face;
    }

    public (Vector3 Min, Vector3 Max) ComputeBoundingBox()
    {
        var minX = _vertices![0].Position.X;
        var minY = _vertices![0].Position.Y;
        var minZ = _vertices![0].Position.Z;
        var maxX = _vertices![0].Position.X;
        var maxY = _vertices![0].Position.Y;
        var maxZ = _vertices![0].Position.Z;

        for (var i = 0; i < _vertexCount; i++)
        {
            if (_vertices![i].Position.X < minX)
            {
                minX = _vertices![i].Position.X;
            }

            if (_vertices![i].Position.Y < minY)
            {
                minY = _vertices![i].Position.Y;
            }

            if (_vertices![i].Position.Z < minZ)
            {
                minZ = _vertices![i].Position.Z;
            }

            if (_vertices![i].Position.X > maxX)
            {
                maxX = _vertices![i].Position.X;
            }

            if (_vertices![i].Position.Y > maxY)
            {
                maxY = _vertices![i].Position.Y;
            }

            if (_vertices![i].Position.Z > maxZ)
            {
                maxZ = _vertices![i].Position.Z;
            }
        }

        return (new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "This is how the algorithm works and splitting it may be more difficult to follow"
    )]
    public (Vector3 Center, float Radius) ComputeBoundingSphere()
    {
        var xMinX = _vertices![0].Position.X;
        var xMinY = _vertices![0].Position.Y;
        var xMinZ = _vertices![0].Position.Z;
        var xMaxX = _vertices![0].Position.X;
        var xMaxY = _vertices![0].Position.Y;
        var xMaxZ = _vertices![0].Position.Z;
        var yMinX = _vertices![0].Position.X;
        var yMinY = _vertices![0].Position.Y;
        var yMinZ = _vertices![0].Position.Z;
        var yMaxX = _vertices![0].Position.X;
        var yMaxY = _vertices![0].Position.Y;
        var yMaxZ = _vertices![0].Position.Z;
        var zMinX = _vertices![0].Position.X;
        var zMinY = _vertices![0].Position.Y;
        var zMinZ = _vertices![0].Position.Z;
        var zMaxX = _vertices![0].Position.X;
        var zMaxY = _vertices![0].Position.Y;
        var zMaxZ = _vertices![0].Position.Z;

        for (var i = 1; i < _vertexCount; i++)
        {
            if (_vertices![i].Position.X < xMinX)
            {
                xMinX = _vertices![i].Position.X;
                xMinY = _vertices![i].Position.Y;
                xMinZ = _vertices![i].Position.Z;
            }

            if (_vertices![i].Position.X > xMaxX)
            {
                xMaxX = _vertices![i].Position.X;
                xMaxY = _vertices![i].Position.Y;
                xMaxZ = _vertices![i].Position.Z;
            }

            if (_vertices![i].Position.Y < yMinY)
            {
                yMinX = _vertices![i].Position.X;
                yMinY = _vertices![i].Position.Y;
                yMinZ = _vertices![i].Position.Z;
            }

            if (_vertices![i].Position.Y > yMaxY)
            {
                yMaxX = _vertices![i].Position.X;
                yMaxY = _vertices![i].Position.Y;
                yMaxZ = _vertices![i].Position.Z;
            }

            if (_vertices![i].Position.Y < zMinY)
            {
                zMinX = _vertices![i].Position.X;
                zMinY = _vertices![i].Position.Y;
                zMinZ = _vertices![i].Position.Z;
            }

            if (_vertices![i].Position.Y > zMaxY)
            {
                zMaxX = _vertices![i].Position.X;
                zMaxY = _vertices![i].Position.Y;
                zMaxZ = _vertices![i].Position.Z;
            }
        }

        var dx = (double)(xMaxX - xMinX);
        var dy = (double)(xMaxY - xMinY);
        var dz = (double)(xMaxZ - xMinZ);
        var xSpan = (dx * dx) + (dy * dy) + (dz * dz);

        dx = yMaxX - yMinX;
        dy = yMaxY - yMinY;
        dz = yMaxZ - yMinZ;
        var ySpan = (dx * dx) + (dy * dy) + (dz * dz);

        dx = zMaxX - zMinX;
        dy = zMaxY - zMinY;
        dz = zMaxZ - zMinZ;
        var zSpan = (dx * dx) + (dy * dy) + (dz * dz);

        var dia1 = new Vector3(xMinX, yMinY, zMinZ);
        var dia2 = new Vector3(xMaxX, yMaxY, zMaxZ);
        var maxSpan = xSpan;

        if (ySpan > maxSpan)
        {
            maxSpan = ySpan;
            dia1 = new Vector3(yMinX, xMinY, zMinZ);
            dia2 = new Vector3(yMaxX, xMaxY, zMaxZ);
        }

        if (zSpan > maxSpan)
        {
            dia1 = new Vector3(zMinX, yMinZ, xMinZ);
            dia2 = new Vector3(zMaxX, yMaxZ, xMaxZ);
        }

        var centerX = (dia1.X + dia2.X) / 2F;
        var centerY = (dia1.Y + dia2.Y) / 2F;
        var centerZ = (dia1.Z + dia2.Z) / 2F;

        dx = dia2.X - centerX;
        dy = dia2.Y - centerY;
        dz = dia2.Z - centerZ;

        var radSqr = (dx * dx) + (dy * dy) + (dz * dz);
        var radius = double.Sqrt(radSqr);

        for (var i = 0; i < _vertexCount; i++)
        {
            dx = _vertices![i].Position.X - centerX;
            dy = _vertices![i].Position.Y - centerY;
            dz = _vertices![i].Position.Z - centerZ;

            var testRad2 = (dx * dx) + (dy * dy) + (dz * dz);
            if (testRad2 <= radSqr)
            {
                continue;
            }

            var testRad = double.Sqrt(testRad2);

            radius = (radius + testRad) / 2F;
            radSqr = radius * radius;

            var oldToNew = testRad - radius;
            centerX = (float)(((radius * centerX) + (oldToNew * _vertices![i].Position.X)) / testRad);
            centerY = (float)(((radius * centerY) + (oldToNew * _vertices![i].Position.Y)) / testRad);
            centerZ = (float)(((radius * centerZ) + (oldToNew * _vertices![i].Position.Z)) / testRad);
        }

        return (new Vector3(centerX, centerY, centerZ), (float)radius);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Free();
        WorldInformation = null;

        _disposed = true;
    }

    [SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "This will create a multiline ternary operator. Difficult to read."
    )]
    private static int TextureCompare(object elem1, object elem2, int pass, int stage)
    {
        var f0 = (Face)elem1;
        var f1 = (Face)elem2;

        if (f0.TextureIndex[pass][stage] < f1.TextureIndex[pass][stage])
        {
            return -1;
        }

        if (f0.TextureIndex[pass][stage] > f1.TextureIndex[pass][stage])
        {
            return 1;
        }

        if (f0.Vertices[0].VertexMaterialIndex[pass] < f1.Vertices[0].VertexMaterialIndex[pass])
        {
            return -1;
        }

        if (f0.Vertices[0].VertexMaterialIndex[pass] > f1.Vertices[0].VertexMaterialIndex[pass])
        {
            return 1;
        }

        return 0;
    }

    private static int Pass0Stage0Compare(object elem1, object elem2) => TextureCompare(elem1, elem2, 0, 0);

    private static int Pass0Stage1Compare(object elem1, object elem2) => TextureCompare(elem1, elem2, 0, 1);

    private static int Pass1Stage0Compare(object elem1, object elem2) => TextureCompare(elem1, elem2, 1, 0);

    private static int Pass1Stage1Compare(object elem1, object elem2) => TextureCompare(elem1, elem2, 1, 1);

    private static int Pass2Stage0Compare(object elem1, object elem2) => TextureCompare(elem1, elem2, 2, 0);

    private static int Pass2Stage1Compare(object elem1, object elem2) => TextureCompare(elem1, elem2, 2, 1);

    private static int Pass3Stage0Compare(object elem1, object elem2) => TextureCompare(elem1, elem2, 3, 0);

    private static int Pass3Stage1Compare(object elem1, object elem2) => TextureCompare(elem1, elem2, 3, 1);

    [SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "This will create a multiline ternary operator. Difficult to read."
    )]
    private static int VertexCompare(object elem1, object elem2)
    {
        var v0 = (Vertex)elem1;
        var v1 = (Vertex)elem2;

        if (v0.BoneIndex < v1.BoneIndex)
        {
            return -1;
        }

        if (v0.BoneIndex > v1.BoneIndex)
        {
            return 1;
        }

        if (v0.VertexMaterialIndex[0] < v1.VertexMaterialIndex[0])
        {
            return -1;
        }

        if (v0.VertexMaterialIndex[0] > v1.VertexMaterialIndex[0])
        {
            return 1;
        }

        return 0;
    }

    private void Free()
    {
        if (_faces is not null)
        {
            _faces = null;
        }

        if (_vertices is not null)
        {
            _vertices = null;
        }

        _faceCount = 0;
        _vertexCount = 0;
        _allocatedFaceCount = 0;
        _allocatedFaceGrowth = 0;
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "This is how the algorithm works and splitting it may be more difficult to follow"
    )]
    private void ComputeMeshStats()
    {
        var textureIndices = new int[MaxPasses][];
        for (var i = 0; i < MaxPasses; i++)
        {
            textureIndices[i] = new int[MaxStages];
        }

        var shaderIndices = new int[MaxPasses];
        var vertexMaterialIndices = new int[MaxPasses];

        _stats.Reset();

        for (var pass = 0; pass < MaxPasses; pass++)
        {
            for (var stage = 0; stage < MaxStages; stage++)
            {
                textureIndices[pass][stage] = _faces![0].TextureIndex[pass][stage];
            }

            shaderIndices[pass] = _faces![0].ShaderIndex[pass];
            vertexMaterialIndices[pass] = _vertices![0].VertexMaterialIndex[pass];
        }

        for (var pass = 0; pass < MaxPasses; pass++)
        {
            for (var stage = 0; stage < MaxStages; stage++)
            {
                for (var faceIndex = 0; faceIndex < _faceCount; faceIndex++)
                {
                    if (_faces![faceIndex].TextureIndex[pass][stage] == textureIndices[pass][stage])
                    {
                        continue;
                    }

                    _stats.HasPerPolygonTexture[pass][stage] = true;
                    break;
                }

                for (var vertexIndex = 0; vertexIndex < _vertexCount; vertexIndex++)
                {
                    if (_vertices![vertexIndex].TextureCoordinates[pass][stage] == new Vector2(0, 0))
                    {
                        continue;
                    }

                    _stats.HasTextureCoordinates[pass][stage] = true;
                    break;
                }
            }

            for (var faceIndex = 0; faceIndex < _faceCount; faceIndex++)
            {
                if (_faces![faceIndex].ShaderIndex[pass] != shaderIndices[pass])
                {
                    continue;
                }

                _stats.HasPerPolygonShader[pass] = true;
                break;
            }

            for (var vertexIndex = 0; vertexIndex < _vertexCount; vertexIndex++)
            {
                if (_vertices![vertexIndex].VertexMaterialIndex[pass] != vertexMaterialIndices[pass])
                {
                    continue;
                }

                _stats.HasPerVertexMaterial[pass] = true;
                break;
            }

            for (var vertexIndex = 0; vertexIndex < _vertexCount; vertexIndex++)
            {
                if (
                    _vertices![vertexIndex].DiffuseColor[pass] == new Vector3(1, 1, 1)
                    && float.Abs(_vertices![vertexIndex].Alpha[pass] - 1F) < float.Epsilon
                )
                {
                    continue;
                }

                _stats.HasDiffuseColor[pass] = true;
                break;
            }

            for (var vertexIndex = 0; vertexIndex < _vertexCount; vertexIndex++)
            {
                if (_vertices![vertexIndex].SpecularColor[pass] == new Vector3(1, 1, 1))
                {
                    continue;
                }

                _stats.HasSpecularColor[pass] = true;
                break;
            }

            for (var vertexIndex = 0; vertexIndex < _vertexCount; vertexIndex++)
            {
                if (_vertices![vertexIndex].SpecularColor[pass] == new Vector3(1, 1, 1))
                {
                    continue;
                }

                _stats.HasSpecularColor[pass] = true;
                break;
            }

            for (var vertexIndex = 0; vertexIndex < _vertexCount; vertexIndex++)
            {
                if (_vertices![vertexIndex].DiffuseIllumination[pass] == new Vector3(0, 0, 0))
                {
                    continue;
                }

                _stats.HasDiffuseIllumination[pass] = true;
                break;
            }

            for (var stage = 0; stage < MaxStages; stage++)
            {
                for (var faceIndex = 0; faceIndex < _faceCount; faceIndex++)
                {
                    if (_faces![faceIndex].TextureIndex[pass][stage] == -1)
                    {
                        continue;
                    }

                    _stats.HasTexture[pass][stage] = true;
                    break;
                }
            }

            for (var faceIndex = 0; faceIndex < _faceCount; faceIndex++)
            {
                if (_faces![faceIndex].ShaderIndex[pass] == -1)
                {
                    continue;
                }

                _stats.HasShader[pass] = true;
                break;
            }

            for (var vertexIndex = 0; vertexIndex < _vertexCount; vertexIndex++)
            {
                if (_vertices![vertexIndex].VertexMaterialIndex[pass] == -1)
                {
                    continue;
                }

                _stats.HasVertexMaterial[pass] = true;
                break;
            }
        }
    }

    private void OptimizeMesh(bool computeNormals)
    {
        var matchNormals = computeNormals ? 0 : 1;
        var uniqueVertices = new VertexArray(_faceCount * 3, matchNormals);

        Vector3 minVector = _faces![0].Vertices[0].Position;
        Vector3 maxVector = _faces![0].Vertices[0].Position;

        for (var faceIdx = 0; faceIdx < _faceCount; faceIdx++)
        {
            for (var faceVertexIdx = 0; faceVertexIdx < 3; faceVertexIdx++)
            {
                minVector.UpdateMin(_faces![faceIdx].Vertices[faceVertexIdx].Position);
                maxVector.UpdateMax(_faces![faceIdx].Vertices[faceVertexIdx].Position);
            }
        }

        uniqueVertices.SetBounds(minVector, maxVector);
        for (var faceIdx = 0; faceIdx < _faceCount; faceIdx++)
        {
            for (var faceVertexIdx = 0; faceVertexIdx < 3; faceVertexIdx++)
            {
                _faces![faceIdx].VertexIndices[faceVertexIdx] = uniqueVertices.SubmitVertex(
                    _faces![faceIdx].Vertices[faceVertexIdx]
                );
            }
        }

        uniqueVertices.PropagateSharedSmoothGroups();

        _vertexCount = uniqueVertices.VertexCount;
        _vertices = new Vertex[_vertexCount];
        for (var vertexIdx = 0; vertexIdx < _vertexCount; vertexIdx++)
        {
            _vertices[vertexIdx] = uniqueVertices[vertexIdx];
        }

        RemoveDegenerateFaces();
        ComputeFaceNormals();
        if (computeNormals)
        {
            ComputeVertexNormals();
        }

        ComputeMeshStats();
        _stats.UvSplitCount = uniqueVertices.UvSplits;

        _faces.Sort((face, face1) => TextureCompareFunctions[_polygonOrderPass][_polygonOrderStage](face, face1));

        SortVertices();
        StripOptimizeMesh();
        _ = VerifyFaceNormals();
    }

    [SuppressMessage(
        "csharpsquid",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "This is how the algorithm works and splitting it may be more difficult to follow"
    )]
    private void StripOptimizeMesh()
    {
        var edgeHash = new WingedEdgeClass[512];
        var edgeTable = new WingedEdgeClass[_faceCount * 3];
        var polygonEdges = new WingedEdgePolygonClass[_faceCount];
        var newPolygons = new Face[_faceCount];
        var newMaterial = new int[_faceCount];
        var preMap = new int[_faceCount];
        var vertexTimeStamp = new int[_vertexCount];

        var vertexCount = 0;
        var polygonsInserted = 0;
        var lastMaterial = 0;
        var edgeCount = 0;
        Array.Fill(preMap, -1);
        Array.Fill(vertexTimeStamp, -1);

        for (var i = 0; i < _faceCount; i++)
        {
            var material = _faces![i].TextureIndex[_polygonOrderPass][_polygonOrderStage];
            for (var j = 0; j < 3; j++)
            {
                var v0 = j;
                var v1 = v0 < 2 ? v0 + 1 : 0;

                v0 = _faces![i].VertexIndices[v0];
                v1 = _faces![i].VertexIndices[v1];

                if (v0 > v1)
                {
                    (v0, v1) = (v1, v0);
                }

                WingedEdgeClass? edge;
                var hash = (v0 + (v1 * 119)) & 511;
                for (edge = edgeHash[hash]; edge is not null; edge = edge.Next)
                {
                    if (edge.Vertex[0] == v0 && edge.Vertex[1] == v1 && edge.MaterialIndex == material)
                    {
                        break;
                    }
                }

                if (edge is not null)
                {
                    edge.Polygons[1] = i;
                }
                else
                {
                    edge = edgeTable[edgeCount++];
                    edge.Next = edgeHash[hash];
                    edgeHash[hash] = edge;
                    edge.Vertex[0] = v0;
                    edge.Vertex[1] = v1;
                    edge.Polygons[0] = i;
                    edge.Polygons[1] = -1;
                    edge.MaterialIndex = material;
                }

                polygonEdges[i].Edges[j] = edge;
            }
        }

        while (polygonsInserted < _faceCount)
        {
            const int startPass = 0;

            var startPolygon = -1;
            var bestCount = 1 << 29;

            for (var findPass = startPass; findPass < 2; findPass++)
            {
                for (var i = 0; i < _faceCount; i++)
                {
                    if (preMap[i] != -1)
                    {
                        continue;
                    }

                    if (findPass == 0 && _faces![i].TextureIndex[_polygonOrderPass][_polygonOrderStage] != lastMaterial)
                    {
                        continue;
                    }

                    var count = 0;
                    for (var j = 0; j < 3; j++)
                    {
                        if (polygonEdges[i].Edges[j].Polygons[1] >= 0)
                        {
                            count += vertexCount + 1;
                        }
                    }

                    for (var j = 0; j < 3; j++)
                    {
                        count += vertexCount - vertexTimeStamp[_faces![i].VertexIndices[j]];
                    }

                    if (count >= bestCount)
                    {
                        continue;
                    }

                    bestCount = count;
                    startPolygon = i;
                }

                if (startPolygon != -1)
                {
                    break;
                }
            }

            _stats.StripCount++;

            lastMaterial = _faces![startPolygon].TextureIndex[_polygonOrderPass][_polygonOrderStage];
            newMaterial[polygonsInserted] = _faces![startPolygon].TextureIndex[_polygonOrderPass][_polygonOrderStage];

            var foundSharedEdge = false;
            newPolygons[polygonsInserted] = _faces![startPolygon];
            Face newPolygon = newPolygons[polygonsInserted];

            for (var edgeIndex = 0; edgeIndex < 3 && !foundSharedEdge; edgeIndex++)
            {
                for (var sideIndex = 0; sideIndex < 2 && !foundSharedEdge; sideIndex++)
                {
                    var polygon = polygonEdges[startPolygon].Edges[edgeIndex].Polygons[sideIndex];
                    if (polygon == -1 || polygon == startPolygon || preMap[polygon] != -1)
                    {
                        continue;
                    }

                    var firstVertex = -1;
                    for (var vertexIdx = 0; vertexIdx < 3; vertexIdx++)
                    {
                        if (
                            newPolygon.VertexIndices[vertexIdx] == polygonEdges[startPolygon].Edges[edgeIndex].Vertex[0]
                            || newPolygon.VertexIndices[vertexIdx]
                                == polygonEdges[startPolygon].Edges[edgeIndex].Vertex[1]
                        )
                        {
                            continue;
                        }

                        firstVertex = newPolygon.VertexIndices[vertexIdx];
                        break;
                    }

                    Debug.Assert(firstVertex != -1, "Could not find first vertex in new polygon");

                    while (newPolygon.VertexIndices[0] != firstVertex)
                    {
                        var temp = newPolygon.VertexIndices[0];
                        newPolygon.VertexIndices[0] = newPolygon.VertexIndices[1];
                        newPolygon.VertexIndices[1] = newPolygon.VertexIndices[2];
                        newPolygon.VertexIndices[2] = temp;
                    }

                    foundSharedEdge = true;
                    break;
                }
            }

            if (!foundSharedEdge)
            {
                newPolygons[polygonsInserted] = _faces![startPolygon];
            }

            preMap[startPolygon] = polygonsInserted;
            polygonsInserted++;

            for (var i = 0; i < 3; i++)
            {
                if (vertexTimeStamp[_faces![startPolygon].VertexIndices[i]] == -1)
                {
                    vertexTimeStamp[_faces![startPolygon].VertexIndices[i]] = vertexCount++;
                }
            }

            if (
                polygonEdges[startPolygon].Edges[0].Polygons[1] == -1
                && polygonEdges[startPolygon].Edges[1].Polygons[1] == -1
                && polygonEdges[startPolygon].Edges[2].Polygons[1] == -1
            )
            {
                continue;
            }

            int[] vertexFifo = [newPolygon.VertexIndices[1], newPolygon.VertexIndices[2]];
            var stripIndexCount = 0;
            var nextPolygon = startPolygon;

            while (nextPolygon != -1)
            {
                startPolygon = nextPolygon;
                nextPolygon = -1;

                for (var i = 0; i < 3 && nextPolygon == -1; i++)
                {
                    if (
                        (
                            polygonEdges[startPolygon].Edges[i].Vertex[0] != vertexFifo[0]
                            || polygonEdges[startPolygon].Edges[i].Vertex[1] != vertexFifo[1]
                        )
                        && (
                            polygonEdges[startPolygon].Edges[i].Vertex[1] != vertexFifo[0]
                            || polygonEdges[startPolygon].Edges[i].Vertex[0] != vertexFifo[1]
                        )
                    )
                    {
                        continue;
                    }

                    for (var j = 0; j < 2; j++)
                    {
                        if (
                            polygonEdges[startPolygon].Edges[i].Polygons[j] <= -1
                            || preMap[polygonEdges[startPolygon].Edges[i].Polygons[j]] != -1
                        )
                        {
                            continue;
                        }

                        nextPolygon = polygonEdges[startPolygon].Edges[i].Polygons[j];
                        break;
                    }
                }

                if (nextPolygon == -1)
                {
                    break;
                }

                var newVertex = -1;
                for (var i = 0; i < 3; i++)
                {
                    if (
                        _faces![nextPolygon].VertexIndices[i] == vertexFifo[0]
                        || _faces![nextPolygon].VertexIndices[i] == vertexFifo[1]
                    )
                    {
                        continue;
                    }

                    newVertex = i;
                    break;
                }

                Debug.Assert(newVertex != -1, "Unable to get the new vertex");

                var newVertexIndex = _faces![nextPolygon].VertexIndices[newVertex];

                newMaterial[polygonsInserted] = _faces![nextPolygon].TextureIndex[_polygonOrderPass][
                    _polygonOrderStage
                ];

                newPolygons[polygonsInserted].VertexIndices[0] = vertexFifo[0];
                newPolygons[polygonsInserted].VertexIndices[1] = vertexFifo[1];
                newPolygons[polygonsInserted].VertexIndices[2] = newVertexIndex;

                if ((stripIndexCount & 1) == 0)
                {
                    (newPolygons[polygonsInserted].VertexIndices[0], newPolygons[polygonsInserted].VertexIndices[1]) = (
                        newPolygons[polygonsInserted].VertexIndices[1],
                        newPolygons[polygonsInserted].VertexIndices[0]
                    );
                }

                vertexFifo[0] = vertexFifo[1];
                vertexFifo[1] = newVertexIndex;

                if (vertexTimeStamp[newVertexIndex] == -1)
                {
                    vertexTimeStamp[newVertexIndex] = vertexCount++;
                }

                preMap[nextPolygon] = polygonsInserted++;
                stripIndexCount++;
            }

            _stats.AverageStripLength += stripIndexCount + 1;
            if (stripIndexCount + 1 > _stats.MaxStripLength)
            {
                _stats.MaxStripLength = stripIndexCount + 1;
            }
        }

        for (var i = 0; i < _faceCount; i++)
        {
            var newIdx = preMap[i];
            for (var pass = 0; pass < MaxPasses; pass++)
            {
                for (var stage = 0; stage < MaxStages; stage++)
                {
                    newPolygons[newIdx].TextureIndex[pass][stage] = _faces![i].TextureIndex[pass][stage];
                }

                newPolygons[newIdx].ShaderIndex[pass] = _faces![i].ShaderIndex[pass];
            }

            newPolygons[newIdx].SmoothingGroup = _faces![i].SmoothingGroup;
            newPolygons[newIdx].Index = _faces![i].Index;
            newPolygons[newIdx].Attributes = _faces![i].Attributes;
            newPolygons[newIdx].AddIndex = _faces![i].AddIndex;
            newPolygons[newIdx].Normal = _faces![i].Normal;
            newPolygons[newIdx].Distance = _faces![i].Distance;
            newPolygons[newIdx].SurfaceType = _faces![i].SurfaceType;

            Debug.Assert(
                newMaterial[newIdx] == _faces![i].TextureIndex[_polygonOrderPass][_polygonOrderStage],
                "Unable to update the new polygons."
            );
        }

        _faces = newPolygons;
        _allocatedFaceCount = _faceCount;

        _stats.AverageStripLength /= _stats.StripCount;
    }

    private void RemoveDegenerateFaces()
    {
        var faceHasher = new FaceHasher();
        var uniqueFaces = new UniqueArray<Face>(_faceCount, _faceCount / 4, faceHasher);

        for (var faceIdx = 0; faceIdx < _faceCount; faceIdx++)
        {
            if (!_faces![faceIdx].IsDegenerate)
            {
                _ = uniqueFaces.Add(_faces![faceIdx]);
            }
        }

        _faceCount = uniqueFaces.Count;
        _allocatedFaceCount = uniqueFaces.Count;
        _currentFace = _faceCount;

        _faces = new Face[_allocatedFaceCount];
        for (var faceIdx = 0; faceIdx < _faceCount; faceIdx++)
        {
            _faces[faceIdx] = uniqueFaces[faceIdx];
        }
    }

    private void ComputeFaceNormals()
    {
        for (var faceIdx = 0; faceIdx < _faceCount; faceIdx++)
        {
            _faces![faceIdx].ComputePlane();
        }
    }

    private bool VerifyFaceNormals()
    {
        var ok = true;
        for (var faceIdx = 0; faceIdx < _faceCount; faceIdx++)
        {
            Vertex v0 = _vertices![_faces![faceIdx].VertexIndices[0]];
            Vertex v1 = _vertices![_faces![faceIdx].VertexIndices[1]];
            Vertex v2 = _vertices![_faces![faceIdx].VertexIndices[2]];
            var normal = Vector3.CrossProduct(v1.Position - v2.Position, v2.Position - v0.Position);
            normal.Normalize();

            var dn = (_faces![faceIdx].Normal - normal).Length;
            if (dn > float.Epsilon)
            {
                ok = false;
            }
        }

        return ok;
    }

    private void ComputeVertexNormals()
    {
        for (var vertexIdx = 0; vertexIdx < _vertexCount; vertexIdx++)
        {
            _vertices![vertexIdx].Normal = new Vector3(0, 0, 0);
        }

        for (var faceIdx = 0; faceIdx < _faceCount; faceIdx++)
        {
            for (var faceVertexIdx = 0; faceVertexIdx < 3; faceVertexIdx++)
            {
                var vertexIndex = _faces![faceIdx].VertexIndices[faceVertexIdx];
                var shadeIndex = _vertices![vertexIndex].ShadeIndex;
                _vertices![shadeIndex].Normal += _faces![faceIdx].Normal;
            }
        }

        if (WorldInformation?.AreMeshesSmoothed == true)
        {
            for (var vertexIdx = 0; vertexIdx < _vertexCount; vertexIdx++)
            {
                if (_vertices![vertexIdx].ShadeIndex == vertexIdx)
                {
                    _vertices![vertexIdx].Normal += WorldInformation.GetSharedVertexNormal(
                        _vertices![vertexIdx].Position,
                        _vertices![vertexIdx].SharedSmoothingGroup
                    );
                }
            }
        }

        for (var vertexIdx = 0; vertexIdx < _vertexCount; vertexIdx++)
        {
            var shadeIndex = _vertices![vertexIdx].ShadeIndex;
            _vertices![vertexIdx].Normal = _vertices![shadeIndex].Normal;
            _vertices![vertexIdx].Normal.Normalize();
        }
    }

    private void GrowFaceArray()
    {
        var oldCount = _allocatedFaceCount;
        var oldFaces = new Face[_faces!.Length];
        Array.Copy(_faces, oldFaces, _faces.Length);

        _allocatedFaceCount += _allocatedFaceGrowth;
        _faces = new Face[_allocatedFaceCount];
        for (var i = 0; i < oldCount; i++)
        {
            _faces[i] = oldFaces[i];
        }
    }

    private void SortVertices()
    {
        _vertices.Sort(VertexCompare);
        var vertexRemapTable = new int[_vertexCount];
        for (var vi = 0; vi < _vertexCount; vi++)
        {
            vertexRemapTable[_vertices![vi].UniqueIndex] = vi;
        }

        for (var fi = 0; fi < _faceCount; fi++)
        {
            for (var vi = 0; vi < 3; vi++)
            {
                var oldIndex = _faces![fi].VertexIndices[vi];
                _faces![fi].VertexIndices[vi] = vertexRemapTable[oldIndex];
            }
        }
    }

    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instantiated though list/array initialization."
    )]
    private sealed class WingedEdgeClass
    {
        public int MaterialIndex { get; set; }

        public WingedEdgeClass? Next { get; set; }

        public int[] Vertex { get; } = new int[2];

        public int[] Polygons { get; } = new int[2];
    }

    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instantiated though list/array initialization."
    )]
    private sealed class WingedEdgePolygonClass
    {
        public WingedEdgeClass[] Edges { get; } = new WingedEdgeClass[3];
    }

    private sealed class VertexArray
    {
        public const int HashTableSize = 4096;

        private readonly Vertex?[] _hashTable;

        private readonly int _matchNormals;
        private Vector3 _center;
        private Vector3 _extent;

        public VertexArray(int maxSize, int matchNormals = 0)
        {
            Debug.Assert(maxSize > 0, $"Maximum size must be positive, was {maxSize}");

            Vertices = new Vertex[maxSize];
            VertexCount = 0;
            UvSplits = 0;

            _hashTable = new Vertex[HashTableSize];
            _matchNormals = matchNormals;

            _center = new Vector3(0, 0, 0);
            _extent = new Vector3(1, 1, 1);
        }

        public int VertexCount { get; set; }

        public int UvSplits { get; set; }

        public Vertex[] Vertices { get; }

        public static bool VerticesShadingMatch(Vertex v0, Vertex v1)
        {
            ArgumentNullException.ThrowIfNull(v0);
            ArgumentNullException.ThrowIfNull(v1);

            var dv = (v0.Position - v1.Position).Length;
            var smoothingGroupMatch =
                (v0.SmoothingGroup & v1.SmoothingGroup) != 0 || v0.SmoothingGroup == v1.SmoothingGroup;

            return dv < float.Epsilon && smoothingGroupMatch && v0.Id == v1.Id;
        }

        public void SetBounds(Vector3 minVector, Vector3 maxVector)
        {
            _extent = (maxVector - minVector) / 2F;
            _center = (maxVector + minVector) / 2F;
        }

        [SuppressMessage(
            "csharpsquid",
            "S3776:Cognitive Complexity of methods should not be too high",
            Justification = "This is how the algorithm works and splitting it may be more difficult to follow"
        )]
        public int SubmitVertex(Vertex vertex)
        {
            ArgumentNullException.ThrowIfNull(vertex);

            var xStart =
                float.Abs(_extent.X) > float.Epsilon ? (vertex.Position.X - _center.X) / _extent.X : (double)_center.X;

            var yStart =
                float.Abs(_extent.Y) > float.Epsilon ? (vertex.Position.Y - _center.Y) / _extent.Y : (double)_center.Y;

            double x;
            double y;
            uint hash;
            var shadeIndex = 0xFFFF_FFFF;
            var lastHash = 0xFFFF_FFFF;
            for (x = xStart - double.Epsilon; x <= xStart + float.Epsilon + .0000001D; x += double.Epsilon)
            {
                for (y = yStart - double.Epsilon; y <= yStart + float.Epsilon + .0000001D; y += double.Epsilon)
                {
                    hash = ComputeHash((float)x, (float)y);
                    if (hash != lastHash)
                    {
                        Vertex? testVertex = _hashTable[hash];
                        while (testVertex is not null)
                        {
                            if (VerticesShadingMatch(vertex, testVertex) && shadeIndex == 0xFFFF_FFFF)
                            {
                                shadeIndex = (uint)testVertex.UniqueIndex;
                                Vertices[unchecked((int)shadeIndex)].SharedSmoothingGroup &= vertex.SmoothingGroup;
                            }

                            if (VerticesMatch(vertex, testVertex))
                            {
                                return testVertex.UniqueIndex;
                            }

                            testVertex = testVertex.NextHash;
                        }
                    }

                    lastHash = hash;
                }
            }

            var newIndex = VertexCount;
            VertexCount++;

            Vertices[newIndex] = vertex;
            Vertices[newIndex].UniqueIndex = newIndex;
            if (shadeIndex == 0xFFFF_FFFF)
            {
                Vertices[newIndex].ShadeIndex = newIndex;
                Vertices[newIndex].SharedSmoothingGroup = vertex.SmoothingGroup;
            }
            else
            {
                Vertices[newIndex].ShadeIndex = unchecked((int)shadeIndex);
            }

            x = (vertex.Position.X - _center.X) / _extent.X;
            y = (vertex.Position.Y - _center.Y) / _extent.Y;
            hash = ComputeHash((float)x, (float)y);
            Vertices[newIndex].NextHash = _hashTable[hash];
            _hashTable[hash] = Vertices[newIndex];

            return newIndex;
        }

        [SuppressMessage(
            "csharpsquid",
            "S3776:Cognitive Complexity of methods should not be too high",
            Justification = "This is how the algorithm works and splitting it may be more difficult to follow"
        )]
        public bool VerticesMatch(Vertex v0, Vertex v1)
        {
            ArgumentNullException.ThrowIfNull(v0);
            ArgumentNullException.ThrowIfNull(v1);

            if (v0.Id != v1.Id)
            {
                return false;
            }

            var dp = (v0.Position - v1.Position).Length;
            if (dp > float.Epsilon)
            {
                return false;
            }

            if (_matchNormals != 0)
            {
                return true;
            }

            var smoothingGroupMatch =
                (v0.SmoothingGroup & v1.SmoothingGroup) != 0 || v0.SmoothingGroup == v1.SmoothingGroup;

            if (!smoothingGroupMatch)
            {
                return false;
            }

            var dn = (v0.Normal - v1.Normal).Length;
            if (dn > float.Epsilon)
            {
                return false;
            }

            for (var pass = 0; pass < MaxPasses; pass++)
            {
                if (v0.DiffuseColor[pass] != v1.DiffuseColor[pass])
                {
                    return false;
                }

                if (v0.SpecularColor[pass] != v1.SpecularColor[pass])
                {
                    return false;
                }

                if (v0.DiffuseIllumination[pass] != v1.DiffuseIllumination[pass])
                {
                    return false;
                }

                if (float.Abs(v0.Alpha[pass] - v1.Alpha[pass]) > float.Epsilon)
                {
                    return false;
                }

                if (v0.VertexMaterialIndex[pass] != v1.VertexMaterialIndex[pass])
                {
                    return false;
                }
            }

            for (var pass = 0; pass < MaxPasses; pass++)
            {
                for (var stage = 0; stage < MaxStages; stage++)
                {
                    if (v0.TextureCoordinates[pass][stage] == v1.TextureCoordinates[pass][stage])
                    {
                        continue;
                    }

                    UvSplits++;
                    return false;
                }
            }

            return true;
        }

        public void PropagateSharedSmoothGroups()
        {
            for (var i = 0; i < VertexCount; i++)
            {
                if (Vertices[i].ShadeIndex != i)
                {
                    Vertices[i].SharedSmoothingGroup = Vertices[Vertices[i].ShadeIndex].SharedSmoothingGroup;
                }
            }
        }

        public Vertex this[int index] => Vertices[index];

        private static uint ComputeHash(float x, float y)
        {
            var ix = (int)x & 0x0000_003F;
            var iy = (int)y & 0x0000_003F;
            return (uint)((iy << 6) | ix);
        }
    }

    private sealed class FaceHasher : IHashCalculator<Face>
    {
        private int _hashValue;

        public int HashBitsCount => 10;

        public int HashValuesCount => 1;

        public bool ItemsMatch(Face x, Face y)
        {
            ArgumentNullException.ThrowIfNull(x);
            ArgumentNullException.ThrowIfNull(y);

            return x.VertexIndices[0] == y.VertexIndices[0]
                && x.VertexIndices[1] == y.VertexIndices[1]
                && x.VertexIndices[2] == y.VertexIndices[2];
        }

        public void ComputeHash(Face item)
        {
            ArgumentNullException.ThrowIfNull(item);

            _hashValue =
                (int)(
                    (item.VertexIndices[0] * 12_345.6F)
                    + (item.VertexIndices[1] * 1_714.38484F)
                    + (item.VertexIndices[2] * 27_561.3F)
                ) & 1023;
        }

        public int GetHashValue(int index = 0) => _hashValue;
    }
}
