using System;
using System.Collections;
using System.Collections.Generic;
using S2_Quad;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector3 position;
    public Material atlas;
    public BlockAtlasTile.EAtlasBlock tile;
    
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

        transform.position = position;

        Mesh[] q =
        {
            new Quad(EBlockSide.Bottom, position, tile).mesh,
            new Quad(EBlockSide.Top, position, tile).mesh,
            new Quad(EBlockSide.Left, position, tile).mesh,
            new Quad(EBlockSide.Right, position, tile).mesh,
            new Quad(EBlockSide.Front, position, tile).mesh,
            new Quad(EBlockSide.Back, position, tile).mesh,
        };
        
        mf.mesh = MeshUtils.MergeMeshes(q);
        mf.mesh.name = $"Cube_{position.x}_{position.y}_{position.z}_generated";
        mr.material = atlas;
    }
}
