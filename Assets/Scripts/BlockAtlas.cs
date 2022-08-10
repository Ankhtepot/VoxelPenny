using System;
using System.Collections.Generic;
using UnityEngine;

namespace S2_Quad
{
    public class BlockUVs
    {
        public Vector2 uv00;
        public Vector2 uv01;
        public Vector2 uv10;
        public Vector2 uv11;
    }

    public static class BlockAtlas
    {
        private const float SizeInTiles = 16f;
        private const float Step = 1 / SizeInTiles;
        
        private static readonly BlockUVs BlockUVs;

        private static readonly Dictionary<EBlockType, Vector2Int> Map;

        [Serializable]
        public enum EBlockType
        {
            WallSmallStones,
            Dirt,
            GreenGrassTop,
            GreenGrassSide,
            Water,
            Sand,
            Air,
        }

        static BlockAtlas()
        {
            Map = BuildMap();
            BlockUVs = new BlockUVs();
        }

        public static BlockUVs GetUVs(EBlockType desiredBlockType)
        {
            Vector2Int location = Map[desiredBlockType];

            BlockUVs.uv00.x = location.x * Step;
            BlockUVs.uv00.y = location.y * Step;
            BlockUVs.uv01.x = location.x * Step + Step;
            BlockUVs.uv01.y = location.y * Step;
            BlockUVs.uv10.x = location.x * Step;
            BlockUVs.uv10.y = location.y * Step + Step;
            BlockUVs.uv11.x = location.x * Step + Step;
            BlockUVs.uv11.y = location.y * Step + Step;

            return BlockUVs;
        }

        private static Dictionary<EBlockType, Vector2Int> BuildMap() =>
            new Dictionary<EBlockType, Vector2Int>
            {
                {EBlockType.WallSmallStones, new Vector2Int(0, 15)},
                {EBlockType.GreenGrassTop, new Vector2Int(2, 6)},
                {EBlockType.Sand, new Vector2Int(2, 14)},
                {EBlockType.Dirt, new Vector2Int(2, 15)},
                {EBlockType.GreenGrassSide, new Vector2Int(3, 15)},
                {EBlockType.Water, new Vector2Int(15, 3)},
                {EBlockType.Air, new Vector2Int(12, 0)}
            };
    }
}