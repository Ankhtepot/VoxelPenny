using System;

namespace Scripts
{
    [Serializable]
    public class PerlinNoiseSettings
    {
        public int Octaves;
        public float Scale;
        public float HeightOffset;
        public float HeightScale;

        public PerlinNoiseSettings(int octaves, float scale, float heightOffset, float heightScale)
        {
            Octaves = octaves;
            Scale = scale;
            HeightOffset = heightOffset;
            HeightScale = heightScale;
        }
    }
}