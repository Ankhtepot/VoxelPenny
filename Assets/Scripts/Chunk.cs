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
using Random = UnityEngine.Random;

public class Chunk : MonoBehaviour
{
    public Vector3 location;
    public Material atlas;
    public TileConfiguration grassConfiguration;
    [HideInInspector] public int width;
    [HideInInspector] public int height;
    [HideInInspector] public int depth;

    public Block[,,] blocks;

    // Flat[x + Width * (y + Depth * z)] = Original[x, y, z]
    // x = i % Width
    // y = (i / Width) % Height
    // z = i / (Width * Height)

    public EBlockType[] chunkData;

    private int _blockCount;

    public void CreateChunk(Vector3 dimensions, Vector3 position, WorldLayers layers)
    {
        location = position;
        width = (int) dimensions.x;
        height = (int) dimensions.y;
        depth = (int) dimensions.z;

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = atlas;
        _blockCount = width * height * depth;
        blocks = new Block[width, height, depth];
        BuildChunk(layers);

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
                    EBlockType blockType = chunkData[FlattenXYZ(x, y, z)];

                    TileConfiguration configuration = blockType switch
                    {
                        EBlockType.ConfiguredGrassCube => grassConfiguration,
                        _ => null
                    };

                    blocks[x, y, z] = new Block(
                        blockType,
                        new Vector3(x, y, z) + location,
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
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2));

        JobHandle handle = jobs.Schedule(inputMeshes.Count, 4);

        Mesh newMesh = new()
        {
            name = $"Chunk_{location.x}_{location.y}_{location.z}"
        };

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

    private void BuildChunk(WorldLayers worldLayers)
    {
        chunkData = new EBlockType[_blockCount];

        for (int i = 0; i < _blockCount; i++)
        {
            UnFlatten(i, out float x, out float y, out float z);

            bool drawDirt = true;

            foreach (WorldLayer layer in worldLayers.Layers)
            {
                if (y > MeshUtils.fBM(x, z, worldLayers.SurfaceSettings)
                    || y < Mathf.Floor(MeshUtils.fBM(x, z, worldLayers.BedrockSettings)))
                {
                    chunkData[i] = EBlockType.Air;
                    continue;
                }

                float layerTopHeight = MeshUtils.fBM(x, z, layer.layers[0]);

                if (IsCaveLayer(x, y, z, layer)
                    || IsEqualLayer(y, layer, layerTopHeight)
                    || IsLessThanLayer(y, layer, layerTopHeight)
                    || IsBetweenLayer(x, y, z, layer, layerTopHeight)
                )
                {
                    chunkData[i] = layer.blockType;
                    drawDirt = false;
                    continue;
                }

                if (drawDirt)
                {
                    chunkData[i] = EBlockType.Dirt;
                }
            }
        }
    }

    private bool IsEqualLayer(float y, WorldLayer layer, float topHeight) =>
        (layer.layerType == ELayerType.Equal && (int) y == (int) topHeight);

    private bool IsLessThanLayer(float y, WorldLayer layer, float topHeight) =>
        (layer.layerType == ELayerType.LessThen && y < topHeight &&
         Random.Range(0f, 1f) < layer.probability);

    private bool IsBetweenLayer(float x, float y, float z, WorldLayer layer, float topHeight)
    {
        float layerBottomHeight = layer.layers.Count == 2 ? MeshUtils.fBM(x, z, layer.layers[1]) : 0;
        return (layer.layerType == ELayerType.BetweenTwo && y >= layerBottomHeight && y <= topHeight &&
                Random.Range(0f, 1f) <= layer.probability);
    }

    private bool IsCaveLayer(float x, float y, float z, WorldLayer layer)
    {
        if (layer.layerType != ELayerType.Cave) return false;

        CavePNSettings settings = (CavePNSettings) layer.layers[0];

        return MeshUtils.fBM3D(x, y, z, settings) < settings.DrawCutoff && Random.Range(0f, 1f) < layer.probability;
    }

    private int FlattenXYZ(int x, int y, int z)
    {
        return x + width * (y + depth * z);
    }

    private void UnFlatten(int index, out float x, out float y, out float z)
    {
        x = index % width + location.x;
        y = (index / width) % height + location.y;
        z = index / (width * height) + location.z;
    }

    [BurstCompile]
    struct ProcessMeshDataJob : IJobParallelFor
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

            NativeArray<Vector3> outputVerts = outputMesh.GetVertexData<Vector3>();
            NativeArray<Vector3> outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
            NativeArray<Vector3> outputUVs = outputMesh.GetVertexData<Vector3>(stream: 2);

            for (int i = 0; i < vCount; i++)
            {
                outputVerts[i + vStart] = verts[i];
                outputNormals[i + vStart] = normals[i];
                outputUVs[i + vStart] = uvs[i];
            }

            verts.Dispose();
            normals.Dispose();
            uvs.Dispose();

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
}