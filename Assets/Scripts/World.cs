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
        public static WorldLayers WorldLayers;

        private static Vector3Int _worldDimensions = new(20, 5, 20);
        private static Vector3Int _extraWorldDimensions = new(10, 5, 10);
        private static Vector3Int _chunkDimensions = new(10, 10, 10);

        private HashSet<Vector3Int> chunkChecker = new();
        private HashSet<Vector2Int> chunkColumns = new();
        private Dictionary<Vector3Int, Chunk> chunks = new();

        private Vector3Int _lastBuildPosition;
        private const int DrawRadius = 4;

        private readonly Queue<IEnumerator> _buildQueue = new();

        private void Awake()
        {
            WorldLayers = worldLayers;
        }

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

        private IEnumerator BuildCoordinator()
        {
            while (true)
            {
                while (_buildQueue.Count > 0)
                {
                    yield return StartCoroutine(_buildQueue.Dequeue());
                }

                yield return null;
            }
        }

        private void BuildChunkColumn(int x, int z, bool meshEnabled = true)
        {
            for (int y = 0; y < _worldDimensions.y; y++)
            {
                Vector3Int position = new(x, _chunkDimensions.y * y, z);

                if (!chunkChecker.Contains(position))
                {
                    GameObject chunk = Instantiate(chunkPrefab, transform, true);
                    Chunk chunkComponent = chunk.GetComponent<Chunk>();
                    chunk.name = $"Chunk_{position.x}_{position.y}_{position.z}";
                    chunkComponent.CreateChunk(_chunkDimensions, position);
                    chunkChecker.Add(position);
                    chunks.Add(position, chunkComponent);
                }
                
                chunks[position].MeshRenderer.enabled = meshEnabled;
            }

            chunkColumns.Add(new Vector2Int(x, z));
        }

        private IEnumerator BuildExtraWorld()
        {
            int zEnd = _worldDimensions.z + _extraWorldDimensions.z;
            int zStart = _worldDimensions.z;
            
            int xEnd = _worldDimensions.x + _extraWorldDimensions.x;
            int xStart = _worldDimensions.x;

            for (int z = zStart; z < zEnd; z++)
            {
                for (int x = 0; x < xEnd; x++)
                {
                    BuildChunkColumn(x * _chunkDimensions.x, z * _chunkDimensions.z, false);
                    yield return null;
                }
            }
            
            for (int z = 0; z < zEnd; z++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    BuildChunkColumn(x * _chunkDimensions.x, z * _chunkDimensions.z, false);
                    yield return null;
                }
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
            StartCoroutine(BuildCoordinator());
            StartCoroutine(UpdateWorld());
            StartCoroutine(BuildExtraWorld());
        }

        private readonly WaitForSeconds _wfs = new(0.5f);
        private IEnumerator UpdateWorld()
        {
            while (true)
            {
                if ((_lastBuildPosition - fpc.transform.position).magnitude > _chunkDimensions.x)
                {
                    Vector3 fpcPosition = fpc.transform.position;
                    _lastBuildPosition = Vector3Int.CeilToInt(fpcPosition);
                    int posX = (int) (fpcPosition.x / _chunkDimensions.x) * _chunkDimensions.x;
                    int posZ = (int) (fpcPosition.z / _chunkDimensions.z) * _chunkDimensions.z;
                    _buildQueue.Enqueue(BuildRecursiveWorld(posX, posZ, DrawRadius));
                    _buildQueue.Enqueue(HideColumns(posX, posZ));
                }

                yield return _wfs;
            }
        }

        private void HideChunkColumn(int x, int z)
        {
            for (int y = 0; y < _worldDimensions.y; y++)
            {
                Vector3Int pos = new(x, y * _chunkDimensions.y, z);
                if (chunkChecker.Contains(pos))
                {
                    chunks[pos].MeshRenderer.enabled = false;
                }
            }
        }

        private IEnumerator HideColumns(int x, int z)
        {
            Vector2Int fpcPos = new(x, z);
            foreach (Vector2Int chunkColumn in chunkColumns)
            {
                if ((chunkColumn - fpcPos).magnitude >= DrawRadius * _chunkDimensions.x)
                {
                    HideChunkColumn(chunkColumn.x, chunkColumn.y);
                }
            }

            yield return null;
        }

        private IEnumerator BuildRecursiveWorld(int x, int z, int rad)
        {
            int nextRad = rad - 1;
            if (rad <= 0) yield break;
            
            BuildChunkColumn(x, z + _chunkDimensions.z);
            _buildQueue.Enqueue(BuildRecursiveWorld(x, z + _chunkDimensions.z, nextRad));
            yield return null;
            
            BuildChunkColumn(x, z - _chunkDimensions.z);
            _buildQueue.Enqueue(BuildRecursiveWorld(x, z - _chunkDimensions.z, nextRad));
            yield return null;
            
            BuildChunkColumn(x + _chunkDimensions.x, z );
            _buildQueue.Enqueue(BuildRecursiveWorld(x + _chunkDimensions.x, z, nextRad));
            yield return null;
            
            BuildChunkColumn(x - _chunkDimensions.x, z );
            _buildQueue.Enqueue(BuildRecursiveWorld(x - _chunkDimensions.x, z, nextRad));
            yield return null;
        }
    }
}