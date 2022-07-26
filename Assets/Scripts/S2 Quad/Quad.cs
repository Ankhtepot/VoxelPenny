using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using S2_Quad;
using UnityEngine;

public class Quad
{
    public Mesh mesh = new Mesh();
    public Quad(Block.EBlockSide side, Vector3 offset, BlockAtlasTile.EAtlasBlock tile)
    {
        mesh.name = "ScriptedCube";

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6];

        BlockAtlasTile atlasUvs = new BlockAtlasTile(tile);

        Vector2 uv00 = atlasUvs.uv00;//new Vector2(0, 0);
        Vector2 uv10 = atlasUvs.uv01;//new Vector2(1, 0);
        Vector2 uv01 = atlasUvs.uv10;//new Vector2(0, 1);
        Vector2 uv11 = atlasUvs.uv11;//new Vector2(1, 1);

        Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f);
        Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f);

        switch (side)
        {
            case Block.EBlockSide.Bottom:
            {
                vertices = new[] {p0, p1, p2, p3};
                normals = new[] {Vector3.down, Vector3.down, Vector3.down, Vector3.down};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case Block.EBlockSide.Top:
            {
                vertices = new[] {p7, p6, p5, p4};
                normals = new[] {Vector3.up, Vector3.up, Vector3.up, Vector3.up};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case Block.EBlockSide.Left:
            {
                vertices = new[] {p7, p4, p0, p3};
                normals = new[] {Vector3.left, Vector3.left, Vector3.left, Vector3.left};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case Block.EBlockSide.Right:
            {
                vertices = new[] {p5, p6, p2, p1};
                normals = new[] {Vector3.right, Vector3.right, Vector3.right, Vector3.right};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case Block.EBlockSide.Front:
            {
                vertices = new[] {p4, p5, p1, p0};
                normals = new[] {Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case Block.EBlockSide.Back:
            {
                vertices = new[] {p6, p7, p3, p2};
                normals = new[] {Vector3.back, Vector3.back, Vector3.back, Vector3.back};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, null);
        }

        mesh.vertices = vertices.Select(vertex => vertex + offset).ToArray();
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = new[] {3, 1, 0, 3, 2, 1};

        mesh.RecalculateBounds();
    }
}