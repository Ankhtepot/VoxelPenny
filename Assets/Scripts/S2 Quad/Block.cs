using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Extensions;
using UnityEngine;
using static MeshUtils;
using static S2_Quad.BlockAtlas;

public class Block
{
    public Vector3 position;
    public Mesh mesh;
    private Chunk parentChunk;

    private EBlockType _originTile;
    private static Vector3Int intOffset;

    private List<EBlockType> IgnoredNeighbourTypes = new List<EBlockType> {EBlockType.Air, EBlockType.Water};

    static Block()
    {
        intOffset = new Vector3Int();
    }

    public Block(EBlockType type, Vector3 offset, Chunk chunk, TileConfiguration tileConfiguration = null)
    {
        parentChunk = chunk;

        if (type == EBlockType.Air) return;
        
        List<Mesh> quads = new List<Mesh>();
        intOffset = offset.ToVector3Int();
        bool isTileConfigured = tileConfiguration != null;
        if (!HasSolidNeighbour(intOffset.x, intOffset.y - 1, intOffset.z))
        {
            quads.Add(new Quad(EBlockSide.Bottom, offset, isTileConfigured ? tileConfiguration.bottomTile : type).mesh);
        }

        if (!HasSolidNeighbour(intOffset.x, intOffset.y + 1, intOffset.z))
        {
            quads.Add(new Quad(EBlockSide.Top, offset, isTileConfigured ? tileConfiguration.bottomTile : type).mesh);
        }

        if (!HasSolidNeighbour(intOffset.x - 1, intOffset.y, intOffset.z))
        {
            quads.Add(new Quad(EBlockSide.Left, offset, isTileConfigured ? tileConfiguration.bottomTile : type).mesh);
        }

        if (!HasSolidNeighbour(intOffset.x + 1, intOffset.y, intOffset.z))
        {
            quads.Add(new Quad(EBlockSide.Right, offset, isTileConfigured ? tileConfiguration.bottomTile : type).mesh);
        }

        if (!HasSolidNeighbour(intOffset.x, intOffset.y, intOffset.z + 1))
        {
            quads.Add(new Quad(EBlockSide.Front, offset, isTileConfigured ? tileConfiguration.bottomTile : type).mesh);
        }

        if (!HasSolidNeighbour(intOffset.x, intOffset.y, intOffset.z - 1))
        {
            quads.Add(new Quad(EBlockSide.Back, offset, isTileConfigured ? tileConfiguration.bottomTile : type).mesh);
        }

        if (quads.Count == 0) return;

        mesh = MergeMeshes(quads);
        mesh.name = $"Cube_{position.x}_{position.y}_{position.z}_generated";
    }

    public bool HasSolidNeighbour(int x, int y, int z)
    {
        if (x < 0 || x >= parentChunk.width
                  || y < 0 || y >= parentChunk.height
                  || z < 0 || z >= parentChunk.depth) return false;

        return !IgnoredNeighbourTypes.Contains(parentChunk.chunkData[FlattenXYZ(x, y, z)]);
    }

    private int FlattenXYZ(int x, int y, int z)
    {
        return x + parentChunk.width * (y + parentChunk.depth * z);
    }
}