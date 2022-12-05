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
        
        private static readonly Dictionary<EBlockType, Vector2Int> Map;

        [Serializable]
        public enum EBlockType
        {
            ConfiguredGrassCube = 0,
            ConfiguredSandCube = 1,
            WallSmallStones = 11,
            Dirt = 12,
            GreenGrassTop = 13,
            GreenGrassSide = 14,
            Water = 15,
            Sand = 16,
            Air = 17,
            Gold = 18,
            Bedrock = 19,
            Diamond = 20,
            NoCrack = 21,
            Crack1 = 22,
            Crack2 = 23,
            Crack3 = 24,
            Crack4 = 25,
        }

        public static int GetHealthByType(EBlockType blockType) => blockType switch
        {
            EBlockType.ConfiguredGrassCube => 2,
            EBlockType.ConfiguredSandCube => 3,
            EBlockType.WallSmallStones => 4,
            EBlockType.Dirt => 2,
            EBlockType.GreenGrassTop => 2,
            EBlockType.GreenGrassSide => 2,
            EBlockType.Water => 1,
            EBlockType.Sand => 3,
            EBlockType.Air => -1,
            EBlockType.Gold => 4,
            EBlockType.Bedrock => -1,
            EBlockType.Diamond => 4,
            EBlockType.NoCrack => 1,
            EBlockType.Crack1 => -1,
            EBlockType.Crack2 => -1,
            EBlockType.Crack3 => -1,
            EBlockType.Crack4 => -1,
            _ => throw new ArgumentOutOfRangeException(nameof(blockType), blockType, $"Add {blockType} to health tab!")
        };
        
        private static Dictionary<EBlockType, Vector2Int> BuildMap() =>
            new()
            {
                {EBlockType.WallSmallStones, new Vector2Int(0, 15)},
                {EBlockType.GreenGrassTop, new Vector2Int(2, 6)},
                {EBlockType.Sand, new Vector2Int(2, 14)},
                {EBlockType.Dirt, new Vector2Int(2, 15)},
                {EBlockType.GreenGrassSide, new Vector2Int(3, 15)},
                {EBlockType.Water, new Vector2Int(15, 3)},
                {EBlockType.Air, new Vector2Int(12, 0)},
                {EBlockType.Gold, new Vector2Int(0, 13)},
                {EBlockType.Bedrock, new Vector2Int(5, 13)},
                {EBlockType.Diamond, new Vector2Int(2, 12)},
                {EBlockType.NoCrack, new Vector2Int(12, 0)}, // same as Air
                {EBlockType.Crack1, new Vector2Int(0, 0)},
                {EBlockType.Crack2, new Vector2Int(1, 0)},
                {EBlockType.Crack3, new Vector2Int(2, 0)},
                {EBlockType.Crack4, new Vector2Int(3, 0)},
            };

        static BlockAtlas()
        {
            Map = BuildMap();
        }

        public static BlockUVs GetUVs(EBlockType desiredBlockType)
        {
            Vector2Int location = Map[desiredBlockType];
            BlockUVs blockUvs = new();
            
            blockUvs.uv00.x = location.x * Step;
            blockUvs.uv00.y = location.y * Step;
            blockUvs.uv01.x = location.x * Step + Step;
            blockUvs.uv01.y = location.y * Step;
            blockUvs.uv10.x = location.x * Step;
            blockUvs.uv10.y = location.y * Step + Step;
            blockUvs.uv11.x = location.x * Step + Step;
            blockUvs.uv11.y = location.y * Step + Step;

            return blockUvs;
        }
    }
}