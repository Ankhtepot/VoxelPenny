using System;
using System.Collections.Generic;
using System.Linq;
using Scripts;
using UnityEngine;

namespace DefaultNamespace
{
    [Serializable]
    public class WorldLayers
    {
        [SerializeField] private WorldLayer surfaceLayer;
        [SerializeField] private WorldLayer bedrockLayer;
        [SerializeField] private List<WorldLayer> layers;

        public PerlinNoiseSettings SurfaceSettings => surfaceLayer.layers[0];
        public WorldLayer BedrockLayer => bedrockLayer;
        public PerlinNoiseSettings BedrockSettings => bedrockLayer.layers[0];
        public List<WorldLayer> Layers => (new List<WorldLayer> {surfaceLayer})
            .Concat(layers)
            .Concat(new List<WorldLayer>{bedrockLayer})
            .ToList();
    }
}