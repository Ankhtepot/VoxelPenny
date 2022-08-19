using System;
using UnityEngine;

[ExecuteInEditMode]
public class Perlin3DGrapher : MonoBehaviour
{
    private readonly Vector3 _dimensions = new(10, 10, 10);
    public float heightScale = 2f;
    [Range(0f, 1f)] public float scale = 0.5f;
    public int octaves = 1;
    public float heightOffset;
    [Range(0.0f, 10.0f)] public float drawCutoff;

    private int _width;
    private int _height;
    private int _depth;

    private void Awake()
    {
        SetWHD();
    }

    private void CreateCubes()
    {
        for (int z = 0; z < _dimensions.z; z++)
        {
            for (int y = 0; y < _dimensions.y; y++)
            {
                for (int x = 0; x < _dimensions.x; x++)
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

        if (cubes.Length < +_dimensions.x * _dimensions.y * _dimensions.z) return;

        for (int z = 0; z < _dimensions.z; z++)
        {
            for (int y = 0; y < _dimensions.y; y++)
            {
                for (int x = 0; x < _dimensions.x; x++)
                {
                    try
                    {
                        float p3d = MeshUtils.fBM3D(x, y, z, octaves, scale, heightScale, heightOffset);
                        cubes[FlattenXYZ(x, y, z)].enabled = p3d >= drawCutoff;
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
        Graph();
    }

    private int FlattenXYZ(int x, int y, int z)
    {
        if (Math.Abs(_dimensions.x - _width) > Single.Epsilon 
            || Math.Abs(_dimensions.y - _height) > Single.Epsilon 
            || Math.Abs(_dimensions.z - _depth) > Single.Epsilon)
        {
            SetWHD();
        }

        return x + _width * (y + _depth * z);
    }

    private void SetWHD()
    {
        _width = (int) _dimensions.x;
        _height = (int) _dimensions.y;
        _depth = (int) _dimensions.z;
    }
}