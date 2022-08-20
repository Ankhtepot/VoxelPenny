using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts
{
    public class World : MonoBehaviour
    {
        public GameObject chunkPrefab;
        public GameObject mCamera;
        public GameObject fpc;
        public Slider loadingBar;
        public bool disableGraphersOnStart;

        public WorldLayers worldLayers;

        private static Vector3Int _worldDimensions = new(5, 3, 5);
        private static Vector3Int _chunkDimensions = new(10, 10, 10);

        private HashSet<Vector3Int> chunkChecker = new();
        private HashSet<Vector2Int> chunColumns = new();
        private Dictionary<Vector3Int, Chunk> chunks = new();

        private Vector3Int _lastBuildPosition;

        private void Start()
        {
            foreach (PerlinGrapher grapher in GetComponentsInChildren<PerlinGrapher>())
            {
                grapher.gameObject.SetActive(!disableGraphersOnStart);
            }

            loadingBar.maxValue = _worldDimensions.x * _worldDimensions.z;
            loadingBar.value = 0;
            StartCoroutine(BuildWorld());
        }

        private void BuildChunkColumn(int x, int z)
        {
            for (int y = 0; y < _worldDimensions.y; y++)
            {
                Vector3Int position = new(x, _chunkDimensions.y * y, z);

                if (chunkChecker.Contains(position))
                {
                    chunks[position].MeshRenderer.enabled = true;
                    return;
                }
                
                GameObject chunk = Instantiate(chunkPrefab, transform, true);
                Chunk chunkComponent = chunk.GetComponent<Chunk>();
                chunk.name = $"Chunk_{position.x}_{position.y}_{position.z}";
                chunkComponent.CreateChunk(_chunkDimensions, position, worldLayers);
                chunkChecker.Add(position);
                chunks.Add(position, chunkComponent);
            }
        }

        private IEnumerator BuildWorld()
        {
            mCamera.SetActive(true);
            loadingBar.gameObject.SetActive(true);
            fpc.SetActive(false);

            for (int z = 0; z < _worldDimensions.z; z++)
            {
                for (int x = 0; x < _worldDimensions.x; x++)
                {
                    BuildChunkColumn(x * _chunkDimensions.x, z * _chunkDimensions.z);
                    loadingBar.value += 1;
                    yield return null;
                }
            }

            mCamera.SetActive(false);
            loadingBar.gameObject.SetActive(false);

            float xPos = (_chunkDimensions.x * _worldDimensions.x) / 2f;
            float zPos = (_chunkDimensions.z * _worldDimensions.z) / 2f;
            float yPos = MeshUtils.fBM(xPos, zPos, worldLayers.SurfaceSettings) + 1;

            fpc.transform.position = new Vector3(xPos, yPos, zPos);
            fpc.SetActive(true);
            _lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
        }
    }
}