using S2_Quad;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Tile Configuration", menuName = "ScriptableObjects/TileConfiguration")]
    public class TileConfiguration : ScriptableObject
    {
        public BlockAtlasTile.EAtlasBlock topTile;
        public BlockAtlasTile.EAtlasBlock bottomTile;
        public BlockAtlasTile.EAtlasBlock leftTile;
        public BlockAtlasTile.EAtlasBlock rightTile;
        public BlockAtlasTile.EAtlasBlock frontTile;
        public BlockAtlasTile.EAtlasBlock backTile;
    }
}