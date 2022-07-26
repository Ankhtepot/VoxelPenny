using System;
using System.Collections.Generic;
using System.Linq;
using S2_Quad;
using UnityEngine;
using static MeshUtils;
using static S2_Quad.BlockAtlas;

public class Quad
{
    public readonly Mesh mesh = new();
    public Quad(EBlockSide side, Vector3 offset, EBlockType tile, EBlockType hType)
    {
        mesh.name = "ScriptedCube";

        Vector3[] vertices;
        Vector3[] normals;
        Vector2[] uvs;

        BlockUVs atlasUvs = GetUVs(tile);
        BlockUVs secondaryUvs = GetUVs(hType);

        List<Vector2> suvs = new()
        {
            secondaryUvs.uv11,
            secondaryUvs.uv10,
            secondaryUvs.uv00,
            secondaryUvs.uv01
        };

        Vector2 uv00 = atlasUvs.uv00;
        Vector2 uv10 = atlasUvs.uv01;
        Vector2 uv01 = atlasUvs.uv10;
        Vector2 uv11 = atlasUvs.uv11;

        Vector3 p0 = new(-0.5f, -0.5f, 0.5f);
        Vector3 p1 = new(0.5f, -0.5f, 0.5f);
        Vector3 p2 = new(0.5f, -0.5f, -0.5f);
        Vector3 p3 = new(-0.5f, -0.5f, -0.5f);
        Vector3 p4 = new(-0.5f, 0.5f, 0.5f);
        Vector3 p5 = new(0.5f, 0.5f, 0.5f);
        Vector3 p6 = new(0.5f, 0.5f, -0.5f);
        Vector3 p7 = new(-0.5f, 0.5f, -0.5f);

        switch (side)
        {
            case EBlockSide.Bottom:
            {
                vertices = new[] {p0, p1, p2, p3};
                normals = new[] {Vector3.down, Vector3.down, Vector3.down, Vector3.down};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case EBlockSide.Top:
            {
                vertices = new[] {p7, p6, p5, p4};
                normals = new[] {Vector3.up, Vector3.up, Vector3.up, Vector3.up};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case EBlockSide.Left:
            {
                vertices = new[] {p7, p4, p0, p3};
                normals = new[] {Vector3.left, Vector3.left, Vector3.left, Vector3.left};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case EBlockSide.Right:
            {
                vertices = new[] {p5, p6, p2, p1};
                normals = new[] {Vector3.right, Vector3.right, Vector3.right, Vector3.right};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case EBlockSide.Front:
            {
                vertices = new[] {p4, p5, p1, p0};
                normals = new[] {Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward};
                uvs = new[] {uv11, uv01, uv00, uv10};
            }
                break;
            case EBlockSide.Back:
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
        mesh.SetUVs(1, suvs);

        mesh.RecalculateBounds();
    }
}