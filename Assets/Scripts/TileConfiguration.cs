using S2_Quad;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Tile Configuration", menuName = "Scriptable Objects/Tile Configuration")]
    public class TileConfiguration : ScriptableObject
    {
        public BlockAtlas.EBlockType topTile;
        public BlockAtlas.EBlockType bottomTile;
        public BlockAtlas.EBlockType leftTile;
        public BlockAtlas.EBlockType rightTile;
        public BlockAtlas.EBlockType frontTile;
        public BlockAtlas.EBlockType backTile;
    }
}