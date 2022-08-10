using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class World : MonoBehaviour
    {
        public static Vector3 worldDimensions = new Vector3(3, 3, 3);
        public static Vector3 chunkDimensions = new Vector3(10, 10, 10);
        public GameObject chunkPrefab;

        private void Start()
        {
            for (int z = 0; z < worldDimensions.z; z++)
            {
                for (int y = 0; y < worldDimensions.y; y++)
                {
                    for (int x = 0; x < worldDimensions.x; x++)
                    {
                        GameObject chunk = Instantiate(chunkPrefab);
                        Vector3 position = new Vector3(chunkDimensions.x * x, chunkDimensions.y * y, chunkDimensions.z * z);
                        chunk.GetComponent<Chunk>().CreateChunk(chunkDimensions, position);
                    }
                }
            }
        }
    }
}