using System;
using DefaultNamespace;
using UnityEngine;
using static S2_Quad.BlockAtlas;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private int size = 16;
    [SerializeField] private float spacing = 0.5f;
    [SerializeField] private TileConfiguration grassConfiguration;
    [SerializeField] private TileConfiguration sandConfiguration;
    [SerializeField] private GameObject blockPrefab;

    // private int _currentX;
    // private int _currentY;
    private void Start()
    {
        int currentX = 0;
        int currentY = 0;

        EBlockType[] types = (EBlockType[]) Enum.GetValues(typeof(EBlockType));
        foreach (EBlockType type in types)
        {
            TileConfiguration configuration = type switch
            {
                EBlockType.ConfiguredGrassCube => grassConfiguration,
                EBlockType.ConfiguredSandCube => sandConfiguration,
                _ => null
            };

            SingleBlock newBlock = Instantiate(blockPrefab, new Vector3(currentX + currentX * spacing, currentY + currentY * spacing, 0), Quaternion.identity)
                .GetComponent<SingleBlock>();

            newBlock.BuildBlock(type, EBlockType.Air, configuration);

            currentX += 1;
            
            if (currentX >= size)
            {
                currentX = 0;
                currentY -= 1;
            }
        }
    }
}
