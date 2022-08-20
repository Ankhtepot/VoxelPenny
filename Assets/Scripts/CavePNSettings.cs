using Scripts;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "CavePerlinNoiseSettings", menuName = "Scriptable Objects/Cave Perlin Noise Setting")]
    public class CavePNSettings : PerlinNoiseSettings
    {
        [Range(0f, 10f)]public float DrawCutoff;
    }
}