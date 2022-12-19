using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static MeshUtils;

public class Chunk : MonoBehaviour
{
    public Material atlas;
    public Material fluid;
    
    public int width = 2;
    public int height = 2;
    public int depth = 2;

    public Vector3 location;

    public Block[,,] blocks;
    //Flat[x + WIDTH * (y + DEPTH * z)] = Original[x, y, z]
    //x = i % WIDTH
    //y = (i / WIDTH) % HEIGHT
    //z = i / (WIDTH * HEIGHT )
    public BlockType[] chunkData;
    public BlockType[] healthData;
    public MeshRenderer meshRendererSolid;
    public MeshRenderer meshRendererFluid;
    private GameObject solidMesh;
    private GameObject fluidMesh;

    CalculateBlockTypes calculateBlockTypes;
    JobHandle jobHandle;
    public NativeArray<Unity.Mathematics.Random> RandomArray { get; private set; }

    struct CalculateBlockTypes : IJobParallelFor
    {
        public NativeArray<BlockType> cData;
        public NativeArray<BlockType> hData;
        public int width;
        public int height;
        public Vector3 location;
        public NativeArray<Unity.Mathematics.Random> randoms;

        public void Execute(int i)
        {
            int x = i % width + (int)location.x;
            int y = (i / width) % height + (int)location.y;
            int z = i / (width * height) + (int)location.z;

            var random = randoms[i];

            int surfaceHeight = (int)fBM(x, z, World.surfaceSettings.octaves,
                                                   World.surfaceSettings.scale, World.surfaceSettings.heightScale,
                                                   World.surfaceSettings.heightOffset);

            int stoneHeight = (int)fBM(x, z, World.stoneSettings.octaves,
                                                   World.stoneSettings.scale, World.stoneSettings.heightScale,
                                                   World.stoneSettings.heightOffset);

            int diamondTHeight = (int)fBM(x, z, World.diamondTSettings.octaves,
                                       World.diamondTSettings.scale, World.diamondTSettings.heightScale,
                                       World.diamondTSettings.heightOffset);

            int diamondBHeight = (int)fBM(x, z, World.diamondBSettings.octaves,
                           World.diamondBSettings.scale, World.diamondBSettings.heightScale,
                           World.diamondBSettings.heightOffset);

            int digCave = (int)fBM3D(x, y, z, World.caveSettings.octaves,
                           World.caveSettings.scale, World.caveSettings.heightScale,
                           World.caveSettings.heightOffset);

            int plantTree = (int)fBM3D(x, y, z, World.treeSettings.octaves,
               World.treeSettings.scale, World.treeSettings.heightScale,
               World.treeSettings.heightOffset);

            hData[i] = BlockType.NOCRACK;

            if (y == 0)
            {
                cData[i] = BlockType.BEDROCK;
                return;
            }

            if (digCave < World.caveSettings.probability)
            {
                cData[i] = BlockType.AIR;
                return;
            }

            if (surfaceHeight == y)
            {
                if (plantTree < World.treeSettings.probability && random.NextFloat(1) <= 0.1)
                {
                    cData[i] = BlockType.WOODBASE;
                }
                else
                    cData[i] = BlockType.GRASSSIDE;
            }
            else if (y < diamondTHeight && y > diamondBHeight && random.NextFloat(1) <= World.diamondTSettings.probability)
                cData[i] = BlockType.DIAMOND;
            else if (y < stoneHeight && random.NextFloat(1) <= World.stoneSettings.probability)
                cData[i] = BlockType.STONE;
            else if (y < surfaceHeight)
                cData[i] = BlockType.DIRT;
            else if (y < 20)
                cData[i] = BlockType.WATER;
            else
                cData[i] = BlockType.AIR;
        }
    }

    void BuildChunk()
    {
        int blockCount = width * depth * height;
        chunkData = new BlockType[blockCount];
        healthData = new BlockType[blockCount];
        NativeArray<BlockType> blockTypes = new(chunkData, Allocator.Persistent);
        NativeArray<BlockType> healthTypes = new(healthData, Allocator.Persistent);

        var randomArray = new Unity.Mathematics.Random[blockCount];
        var seed = new System.Random();

        for (int i = 0; i < blockCount; ++i)
            randomArray[i] = new Unity.Mathematics.Random((uint)seed.Next());

        RandomArray = new NativeArray<Unity.Mathematics.Random>(randomArray, Allocator.Persistent);

        calculateBlockTypes = new CalculateBlockTypes()
        {
            cData = blockTypes,
            hData = healthTypes,
            width = width,
            height = height,
            location = location,
            randoms = RandomArray
        };

        jobHandle = calculateBlockTypes.Schedule(chunkData.Length, 64);
        jobHandle.Complete();
        calculateBlockTypes.cData.CopyTo(chunkData);
        calculateBlockTypes.hData.CopyTo(healthData);
        blockTypes.Dispose();
        healthTypes.Dispose();
        RandomArray.Dispose();

        BuildTrees();
    }

    private (Vector3Int, BlockType)[] treeDesign = new (Vector3Int, BlockType)[]
    {
        (new Vector3Int(-1,1,-1), BlockType.LEAVES),
        (new Vector3Int(0,1,-1), BlockType.LEAVES),
        (new Vector3Int(0,2,-1), BlockType.LEAVES),
        (new Vector3Int(1,2,-1), BlockType.LEAVES),
        (new Vector3Int(-1,3,-1), BlockType.LEAVES),
        (new Vector3Int(0,4,-1), BlockType.LEAVES),
        (new Vector3Int(0,0,0), BlockType.WOOD),
        (new Vector3Int(-1,1,0), BlockType.LEAVES),
        (new Vector3Int(0,1,0), BlockType.WOOD),
        (new Vector3Int(-1,2,0), BlockType.LEAVES),
        (new Vector3Int(0,2,0), BlockType.LEAVES),
        (new Vector3Int(1,2,0), BlockType.LEAVES),
        (new Vector3Int(-1,3,0), BlockType.LEAVES),
        (new Vector3Int(0,3,0), BlockType.WOOD),
        (new Vector3Int(1,3,0), BlockType.LEAVES),
        (new Vector3Int(-1,4,0), BlockType.LEAVES),
        (new Vector3Int(0,4,0), BlockType.LEAVES),
        (new Vector3Int(1,4,0), BlockType.LEAVES),
        (new Vector3Int(0,5,0), BlockType.LEAVES),
        (new Vector3Int(0,1,1), BlockType.LEAVES),
        (new Vector3Int(-1,2,1), BlockType.LEAVES),
        (new Vector3Int(0,2,1), BlockType.LEAVES),
        (new Vector3Int(0,3,1), BlockType.LEAVES),
        (new Vector3Int(1,3,1), BlockType.LEAVES),
        (new Vector3Int(-1,4,1), BlockType.LEAVES),
        (new Vector3Int(0,4,1), BlockType.LEAVES),
    };
    
    private void BuildTrees()
    {
        for (int i = 0; i < chunkData.Length; i++)
        {
            if (chunkData[i] == BlockType.WOODBASE)
            {
                foreach ((Vector3Int, BlockType) valueTuple in treeDesign)
                {
                    Vector3Int blockPos = World.FromFlat(i) + valueTuple.Item1;
                    int bIndex = World.ToFlat(blockPos);
                    if (bIndex >= 0 && bIndex < chunkData.Length)
                    {
                        chunkData[bIndex] = valueTuple.Item2;
                        healthData[bIndex] = BlockType.NOCRACK;
                    }
                }
                
            }
        }
    } 

    // Start is called before the first frame update
    void Start()
    {

    }

    public void CreateChunk(Vector3 dimensions, Vector3 position, bool rebuildBlocks = true)
    {
        location = position;
        width = (int) dimensions.x;
        height = (int)dimensions.y;
        depth = (int)dimensions.z;

        MeshFilter mfs; //solid
        MeshRenderer mrs;
        MeshFilter mff; //fluid
        MeshRenderer mrf;


        if (solidMesh == null)
        {
            solidMesh = new GameObject("Solid");
            solidMesh.transform.parent = this.gameObject.transform;
            mfs = solidMesh.AddComponent<MeshFilter>();
            mrs = solidMesh.AddComponent<MeshRenderer>();
            meshRendererSolid = mrs;
            mrs.material = atlas;
        }
        else
        {
            mfs = solidMesh.GetComponent<MeshFilter>();
            DestroyImmediate(solidMesh.GetComponent<Collider>());
        }

        if (fluidMesh == null)
        {
            fluidMesh = new GameObject("Fluid");
            fluidMesh.transform.parent = gameObject.transform;
            mff = fluidMesh.AddComponent<MeshFilter>();
            mrf = fluidMesh.AddComponent<MeshRenderer>();
            meshRendererFluid = mrf;
            mrf.material = fluid;
        }
        else
        {
            mff = fluidMesh.GetComponent<MeshFilter>();
            DestroyImmediate(fluidMesh.GetComponent<Collider>());
        }
        
        blocks = new Block[width, height, depth];
        if(rebuildBlocks)
            BuildChunk();

        for (int pass = 0; pass < 2; pass++)
        {
            
            var inputMeshes = new List<Mesh>();
            int vertexStart = 0;
            int triStart = 0;
            int meshCount = width * height * depth;
            int m = 0;
            var jobs = new ProcessMeshDataJob();
            jobs.vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            jobs.triStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);


            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        blocks[x, y, z] = new Block(new Vector3(x, y, z) + location,
                            chunkData[x + width * (y + depth * z)], this,
                            healthData[x + width * (y + depth * z)]);
                        if (blocks[x, y, z].mesh != null
                            && ((pass == 0 && !canFlow.Contains(chunkData[x + width * (y + depth * z)]))
                            || (pass == 1 && canFlow.Contains(chunkData[x + width * (y + depth * z)]))))
                        {
                            inputMeshes.Add(blocks[x, y, z].mesh);
                            var vcount = blocks[x, y, z].mesh.vertexCount;
                            var icount = (int) blocks[x, y, z].mesh.GetIndexCount(0);
                            jobs.vertexStart[m] = vertexStart;
                            jobs.triStart[m] = triStart;
                            vertexStart += vcount;
                            triStart += icount;
                            m++;
                        }
                    }
                }
            }

            jobs.meshData = Mesh.AcquireReadOnlyMeshData(inputMeshes);
            var outputMeshData = Mesh.AllocateWritableMeshData(1);
            jobs.outputMesh = outputMeshData[0];
            jobs.outputMesh.SetIndexBufferParams(triStart, IndexFormat.UInt32);
            jobs.outputMesh.SetVertexBufferParams(vertexStart,
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, stream: 3));

            var handle = jobs.Schedule(inputMeshes.Count, 4);
            var newMesh = new Mesh();
            newMesh.name = "Chunk_" + location.x + "_" + location.y + "_" + location.z;
            var sm = new SubMeshDescriptor(0, triStart, MeshTopology.Triangles);
            sm.firstVertex = 0;
            sm.vertexCount = vertexStart;

            handle.Complete();

            jobs.outputMesh.subMeshCount = 1;
            jobs.outputMesh.SetSubMesh(0, sm);
            Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[] {newMesh});
            jobs.meshData.Dispose();
            jobs.vertexStart.Dispose();
            jobs.triStart.Dispose();
            newMesh.RecalculateBounds();

            if (pass == 0)
            {
                mfs.mesh = newMesh;
                MeshCollider collider = solidMesh.AddComponent<MeshCollider>();
                collider.sharedMesh = mfs.mesh;
            }
            else
            {
                mff.mesh = newMesh;
                MeshCollider collider = fluidMesh.AddComponent<MeshCollider>();
                fluidMesh.layer = 4; // water
                collider.sharedMesh = mff.mesh;
            }
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
            var data = meshData[index];
            var vCount = data.vertexCount;
            var vStart = vertexStart[index];

            var verts = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetVertices(verts.Reinterpret<Vector3>());

            var normals = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetNormals(normals.Reinterpret<Vector3>());

            var uvs = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetUVs(0, uvs.Reinterpret<Vector3>());

            var uvs2 = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetUVs(1, uvs2.Reinterpret<Vector3>());

            var outputVerts = outputMesh.GetVertexData<Vector3>();
            var outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
            var outputUVs = outputMesh.GetVertexData<Vector3>(stream: 2);
            var outputUVs2 = outputMesh.GetVertexData<Vector3>(stream: 3);

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

            var tStart = triStart[index];
            var tCount = data.GetSubMesh(0).indexCount;
            var outputTris = outputMesh.GetIndexData<int>();
            if (data.indexFormat == IndexFormat.UInt16)
            {
                var tris = data.GetIndexData<ushort>();
                for (int i = 0; i < tCount; ++i)
                {
                    int idx = tris[i];
                    outputTris[i + tStart] = vStart + idx;
                }
            }
            else
            {
                var tris = data.GetIndexData<int>();
                for (int i = 0; i < tCount; ++i)
                {
                    int idx = tris[i];
                    outputTris[i + tStart] = vStart + idx;
                }
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
