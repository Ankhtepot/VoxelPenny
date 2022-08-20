using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class World : MonoBehaviour
    {
        public GameObject chunkPrefab;
        public GameObject mCamera;
        public GameObject fpc;
        public Slider loadingBar;

        [Header("Layers")]
        public WorldLayers worldLayers;
        
        private static Vector3 worldDimensions = new(5, 3, 5);
        private static Vector3 chunkDimensions = new(10, 10, 10);

        private void Start()
        {
            loadingBar.maxValue = worldDimensions.x * worldDimensions.y * worldDimensions.z;
            loadingBar.value = 0;
            StartCoroutine(BuildWorld());
        }

        private IEnumerator BuildWorld()
        {
            mCamera.SetActive(true);
            loadingBar.gameObject.SetActive(true);
            fpc.SetActive(false);
            
            for (int z = 0; z < worldDimensions.z; z++)
            {
                for (int y = 0; y < worldDimensions.y; y++)
                {
                    for (int x = 0; x < worldDimensions.x; x++)
                    {
                        GameObject chunk = Instantiate(chunkPrefab, transform, true);
                        Vector3 position = new(chunkDimensions.x * x, chunkDimensions.y * y, chunkDimensions.z * z);
                        Chunk chunkComponent = chunk.GetComponent<Chunk>();
                        chunkComponent.CreateChunk(chunkDimensions, position, worldLayers);
                        loadingBar.value += 1;
                        yield return null;
                    }
                }
            }
            
            mCamera.SetActive(false);
            loadingBar.gameObject.SetActive(false);

            float xPos = (chunkDimensions.x * worldDimensions.x) / 2f;
            float zPos = (chunkDimensions.z * worldDimensions.z) / 2f;
            float yPos = MeshUtils.fBM(xPos, zPos, worldLayers.SurfaceSettings) + 1;

            fpc.transform.position = new Vector3(xPos, yPos, zPos);
            fpc.SetActive(true);
        }
    }
}