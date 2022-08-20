using System;
using DefaultNamespace;
using UnityEngine;

[ExecuteInEditMode]
public class Perlin3DGrapher : MonoBehaviour
{
    public Vector3Int dimensions = new(10, 10, 10);
    public float heightScale = 2f;
    [Range(0f, 1f)] public float scale = 0.5f;
    public int octaves = 1;
    public float heightOffset;
    [Range(0.0f, 10.0f)] public float drawCutoff;
    [SerializeField] private CavePNSettings settings;

    private int _width;
    private int _height;
    private int _depth;

    private void Awake()
    {
        scale = settings.Scale;
        octaves = settings.Octaves;
        heightOffset = settings.HeightOffset;
        heightScale = settings.HeightScale;
        drawCutoff = settings.DrawCutoff;
        SetWHD();
    }

    private void CreateCubes()
    {
        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "perlin_cube";
                    cube.transform.parent = transform;
                    cube.transform.position = new Vector3(x, y, z);
                }
            }
        }
    }

    private void Graph()
    {
        MeshRenderer[] cubes = GetComponentsInChildren<MeshRenderer>();
        if (cubes.Length == 0)
        {
            CreateCubes();
        }

        if (cubes.Length < dimensions.x * dimensions.y * dimensions.z) return;

        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    try
                    {
                        float p3d = MeshUtils.fBM3D(x, y, z, settings.Octaves, settings.Scale, settings.HeightScale, settings.HeightOffset);
                        cubes[FlattenXYZ(x, y, z)].enabled = p3d >= settings.DrawCutoff;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }
    }

    private void OnValidate()
    {
        settings.Octaves = octaves;
        settings.Scale = scale;
        settings.HeightOffset = heightOffset;
        settings.HeightScale = heightScale;
        dimensions = dimensions;
        settings.DrawCutoff = drawCutoff;
        Graph();
    }

    private int FlattenXYZ(int x, int y, int z)
    {
        if (Math.Abs(dimensions.x - _width) > Single.Epsilon 
            || Math.Abs(dimensions.y - _height) > Single.Epsilon 
            || Math.Abs(dimensions.z - _depth) > Single.Epsilon)
        {
            SetWHD();
        }

        return x + _width * (y + _depth * z);
    }

    private void SetWHD()
    {
        _width = dimensions.x;
        _height = dimensions.y;
        _depth = dimensions.z;
    }
}