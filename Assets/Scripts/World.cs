using System;
using System.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    public class World : MonoBehaviour
    {
        public static Vector3 worldDimensions = new(5, 3, 5);
        public static Vector3 chunkDimensions = new(10, 10, 10);
        public GameObject chunkPrefab;

        private void Start()
        {
            StartCoroutine(BuildWorld());
        }

        private IEnumerator BuildWorld()
        {
            for (int z = 0; z < worldDimensions.z; z++)
            {
                for (int y = 0; y < worldDimensions.y; y++)
                {
                    for (int x = 0; x < worldDimensions.x; x++)
                    {
                        GameObject chunk = Instantiate(chunkPrefab, transform, true);
                        Vector3 position = new(chunkDimensions.x * x, chunkDimensions.y * y, chunkDimensions.z * z);
                        chunk.GetComponent<Chunk>().CreateChunk(chunkDimensions, position);
                        yield return null;
                    }
                }
            }
        }
    }
}