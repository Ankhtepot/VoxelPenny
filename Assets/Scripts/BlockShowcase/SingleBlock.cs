using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static MeshUtils;
using static S2_Quad.BlockAtlas;

namespace DefaultNamespace
{
    public class SingleBlock : MonoBehaviour
    {
        public MeshFilter mf;
        public TextMeshPro label;

        public void BuildBlock(BlockType type, BlockType hType)
        {
            Vector3 offset = Vector3.zero;

            List<Mesh> quads = new()
            {
                new Quad(BlockSide.BOTTOM, offset, type, hType).mesh,
                new Quad(BlockSide.TOP, offset, type, hType).mesh,
                new Quad(BlockSide.LEFT, offset, type, hType).mesh,
                new Quad(BlockSide.RIGHT, offset, type, hType).mesh,
                new Quad(BlockSide.FRONT, offset, type, hType).mesh,
                new Quad(BlockSide.BACK, offset, type, hType).mesh
            };

            Mesh mesh = MergeMeshes(quads.ToArray());
            mesh.name = $"Cube_{offset.x}_{offset.y}_{offset.z}_generated";
            mf.mesh = mesh;

            label.text = Enum.GetName(typeof(EBlockType), type);
        }
    }
}