using System;
using Scripts;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour
{
    public LineRenderer lr;
    public PerlinNoiseSettings perlinNoiseSettings;
    
    public float heightOffset;
    public float heightScale = 2f;
    public float scale = 0.5f;
    public int octaves = 1;

    private void Awake()
    {
        scale = perlinNoiseSettings.Scale;
        octaves = perlinNoiseSettings.Octaves;
        heightOffset = perlinNoiseSettings.HeightOffset;
        heightScale = perlinNoiseSettings.HeightScale;
    }

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 100;
        Graph();
    }

    private void Graph()
    {
        int z = 11;
        Vector3[] positions = new Vector3[lr.positionCount];

        for (int x = 0; x < lr.positionCount; x++)
        {
            float y = MeshUtils.fBM(x, z, perlinNoiseSettings);
            positions[x] = new Vector3(x, y, z);
        }
        
        lr.SetPositions(positions);
    }

    private void OnValidate()
    {
        perlinNoiseSettings.Octaves = octaves;
        perlinNoiseSettings.Scale = scale;
        perlinNoiseSettings.HeightOffset = heightOffset;
        perlinNoiseSettings.HeightScale = heightScale;
        Graph();
    }
}
