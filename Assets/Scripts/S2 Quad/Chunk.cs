using System.Collections.Generic;
using S2_Quad;
using UnityEngine;

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

        var inputMeshes = new List<Mesh>(width * height * depth);
        int vertexStart = 0;
        int triStart = 0;

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    blocks[x, y, z] = new Block(BlockAtlas.EBlockType.Dirt, new Vector3(x, y, z));
                }
            }
        }
        
        transform.position = position;
    }
}
