using System;
using DefaultNamespace;
using UnityEngine;

namespace Scripts
{
    [Serializable]
    public class WorldLayers
    {
        [SerializeField] private WorldLayer surfaceLayer;
        [SerializeField] private WorldLayer bedrockLayer;
        [SerializeField] private WorldLayer caveLayer;
        [SerializeField] private WorldLayer diamondLayer;
        [SerializeField] private WorldLayer stoneLayer;

        public PerlinNoiseSettings SurfaceSettings => surfaceLayer.layers[0];
        public WorldLayer BedrockLayer => bedrockLayer;
        public PerlinNoiseSettings BedrockSettings => bedrockLayer.layers[0];
        public WorldLayer CaveLayer => caveLayer;
        public CavePNSettings CaveSettings => (CavePNSettings)caveLayer.layers[0];
        public PerlinNoiseSettings DiamondTopSettings => diamondLayer.layers[0];
        public PerlinNoiseSettings DiamondBottomSettings => diamondLayer.layers[1];
        public WorldLayer DiamondLayer => diamondLayer;
        public PerlinNoiseSettings StoneSettings => stoneLayer.layers[0];
        public WorldLayer StoneLayer => stoneLayer;
    }
}