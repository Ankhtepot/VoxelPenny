using System;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour
{
    public LineRenderer lr;
    public float heightOffset;
    public float heightScale = 2f;
    public float scale = 0.5f;
    public int octaves = 1;
    
    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 100;
        Graph();
    }

    float fBM(float x, float z) // fractal Brownian Motion
    {
        float total = 0;
        float frequency = 1;
        
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency) * heightScale;
            frequency *= 2;
        }

        return total;
    }

    private void Graph()
    {
        int z = 11;
        Vector3[] positions = new Vector3[lr.positionCount];

        for (int x = 0; x < lr.positionCount; x++)
        {
            float y = fBM(x, z) + heightOffset;
            positions[x] = new Vector3(x, y, z);
        }
        
        lr.SetPositions(positions);
    }

    private void OnValidate()
    {
        Graph();
    }
}
