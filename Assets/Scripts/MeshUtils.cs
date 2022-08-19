using System.Collections.Generic;
using System.Linq;
using Scripts;
using UnityEngine;
using VertexData = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector2>;

public static class MeshUtils
{
    public enum EBlockSide
    {
        Bottom,
        Top,
        Left,
        Right,
        Front,
        Back,
    }

    public static Mesh MergeMeshes(IEnumerable<Mesh> inputMeshes)
    {
        Mesh mesh = new Mesh();

        Dictionary<VertexData, int> pointsOrder = new Dictionary<VertexData, int>();
        HashSet<VertexData> pointsHash = new HashSet<VertexData>();
        List<int> tris = new List<int>();

        int pIndex = 0;

        List<Mesh> meshes = inputMeshes.ToList();

        for (int i = 0; i < meshes.Count; i++)
        {
            if (meshes.ElementAt(i) == null) continue;

            for (int j = 0; j < meshes.ElementAt(i).vertices.Length; j++)
            {
                Vector3 v = meshes.ElementAt(i).vertices[j];
                Vector3 n = meshes.ElementAt(i).normals[j];
                Vector2 u = meshes.ElementAt(i).uv[j];
                VertexData p = new VertexData(v, n, u);

                if (!pointsHash.Contains(p))
                {
                    pointsOrder.Add(p, pIndex);
                    pointsHash.Add(p);
                    pIndex++;
                }
            }

            for (int t = 0; t < meshes.ElementAt(i).triangles.Length; t++)
            {
                int triPoint = meshes.ElementAt(i).triangles[t];
                Vector3 v = meshes.ElementAt(i).vertices[triPoint];
                Vector3 n = meshes.ElementAt(i).normals[triPoint];
                Vector2 u = meshes.ElementAt(i).uv[triPoint];
                VertexData p = new VertexData(v, n, u);

                pointsOrder.TryGetValue(p, out int index);
                tris.Add(index);
            }

            meshes[i] = null;
        }

        ExtractArrays(pointsOrder, mesh);
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static void ExtractArrays(Dictionary<VertexData, int> list, Mesh mesh)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        foreach (VertexData v in list.Keys)
        {
            verts.Add(v.Item1);
            norms.Add(v.Item2);
            uvs.Add(v.Item3);
        }

        mesh.vertices = verts.ToArray();
        mesh.normals = norms.ToArray();
        mesh.uv = uvs.ToArray();
    }

    public static float fBM(float x, float z, PerlinNoiseSettings pns) // finite Brownian Motion
    {
        return fBM(x, z, pns.Octaves, pns.Scale, pns.HeightScale, pns.HeightOffset);
    }

    public static float
        fBM(float x, float z, int octaves, float scale, float heightScale, float heightOffset) // finite Brownian Motion
    {
        float total = 0;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency) * heightScale;
            frequency *= 2;
        }

        return total + heightOffset;
    }

    public static float
        fBM3D(float x, float y, float z, int octaves, float scale, float heightScale,
            float heightOffset) // finite Brownian Motion 3D
    {
        float XY = fBM(x, y, octaves, scale, heightScale, heightOffset);
        float XZ = fBM(x, z, octaves, scale, heightScale, heightOffset);
        float YZ = fBM(y, z, octaves, scale, heightScale, heightOffset);
        float YX = fBM(y, x, octaves, scale, heightScale, heightOffset);
        float ZX = fBM(z, x, octaves, scale, heightScale, heightOffset);
        float ZY = fBM(z, y, octaves, scale, heightScale, heightOffset);

        return (XY + YZ + XZ + YX + ZY + ZX) / 6.0f;
    }
}