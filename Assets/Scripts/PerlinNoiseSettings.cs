using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Perlin Noise Setting", fileName = "PerlinNoiseSetting")]
    public class PerlinNoiseSettings : ScriptableObject
    {
        public int Octaves;
        public float Scale;
        public float HeightOffset;
        public float HeightScale;
    }
}