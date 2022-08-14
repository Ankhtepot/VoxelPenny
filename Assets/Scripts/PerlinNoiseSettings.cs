using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Perlin Noise Setting", fileName = "PerlinNoiseSetting")]
    public class PerlinNoiseSettings : ScriptableObject
    {
        public int Octaves;
        [Range(0f, 1f)]public float Scale;
        public float HeightOffset;
        public float HeightScale;
        [Range(0f, 1f)]public float Probability;
    }
}