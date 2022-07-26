using System;
using System.Collections.Generic;
using UnityEngine;

namespace S2_Quad
{
    public class BlockAtlasTile
    {
        private const float sizeInTiles = 16f;
        private const float sizeInPixels = 1280f;
        private readonly float step = 1 / sizeInTiles;
        
        public Vector2 uv00;
        public Vector2 uv01;
        public Vector2 uv10;
        public Vector2 uv11;

        private static Dictionary<EAtlasBlock, Vector2Int> map;

        [Serializable]
        public enum EAtlasBlock
        {
            WallSmallStones,
            Dirt,
            GreenGrass,
        }

        public BlockAtlasTile(EAtlasBlock desiredBlock)
        {
            if (map == null)
            {
                map = BuildMap();
                uv00 = new Vector2(0, 0);
                uv01 = new Vector2(0, 0);
                uv10 = new Vector2(0, 0);
                uv11 = new Vector2(0, 0);
            }

            Vector2Int location = map[desiredBlock];

            uv00.x = location.x * step;
            uv00.y = location.y * step;
            uv01.x = location.x * step + step;
            uv01.y = location.y * step;
            uv10.x = location.x * step;
            uv10.y = location.y * step + step;
            uv11.x = location.x * step + step;
            uv11.y = location.y * step + step;
        }

        private Dictionary<EAtlasBlock, Vector2Int> BuildMap()
        {
            return new Dictionary<EAtlasBlock, Vector2Int>
            {
                {EAtlasBlock.WallSmallStones, new Vector2Int(0, 16)},
                {EAtlasBlock.Dirt, new Vector2Int(0, 2)},
                {EAtlasBlock.GreenGrass, new Vector2Int(2, 6)}
            };

        }
    }
    
    
}