using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
using static S2_Quad.BlockAtlas;

namespace Scripts
{
    public class World : MonoBehaviour
    {
        public GameObject chunkPrefab;
        public GameObject mCamera;
        public GameObject fpc;
        public Slider loadingBar;
        public bool disableGraphersOnStart;
        public bool loadFromFile = false;

        public WorldLayers worldLayers;
        public static WorldLayers WorldLayers;

        public static Vector3Int WorldDimensions = new(5, 5, 5);
        public static Vector3Int ChunkDimensions = new(10, 10, 10);
        private static Vector3Int _extraWorldDimensions = new(5, 5, 5);

        public HashSet<Vector3Int> ChunkChecker = new();
        public HashSet<Vector2Int> ChunkColumns = new();
        public Dictionary<Vector3Int, Chunk> Chunks = new();

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

            loadingBar.maxValue = WorldDimensions.x * WorldDimensions.z;
            loadingBar.value = 0;

            if (loadFromFile)
            {
                StartCoroutine(LoadWorldFromFile());
            }
            
            StartCoroutine(BuildWorld());
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 10))
                {
                    Vector3 hitBlock = Input.GetMouseButtonDown(0) 
                        ? hit.point - hit.normal / 2f 
                        : hit.point + hit.normal / 2f;

                    Chunk thisChunk = hit.collider.gameObject.GetComponent<Chunk>();

                    int bx = (int) (Mathf.Round(hitBlock.x) - thisChunk.location.x);
                    int by = (int) (Mathf.Round(hitBlock.y) - thisChunk.location.y);
                    int bz = (int) (Mathf.Round(hitBlock.z) - thisChunk.location.z);

                    Vector3Int neighbour;

                    if (bx == ChunkDimensions.x)
                    {
                        neighbour = new Vector3Int(thisChunk.location.x + ChunkDimensions.x,
                            thisChunk.location.y, thisChunk.location.z);
                        thisChunk = Chunks[neighbour];
                        bx = 0;
                    } 
                    else if (bx == -1)
                    {
                        neighbour = new Vector3Int(thisChunk.location.x - ChunkDimensions.x,
                            thisChunk.location.y, thisChunk.location.z);
                        thisChunk = Chunks[neighbour];
                        bx = ChunkDimensions.x - 1;
                    }
                    else if (by == ChunkDimensions.y)
                    {
                        neighbour = new Vector3Int(thisChunk.location.x,
                            thisChunk.location.y + ChunkDimensions.y, thisChunk.location.z);
                        thisChunk = Chunks[neighbour];
                        by = 0;
                    } 
                    else if (by == -1)
                    {
                        neighbour = new Vector3Int(thisChunk.location.x,
                            thisChunk.location.y - ChunkDimensions.y, thisChunk.location.z);
                        thisChunk = Chunks[neighbour];
                        by = ChunkDimensions.y - 1;
                    }
                    else if (bz == ChunkDimensions.z)
                    {
                        neighbour = new Vector3Int(thisChunk.location.x,
                            thisChunk.location.y, thisChunk.location.z + ChunkDimensions.z);
                        thisChunk = Chunks[neighbour];
                        bz = 0;
                    } 
                    else if (bz == -1)
                    {
                        neighbour = new Vector3Int(thisChunk.location.x,
                            thisChunk.location.y, thisChunk.location.z - ChunkDimensions.z);
                        thisChunk = Chunks[neighbour];
                        bz = ChunkDimensions.z - 1;
                    }
                    
                    int i = bx + ChunkDimensions.x * (by + ChunkDimensions.z * bz);

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (GetHealthByType(thisChunk.chunkData[i]) == -1) return;
                        
                        if (thisChunk.healthData[i] == EBlockType.NoCrack)
                        {
                            StartCoroutine(HealBlock(thisChunk, i));
                        }
                        
                        thisChunk.healthData[i]++;

                        if (thisChunk.healthData[i] == EBlockType.NoCrack + GetHealthByType(thisChunk.chunkData[i]))
                        {
                            thisChunk.chunkData[i] = EBlockType.Air;
                        }
                    }
                    else
                    {
                        thisChunk.chunkData[i] = _buildType;
                        thisChunk.healthData[i] = EBlockType.NoCrack;
                    }

                    RedrawChunk(thisChunk);
                }
            }
        }

        public void SaveWorld()
        {
            FileSaver.Save(this);
        }

        private IEnumerator LoadWorldFromFile()
        {
            WorldData wd = FileSaver.Load();

            if (wd == null)
            {
                StartCoroutine(BuildWorld());
                yield break;
            }
            
            ChunkChecker.Clear();
            for (int i = 0; i < wd.chunkCheckerValue.Length; i+=3)
            {
                ChunkChecker.Add(new(
                    wd.chunkCheckerValue[i],
                    wd.chunkCheckerValue[i+1],
                    wd.chunkCheckerValue[i+2]
                ));
            }
            
            ChunkColumns.Clear();
            for (int i = 0; i < wd.chunkColumnValues.Length; i+=2)
            {
                ChunkColumns.Add(new(
                    wd.chunkColumnValues[i],
                    wd.chunkColumnValues[i+1]
                ));
            }

            int index = 0;
            foreach (Vector3Int chunkPos in ChunkChecker)
            {
                GameObject chunk = Instantiate(chunkPrefab);
                chunk.name = $"Chunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}";
                Chunk c = chunk.GetComponent<Chunk>();
                int blockCount = ChunkDimensions.x * ChunkDimensions.y * ChunkDimensions.z;
                c.chunkData = new EBlockType[blockCount];
                c.healthData = new EBlockType[blockCount];

                for (int i = 0; i < blockCount; i++)
                {
                    c.chunkData[i] = (EBlockType) wd.allChunkData[index];
                    c.healthData[i] = EBlockType.NoCrack;
                    index++;
                }
                
                c.CreateChunk(ChunkDimensions, chunkPos, false);
                Chunks.Add(chunkPos, c);
                RedrawChunk(c);
                
                yield return null;
            }

            fpc.transform.position = new Vector3(wd.fpcX, wd.fpcY, wd.fpcZ);
            mCamera.SetActive(false);
            fpc.SetActive(true);
            _lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
        }

        private void RedrawChunk(Chunk c)
        {
            DestroyImmediate(c.GetComponent<MeshFilter>());
            DestroyImmediate(c.GetComponent<MeshRenderer>());
            DestroyImmediate(c.GetComponent<MeshCollider>());
            c.CreateChunk(ChunkDimensions, c.location, false);
        }

        private WaitForSeconds _threeSeconds = new(3);

        private IEnumerator HealBlock(Chunk c, int blockIndex)
        {
            yield return _threeSeconds;
            if (c.chunkData[blockIndex] != EBlockType.Air)
            {
                c.healthData[blockIndex] = EBlockType.NoCrack;
                
                RedrawChunk(c);
            }
        }

        private EBlockType _buildType = EBlockType.Dirt;
        public void SetBuildType(int type)
        {
            _buildType = (EBlockType) type;
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
            for (int y = 0; y < WorldDimensions.y; y++)
            {
                Vector3Int position = new(x, ChunkDimensions.y * y, z);

                if (!ChunkChecker.Contains(position))
                {
                    GameObject chunk = Instantiate(chunkPrefab, transform, true);
                    Chunk chunkComponent = chunk.GetComponent<Chunk>();
                    chunk.name = $"Chunk_{position.x}_{position.y}_{position.z}";
                    chunkComponent.CreateChunk(ChunkDimensions, position);
                    ChunkChecker.Add(position);
                    Chunks.Add(position, chunkComponent);
                }
                
                Chunks[position].MeshRenderer.enabled = meshEnabled;
            }

            ChunkColumns.Add(new Vector2Int(x, z));
        }

        private IEnumerator BuildExtraWorld()
        {
            int zEnd = WorldDimensions.z + _extraWorldDimensions.z;
            int zStart = WorldDimensions.z;
            
            int xEnd = WorldDimensions.x + _extraWorldDimensions.x;
            int xStart = WorldDimensions.x;

            for (int z = zStart; z < zEnd; z++)
            {
                for (int x = 0; x < xEnd; x++)
                {
                    BuildChunkColumn(x * ChunkDimensions.x, z * ChunkDimensions.z, false);
                    yield return null;
                }
            }
            
            for (int z = 0; z < zEnd; z++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    BuildChunkColumn(x * ChunkDimensions.x, z * ChunkDimensions.z, false);
                    yield return null;
                }
            }
        }

        private IEnumerator BuildWorld()
        {
            mCamera.SetActive(true);
            loadingBar.gameObject.SetActive(true);
            fpc.SetActive(false);

            for (int z = 0; z < WorldDimensions.z; z++)
            {
                for (int x = 0; x < WorldDimensions.x; x++)
                {
                    BuildChunkColumn(x * ChunkDimensions.x, z * ChunkDimensions.z);
                    loadingBar.value += 1;
                    yield return null;
                }
            }

            mCamera.SetActive(false);
            loadingBar.gameObject.SetActive(false);

            float xPos = (ChunkDimensions.x * WorldDimensions.x) / 2f;
            float zPos = (ChunkDimensions.z * WorldDimensions.z) / 2f;
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
                if ((_lastBuildPosition - fpc.transform.position).magnitude > ChunkDimensions.x)
                {
                    Vector3 fpcPosition = fpc.transform.position;
                    _lastBuildPosition = Vector3Int.CeilToInt(fpcPosition);
                    int posX = (int) (fpcPosition.x / ChunkDimensions.x) * ChunkDimensions.x;
                    int posZ = (int) (fpcPosition.z / ChunkDimensions.z) * ChunkDimensions.z;
                    _buildQueue.Enqueue(BuildRecursiveWorld(posX, posZ, DrawRadius));
                    _buildQueue.Enqueue(HideColumns(posX, posZ));
                }

                yield return _wfs;
            }
        }

        private void HideChunkColumn(int x, int z)
        {
            for (int y = 0; y < WorldDimensions.y; y++)
            {
                Vector3Int pos = new(x, y * ChunkDimensions.y, z);
                if (ChunkChecker.Contains(pos))
                {
                    Chunks[pos].MeshRenderer.enabled = false;
                }
            }
        }

        private IEnumerator HideColumns(int x, int z)
        {
            Vector2Int fpcPos = new(x, z);
            foreach (Vector2Int chunkColumn in ChunkColumns)
            {
                if ((chunkColumn - fpcPos).magnitude >= DrawRadius * ChunkDimensions.x)
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
            
            BuildChunkColumn(x, z + ChunkDimensions.z);
            _buildQueue.Enqueue(BuildRecursiveWorld(x, z + ChunkDimensions.z, nextRad));
            yield return null;
            
            BuildChunkColumn(x, z - ChunkDimensions.z);
            _buildQueue.Enqueue(BuildRecursiveWorld(x, z - ChunkDimensions.z, nextRad));
            yield return null;
            
            BuildChunkColumn(x + ChunkDimensions.x, z );
            _buildQueue.Enqueue(BuildRecursiveWorld(x + ChunkDimensions.x, z, nextRad));
            yield return null;
            
            BuildChunkColumn(x - ChunkDimensions.x, z );
            _buildQueue.Enqueue(BuildRecursiveWorld(x - ChunkDimensions.x, z, nextRad));
            yield return null;
        }
    }
}