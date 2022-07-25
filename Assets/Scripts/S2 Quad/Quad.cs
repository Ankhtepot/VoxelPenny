using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad
{
    private void Start()
    {
        Mesh mesh = new Mesh();
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

        mesh.name = "ScriptedQuad";

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6];

        Vector2 uv00 = new Vector2(0, 0);
        Vector2 uv10 = new Vector2(1, 0);
        Vector2 uv01 = new Vector2(0, 1);
        Vector2 uv11 = new Vector2(1, 1);

        Vector3 p0 = new Vector3(-0.5f, -0.5f,0.5f);
        Vector3 p1 = new Vector3(0.5f, -0.5f,0.5f);
        Vector3 p2 = new Vector3(0.5f, -0.5f,-0.5f);
        Vector3 p3 = new Vector3(-0.5f, -0.5f,-0.5f);
        Vector3 p4 = new Vector3(-0.5f, 0.5f,0.5f);
        Vector3 p5 = new Vector3(0.5f, 0.5f,0.5f);
        Vector3 p6 = new Vector3(0.5f, 0.5f,-0.5f);
        Vector3 p7 = new Vector3(-0.5f, 0.5f,-0.5f);

        vertices = new[] {p4, p5, p1, p0};
        normals = new[] {Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward};
        uvs = new[] {uv11, uv01, uv00, uv10};
        triangles = new[] {3, 1, 0, 3, 2, 1};

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        mesh.RecalculateBounds();

        mf.mesh = mesh;
    }
}
