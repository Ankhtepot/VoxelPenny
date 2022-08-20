using System;
using System.Collections.Generic;
using UnityEngine;
using static S2_Quad.BlockAtlas;

namespace Scripts
{
    public enum ELayerType
    {
        Equal = 1,
        LessThen = 2,
        BetweenTwo = 3,
        Cave = 4,
    }

    [CreateAssetMenu(menuName = "Scriptable Objects/World Layer", fileName = "WorldLayer")]
    public class WorldLayer : ScriptableObject
    {
        [Range(0f, 1f)] public float probability;
        public ELayerType layerType = ELayerType.LessThen;
        public EBlockType blockType = EBlockType.Dirt;
        public List<PerlinNoiseSettings> layers;

        private void OnValidate()
        {
            List<PerlinNoiseSettings> newLayerList = new();

            if (layers.Count == 0) return;

            if (layerType == ELayerType.BetweenTwo && layers.Count == 1)
            {
                newLayerList = new() {layers[0], null};
            }
            else if (layers.Count == 2 && layerType != ELayerType.BetweenTwo)
            {
                newLayerList = new List<PerlinNoiseSettings> {layers[0]};
            }
            else
            {
                newLayerList = layers;
            }

            layers = newLayerList;
        }
    }
}