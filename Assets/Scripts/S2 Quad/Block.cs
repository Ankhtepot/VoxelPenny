using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using S2_Quad;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector3 position;
    public Material atlas;
    public BlockAtlasTile.EAtlasBlock tile;

    [Header("Override Tiles")] 
    public TileConfiguration tileConfiguration;

    private BlockAtlasTile.EAtlasBlock _originTile;
    
    [Serializable]
    public enum EBlockSide
    {
        Bottom,
        Top,
        Left,
        Right,
        Front,
        Back,
    }
    
    private void Start()
    {
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

        bool isTileConfigured = tileConfiguration != null;
        
        Mesh[] q =
        {
            new Quad(EBlockSide.Bottom, Vector3.zero, isTileConfigured ? tileConfiguration.bottomTile : tile).mesh,
            new Quad(EBlockSide.Top, Vector3.zero, isTileConfigured ? tileConfiguration.topTile : tile).mesh,
            new Quad(EBlockSide.Left, Vector3.zero, isTileConfigured ? tileConfiguration.leftTile : tile).mesh,
            new Quad(EBlockSide.Right, Vector3.zero, isTileConfigured ? tileConfiguration.rightTile : tile).mesh,
            new Quad(EBlockSide.Front, Vector3.zero, isTileConfigured ? tileConfiguration.frontTile : tile).mesh,
            new Quad(EBlockSide.Back, Vector3.zero, isTileConfigured ? tileConfiguration.backTile : tile).mesh,
        };
        
        transform.position = position;
        
        mf.mesh = MeshUtils.MergeMeshes(q);
        mf.mesh.name = $"Cube_{position.x}_{position.y}_{position.z}_generated";
        mr.material = atlas;
    }
}
