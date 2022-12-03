using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static S2_Quad.BlockAtlas;

namespace DefaultNamespace
{
    public class SingleBlock : MonoBehaviour
    {
        public MeshFilter mf;
        public TextMeshPro label;

        public void BuildBlock(EBlockType type, EBlockType hType, TileConfiguration tileConfiguration)
        {
            Vector3 offset = Vector3.zero;

            List<Mesh> quads = new();
            bool isTileConfigured = tileConfiguration;
                quads.Add(new Quad(MeshUtils.EBlockSide.Bottom, offset,
                    isTileConfigured ? tileConfiguration!.bottomTile : type, hType).mesh);

                quads.Add(new Quad(MeshUtils.EBlockSide.Top, offset,
                    isTileConfigured ? tileConfiguration!.topTile : type, hType).mesh);

                quads.Add(new Quad(MeshUtils.EBlockSide.Left, offset,
                    isTileConfigured ? tileConfiguration!.leftTile : type, hType).mesh);
                
                quads.Add(new Quad(MeshUtils.EBlockSide.Right, offset,
                    isTileConfigured ? tileConfiguration!.rightTile : type, hType).mesh);

                quads.Add(new Quad(MeshUtils.EBlockSide.Front, offset,
                    isTileConfigured ? tileConfiguration!.frontTile : type, hType).mesh);

                quads.Add(new Quad(MeshUtils.EBlockSide.Back, offset,
                    isTileConfigured ? tileConfiguration!.backTile : type, hType).mesh);

            Mesh mesh  = MeshUtils.MergeMeshes(quads);
            mesh.name = $"Cube_{offset.x}_{offset.y}_{offset.z}_generated";
            mf.mesh = mesh;

            label.text = Enum.GetName(typeof(EBlockType), type);
        }
    }
}