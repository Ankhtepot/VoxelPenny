using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using S2_Quad;
using UnityEngine;
using static MeshUtils;

public class Block
{
    public Vector3 position;
    public Mesh mesh;

    private BlockAtlas.EBlockType _originTile;

    public Block(BlockAtlas.EBlockType tile, Vector3 offset, TileConfiguration tileConfiguration = null)
    {
        bool isTileConfigured = tileConfiguration != null;
        
        Mesh[] q =
        {
            new Quad(EBlockSide.Bottom, offset, isTileConfigured ? tileConfiguration.bottomTile : tile).mesh,
            new Quad(EBlockSide.Top, offset, isTileConfigured ? tileConfiguration.topTile : tile).mesh,
            new Quad(EBlockSide.Left, offset, isTileConfigured ? tileConfiguration.leftTile : tile).mesh,
            new Quad(EBlockSide.Right, offset, isTileConfigured ? tileConfiguration.rightTile : tile).mesh,
            new Quad(EBlockSide.Front, offset, isTileConfigured ? tileConfiguration.frontTile : tile).mesh,
            new Quad(EBlockSide.Back, offset, isTileConfigured ? tileConfiguration.backTile : tile).mesh,
        };
        
        mesh = MergeMeshes(q);
        mesh.name = $"Cube_{position.x}_{position.y}_{position.z}_generated";
    }
}
