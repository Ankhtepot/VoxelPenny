using System;
using System.Collections.Generic;
using DefaultNamespace;
using Scripts;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static S2_Quad.BlockAtlas;
using Random = Unity.Mathematics.Random;

public class Chunk : MonoBehaviour
{
    public Vector3Int location;
    public Material atlas;
    public TileConfiguration grassConfiguration;
    [HideInInspector] public int width;
    [HideInInspector] public int height;
    [HideInInspector] public int depth;

    private Block[,,] blocks;

    // Flat[x + Width * (y + Depth * z)] = Original[x, y, z]
    // x = i % Width
    // y = (i / Width) % Height
    // z = i / (Width * Height)

    public EBlockType[] chunkData;
    public EBlockType[] healthData;
    [HideInInspector] public MeshRenderer MeshRenderer;

    private CalculateBlockTypes _calculateBlockTypes;
    private JobHandle _jobHandle;
    private NativeArray<Random> RandomArray { get; set; }

    private int _blockCount;

    public void CreateChunk(Vector3Int dimensions, Vector3Int position, bool rebuildBlocks = true)
    {
        location = position;
        width = dimensions.x;
        height = dimensions.y;
        depth = dimensions.z;

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshRenderer.material = atlas;
        _blockCount = width * height * depth;
        blocks = new Block[width, height, depth];
        
        if (rebuildBlocks)
        {
            BuildChunk();
        }

        List<Mesh> inputMeshes = new();
        int vertexStart = 0;
        int triStart = 0;
        int blocksCounter = 0;
        ProcessMeshDataJob jobs = new()
        {
            vertexStart = new NativeArray<int>(_blockCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
            triStart = new NativeArray<int>(_blockCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
        };

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = FlattenXYZ(x, y, z);
                    EBlockType blockType = chunkData[index];
                    EBlockType healthType = healthData[index];

                    TileConfiguration configuration = blockType switch
                    {
                        EBlockType.ConfiguredGrassCube => grassConfiguration,
                        _ => null
                    };

                    blocks[x, y, z] = new Block(
                        blockType, healthType,
                        new Vector3Int(x, y, z) + location,
                        this,
                        configuration
                    );

                    Block currentBlock = blocks[x, y, z];

                    if (!currentBlock.mesh) continue;

                    inputMeshes.Add(currentBlock.mesh);
                    int vCount = currentBlock.mesh.vertexCount;
                    int iCount = (int) currentBlock.mesh.GetIndexCount(0);
                    jobs.vertexStart[blocksCounter] = vertexStart;
                    jobs.triStart[blocksCounter] = triStart;
                    vertexStart += vCount;
                    triStart += iCount;
                    blocksCounter++;
                }
            }
        }

        jobs.meshData = Mesh.AcquireReadOnlyMeshData(inputMeshes);
        Mesh.MeshDataArray outputMeshData = Mesh.AllocateWritableMeshData(1);
        jobs.outputMesh = outputMeshData[0];
        jobs.outputMesh.SetIndexBufferParams(triStart, IndexFormat.UInt32);
        jobs.outputMesh.SetVertexBufferParams(vertexStart,
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, stream: 3)
        );

        JobHandle handle = jobs.Schedule(inputMeshes.Count, 4);

        Mesh newMesh = new()
        {
            name = $"Chunk_{location.x}_{location.y}_{location.z}"
        };

        // ReSharper disable once RedundantArgumentDefaultValue - MeshTopology.Triangles left for reference even its default value
        SubMeshDescriptor sm = new(0, triStart, MeshTopology.Triangles)
        {
            firstVertex = 0,
            vertexCount = vertexStart
        };

        handle.Complete();

        jobs.outputMesh.subMeshCount = 1;
        jobs.outputMesh.SetSubMesh(0, sm);

        Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[] {newMesh});

        jobs.meshData.Dispose();
        jobs.vertexStart.Dispose();
        jobs.triStart.Dispose();
        newMesh.RecalculateBounds();

        mf.mesh = newMesh;
        MeshCollider myCollider = gameObject.AddComponent<MeshCollider>();
        myCollider.sharedMesh = mf.mesh;
    }

    private void BuildChunk()
    {
        chunkData = new EBlockType[_blockCount];
        healthData = new EBlockType[_blockCount];

        NativeArray<EBlockType> blockTypes = new(chunkData, Allocator.Persistent);
        NativeArray<EBlockType> healthTypes = new(healthData, Allocator.Persistent);

        Random[] randomArray = new Random[_blockCount];
        System.Random seed = new();

        for (int i = 0; i < _blockCount; ++i)
            randomArray[i] = new Random((uint)seed.Next());

        RandomArray = new NativeArray<Random>(randomArray, Allocator.Persistent);

        _calculateBlockTypes = new CalculateBlockTypes
        {
            chunkData = blockTypes,
            healthData = healthTypes,
            width = width,
            height = height,
            location = location,
            randoms = RandomArray,
        };

        _jobHandle = _calculateBlockTypes.Schedule(chunkData.Length, 64);
        _jobHandle.Complete();
        _calculateBlockTypes.chunkData.CopyTo(chunkData);
        _calculateBlockTypes.healthData.CopyTo(healthData);
        blockTypes.Dispose();
        healthTypes.Dispose();
        RandomArray.Dispose();
    }
    
    private int FlattenXYZ(int x, int y, int z)
    {
        return x + width * (y + depth * z);
    }

    [BurstCompile]
    private struct ProcessMeshDataJob : IJobParallelFor
    {
        [ReadOnly] public Mesh.MeshDataArray meshData;
        public Mesh.MeshData outputMesh;
        public NativeArray<int> vertexStart;
        public NativeArray<int> triStart;

        public void Execute(int index)
        {
            Mesh.MeshData data = meshData[index];
            int vCount = data.vertexCount;
            int vStart = vertexStart[index];

            NativeArray<float3> verts = new(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetVertices(verts.Reinterpret<Vector3>());

            NativeArray<float3> normals = new(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetVertices(normals.Reinterpret<Vector3>());

            NativeArray<float3> uvs = new(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetUVs(0, uvs.Reinterpret<Vector3>());
            
            NativeArray<float3> uvs2 = new(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetUVs(1, uvs2.Reinterpret<Vector3>());

            NativeArray<Vector3> outputVerts = outputMesh.GetVertexData<Vector3>();
            NativeArray<Vector3> outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
            NativeArray<Vector3> outputUVs = outputMesh.GetVertexData<Vector3>(stream: 2);
            NativeArray<Vector3> outputUVs2 = outputMesh.GetVertexData<Vector3>(stream: 3);

            for (int i = 0; i < vCount; i++)
            {
                outputVerts[i + vStart] = verts[i];
                outputNormals[i + vStart] = normals[i];
                outputUVs[i + vStart] = uvs[i];
                outputUVs2[i + vStart] = uvs2[i];
            }

            verts.Dispose();
            normals.Dispose();
            uvs.Dispose();
            uvs2.Dispose();

            int tStart = triStart[index];
            int tCount = data.GetSubMesh(0).indexCount;
            NativeArray<int> outputTris = outputMesh.GetIndexData<int>();

            if (data.indexFormat == IndexFormat.UInt16)
            {
                NativeArray<ushort> tris = data.GetIndexData<ushort>();
                for (int i = 0; i < tCount; i++)
                {
                    outputTris[i + tStart] = vStart + tris[i];
                }
            }
            else
            {
                NativeArray<int> tris = data.GetIndexData<int>();
                for (int i = 0; i < tCount; i++)
                {
                    outputTris[i + tStart] = vStart + tris[i];
                }
            }
        }
    }

    private struct CalculateBlockTypes : IJobParallelFor
    {
        public NativeArray<EBlockType> chunkData;
        public NativeArray<EBlockType> healthData;
        public int width;
        public int height;
        public Vector3 location;
        public NativeArray<Random> randoms;

        public void Execute(int i)
        {
            UnFlatten(i, out float x, out float y, out float z);

            Random random = randoms[i];

            PerlinNoiseSettings surfaceSettings = World.WorldLayers.SurfaceSettings;
            PerlinNoiseSettings stoneSettings = World.WorldLayers.StoneSettings;
            PerlinNoiseSettings diamondTSettings = World.WorldLayers.DiamondTopSettings;
            PerlinNoiseSettings diamondBSettings = World.WorldLayers.DiamondBottomSettings;
            CavePNSettings caveSettings = World.WorldLayers.CaveSettings;

            int surfaceHeight = (int)MeshUtils.fBM(x, z, surfaceSettings.Octaves,
                                                   surfaceSettings.Scale, surfaceSettings.HeightScale,
                                                   surfaceSettings.HeightOffset);

            int stoneHeight = (int)MeshUtils.fBM(x, z, stoneSettings.Octaves,
                                                   stoneSettings.Scale, stoneSettings.HeightScale,
                                                   stoneSettings.HeightOffset);

            int diamondTHeight = (int)MeshUtils.fBM(x, z, diamondTSettings.Octaves,
                                       diamondTSettings.Scale, diamondTSettings.HeightScale,
                                       diamondTSettings.HeightOffset);

            int diamondBHeight = (int)MeshUtils.fBM(x, z, diamondBSettings.Octaves,
                           diamondBSettings.Scale, diamondBSettings.HeightScale,
                           diamondBSettings.HeightOffset);

            int digCave = (int)MeshUtils.fBM3D(x, y, z, caveSettings.Octaves,
                           caveSettings.Scale, caveSettings.HeightScale,
                           caveSettings.HeightOffset);

            healthData[i] = EBlockType.NoCrack;
            
            if (y == 0)
            {
                chunkData[i] = EBlockType.Bedrock;
                return;
            }
            
            if (digCave < World.WorldLayers.CaveSettings.DrawCutoff)
            {
                chunkData[i] = EBlockType.Air;
                return;
            }

            if (Math.Abs(surfaceHeight - y) < Single.Epsilon)
            {
                chunkData[i] = EBlockType.ConfiguredGrassCube;
            }
            else if (y < diamondTHeight && y > diamondBHeight && random.NextFloat(1) <= World.WorldLayers.DiamondLayer.probability)
                chunkData[i] = EBlockType.Diamond;
            else if (y < stoneHeight && random.NextFloat(1) <= World.WorldLayers.StoneLayer.probability)
                chunkData[i] = EBlockType.WallSmallStones;
            else if (y < surfaceHeight)
                chunkData[i] = EBlockType.Dirt;
            else
                chunkData[i] = EBlockType.Air;
        }

        private void UnFlatten(int index, out float x, out float y, out float z)
        {
            x = index % width + location.x;
            y = (index / width) % height + location.y;
            // ReSharper disable once PossibleLossOfFraction - should be OK here
            z = index / (width * height) + location.z;
        }
    }
}