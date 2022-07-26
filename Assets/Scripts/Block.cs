﻿using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using static MeshUtils;
using static S2_Quad.BlockAtlas;

public class Block
{
    public readonly Mesh mesh;
    private Vector3 position;
    private readonly Chunk _parentChunk;
    
    private readonly List<EBlockType> _ignoredNeighbourTypes = new() {EBlockType.Air, EBlockType.Water};

    public Block(EBlockType type, EBlockType hType, Vector3Int offset, Chunk chunk, TileConfiguration tileConfiguration = null)
    {
        _parentChunk = chunk;
        Vector3Int blockLocalPos = (offset - chunk.location);

        if (type == EBlockType.Air) return;
        
        List<Mesh> quads = new();
        bool isTileConfigured = tileConfiguration;
        if (!HasSolidNeighbour(blockLocalPos.x, blockLocalPos.y - 1, blockLocalPos.z))
        {
            quads.Add(new Quad(EBlockSide.Bottom, offset, isTileConfigured ? tileConfiguration!.bottomTile : type, hType).mesh);
        }

        if (!HasSolidNeighbour(blockLocalPos.x, blockLocalPos.y + 1, blockLocalPos.z))
        {
            quads.Add(new Quad(EBlockSide.Top, offset, isTileConfigured ? tileConfiguration!.topTile : type, hType).mesh);
        }

        if (!HasSolidNeighbour(blockLocalPos.x - 1, blockLocalPos.y, blockLocalPos.z))
        {
            quads.Add(new Quad(EBlockSide.Left, offset, isTileConfigured ? tileConfiguration!.leftTile : type, hType).mesh);
        }

        if (!HasSolidNeighbour(blockLocalPos.x + 1, blockLocalPos.y, blockLocalPos.z))
        {
            quads.Add(new Quad(EBlockSide.Right, offset, isTileConfigured ? tileConfiguration!.rightTile : type, hType).mesh);
        }

        if (!HasSolidNeighbour(blockLocalPos.x, blockLocalPos.y, blockLocalPos.z + 1))
        {
            quads.Add(new Quad(EBlockSide.Front, offset, isTileConfigured ? tileConfiguration!.frontTile : type, hType).mesh);
        }

        if (!HasSolidNeighbour(blockLocalPos.x, blockLocalPos.y, blockLocalPos.z - 1))
        {
            quads.Add(new Quad(EBlockSide.Back, offset, isTileConfigured ? tileConfiguration!.backTile : type, hType).mesh);
        }

        if (quads.Count == 0) return;

        mesh = MergeMeshes(quads);
        mesh.name = $"Cube_{position.x}_{position.y}_{position.z}_generated";
    }

    private bool HasSolidNeighbour(int x, int y, int z)
    {
        if (x < 0 || x >= _parentChunk.width
                  || y < 0 || y >= _parentChunk.height
                  || z < 0 || z >= _parentChunk.depth) return false;

        return !_ignoredNeighbourTypes.Contains(_parentChunk.chunkData[FlattenXYZ(x, y, z)]);
    }

    private int FlattenXYZ(int x, int y, int z)
    {
        return x + _parentChunk.width * (y + _parentChunk.depth * z);
    }
}