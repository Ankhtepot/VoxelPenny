using System.Collections.Generic;
using S2_Quad;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk : MonoBehaviour
{
    public Vector3 position;
    public Material atlas;
    public int width;
    public int height;
    public int depth;

    public Block[,,] blocks;

    private void Start()
    {
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = atlas;
        blocks = new Block[width, height, depth];

        int meshCount = width * height * depth;
        List<Mesh> inputMeshes = new List<Mesh>(meshCount);
        int vertexStart = 0;
        int triStart = 0;
        int blocksCounter = 0;
        var jobs = new ProcessMeshDataJob();
        jobs.vertexStart =
            new NativeArray<int>(meshCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        jobs.triStart =
            new NativeArray<int>(meshCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    blocks[x, y, z] = new Block(BlockAtlas.EBlockType.Dirt, new Vector3(x, y, z));
                    Block currentBlock = blocks[x, y, z];
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

        transform.position = position;
    }
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

        NativeArray<float3> verts =
            new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        data.GetVertices(verts.Reinterpret<Vector3>());

        var normals = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        data.GetVertices(normals.Reinterpret<Vector3>());

        var uvs = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        data.GetUVs(0, uvs.Reinterpret<Vector3>());

        var outputVerts = outputMesh.GetVertexData<Vector3>();
        var outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
        var outputUVs = outputMesh.GetVertexData<Vector3>(stream: 2);

        for (int i = 0; i < vCount; i++)
        {
            outputVerts[i + vStart] = verts[i];
            outputNormals[i + vStart] = normals[i];
            outputUVs[i + vStart] = uvs[i];
        }

        verts.Dispose();
        normals.Dispose();
        uvs.Dispose();
    }
}