using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct PerlinSettings
{
    public float heightScale;
    public float scale;
    public int octaves;
    public float heightOffset;
    public float probability;

    public PerlinSettings(float hs, float s, int o, float ho, float p)
    {
        heightScale = hs;
        scale = s;
        octaves = o;
        heightOffset = ho;
        probability = p;
    }
}


public class World : MonoBehaviour
{
    public static Vector3Int worldDimensions = new Vector3Int(5, 5, 5);
    public static Vector3Int extraWorldDimensions = new Vector3Int(5, 5, 5);
    public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);
    public bool loadFromFile = false;
    public GameObject chunkPrefab;
    public GameObject mCamera;
    public GameObject fpc;
    public Slider loadingBar;

    public static PerlinSettings surfaceSettings;
    public PerlinGrapher surface;

    public static PerlinSettings stoneSettings;
    public PerlinGrapher stone;

    public static PerlinSettings diamondTSettings;
    public PerlinGrapher diamondT;

    public static PerlinSettings diamondBSettings;
    public PerlinGrapher diamondB;

    public static PerlinSettings caveSettings;
    public Perlin3DGrapher caves;
    
    public HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>();
    public HashSet<Vector2Int> chunkColumns = new HashSet<Vector2Int>();
    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    Vector3Int lastBuildPosition;
    int drawRadius = 3;

    Queue<IEnumerator> buildQueue = new Queue<IEnumerator>();

    IEnumerator BuildCoordinator()
    {
        while (true)
        {
            while (buildQueue.Count > 0)
                yield return StartCoroutine(buildQueue.Dequeue());
            yield return null;
        }
    }

    public void SaveWorld()
    {
        FileSaver.Save(this);
    }

    IEnumerator LoadWorldFromFile()
    {
        WorldData wd = FileSaver.Load();
        if (wd == null)
        {
            StartCoroutine(BuildWorld());
            yield break;
        }

        chunkChecker.Clear();
        for (int i = 0; i < wd.chunkCheckerValues.Length; i += 3)
        {
            chunkChecker.Add(new Vector3Int(wd.chunkCheckerValues[i],
                wd.chunkCheckerValues[i + 1],
                wd.chunkCheckerValues[i + 2]));
        }

        chunkColumns.Clear();
        for (int i = 0; i < wd.chunkColumnValues.Length; i += 2)
        {
            chunkColumns.Add(new Vector2Int(wd.chunkColumnValues[i],
                                                wd.chunkColumnValues[i + 1]));
        }

        int index = 0;
        int vIndex = 0;
        loadingBar.maxValue = chunkChecker.Count;
        foreach (Vector3Int chunkPos in chunkChecker)
        {
            GameObject chunk = Instantiate(chunkPrefab);
            chunk.name = "Chunk_" + chunkPos.x + "_" + chunkPos.y + "_" + chunkPos.z;
            Chunk c = chunk.GetComponent<Chunk>();
            int blockCount = chunkDimensions.x * chunkDimensions.y * chunkDimensions.z;
            c.chunkData = new MeshUtils.BlockType[blockCount];
            c.healthData = new MeshUtils.BlockType[blockCount];

            for (int i = 0; i < blockCount; i++)
            {
                c.chunkData[i] = (MeshUtils.BlockType)wd.allChunkData[index];
                c.healthData[i] = MeshUtils.BlockType.NOCRACK;
                index++;
            }
            loadingBar.value++;
            c.CreateChunk(chunkDimensions, chunkPos, false);
            chunks.Add(chunkPos, c);
            RedrawChunk(c);
            c.meshRenderer.enabled = wd.chunkVisibility[vIndex];
            vIndex++;
            yield return null;
        }

        fpc.transform.position = new Vector3(wd.fpcX, wd.fpcY, wd.fpcZ);
        mCamera.SetActive(false);
        fpc.SetActive(true);
        loadingBar.gameObject.SetActive(false);
        lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
        StartCoroutine(BuildCoordinator());
        StartCoroutine(UpdateWorld());

    }

    // Start is called before the first frame update
    void Start()
    {
        loadingBar.maxValue = worldDimensions.x * worldDimensions.z;

        surfaceSettings = new PerlinSettings(surface.heightScale, surface.scale,
                                     surface.octaves, surface.heightOffset, surface.probability);

        stoneSettings = new PerlinSettings(stone.heightScale, stone.scale,
                             stone.octaves, stone.heightOffset, stone.probability);

        diamondTSettings = new PerlinSettings(diamondT.heightScale, diamondT.scale,
                     diamondT.octaves, diamondT.heightOffset, diamondT.probability);

        diamondBSettings = new PerlinSettings(diamondB.heightScale, diamondB.scale,
                     diamondB.octaves, diamondB.heightOffset, diamondB.probability);

        caveSettings = new PerlinSettings(caves.heightScale, caves.scale,
             caves.octaves, caves.heightOffset, caves.DrawCutOff);

        if (loadFromFile)
            StartCoroutine(LoadWorldFromFile());
        else
            StartCoroutine(BuildWorld());
    }

    MeshUtils.BlockType buildType = MeshUtils.BlockType.DIRT;
    public void SetBuildType(int type)
    {
        buildType = (MeshUtils.BlockType) type;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 10))
            {
                Vector3 hitBlock = Vector3.zero;
                if (Input.GetMouseButtonDown(0))
                {
                    hitBlock = hit.point - hit.normal / 2.0f;
                }
                else
                    hitBlock = hit.point + hit.normal / 2.0f;

                //Debug.Log("Block Location: " + hitBlock.x + "," + hitBlock.y + "," + hitBlock.z);
                Chunk thisChunk = hit.collider.gameObject.GetComponent<Chunk>();

                int bx = (int)(Mathf.Round(hitBlock.x) - thisChunk.location.x);
                int by = (int)(Mathf.Round(hitBlock.y) - thisChunk.location.y);
                int bz = (int)(Mathf.Round(hitBlock.z) - thisChunk.location.z);

                Vector3Int neighbour;
                if (bx == chunkDimensions.x)
                {
                    neighbour = new Vector3Int((int)thisChunk.location.x + chunkDimensions.x,
                                                (int)thisChunk.location.y,
                                                 (int)thisChunk.location.z);
                    thisChunk = chunks[neighbour];
                    bx = 0;
                }
                else if (bx == -1)
                {
                    neighbour = new Vector3Int((int)thisChunk.location.x - chunkDimensions.x,
                                                (int)thisChunk.location.y,
                                                 (int)thisChunk.location.z);
                    thisChunk = chunks[neighbour];
                    bx = chunkDimensions.x - 1;
                }
                else if (by == chunkDimensions.y)
                {
                    neighbour = new Vector3Int((int)thisChunk.location.x,
                                                (int)thisChunk.location.y + chunkDimensions.y,
                                                 (int)thisChunk.location.z);
                    thisChunk = chunks[neighbour];
                    by = 0;
                }
                else if (by == -1)
                {
                    neighbour = new Vector3Int((int)thisChunk.location.x,
                                                (int)thisChunk.location.y - chunkDimensions.y,
                                                 (int)thisChunk.location.z);
                    thisChunk = chunks[neighbour];
                    by = chunkDimensions.y - 1;
                }
                else if (bz == chunkDimensions.z)
                {
                    neighbour = new Vector3Int((int)thisChunk.location.x,
                                                (int)thisChunk.location.y,
                                                 (int)thisChunk.location.z + chunkDimensions.z);
                    thisChunk = chunks[neighbour];
                    bz = 0;
                }
                else if (bz == -1)
                {
                    neighbour = new Vector3Int((int)thisChunk.location.x,
                                                (int)thisChunk.location.y,
                                                 (int)thisChunk.location.z - chunkDimensions.z);
                    thisChunk = chunks[neighbour];
                    bz = chunkDimensions.z - 1;
                }


                int i = bx + chunkDimensions.x * (by + chunkDimensions.z * bz);

                if (Input.GetMouseButtonDown(0))
                {
                    if (MeshUtils.blockTypeHealth[(int)thisChunk.chunkData[i]] != -1)
                    {
                        if (thisChunk.healthData[i] == MeshUtils.BlockType.NOCRACK)
                            StartCoroutine(HealBlock(thisChunk, i));
                        thisChunk.healthData[i]++;
                        if (thisChunk.healthData[i] == MeshUtils.BlockType.NOCRACK +
                                                       MeshUtils.blockTypeHealth[(int)thisChunk.chunkData[i]])
                            thisChunk.chunkData[i] = MeshUtils.BlockType.AIR;
                    }
                }
                else
                {
                    thisChunk.chunkData[i] = buildType;
                    thisChunk.healthData[i] = MeshUtils.BlockType.NOCRACK;
                }

                RedrawChunk(thisChunk);
            }
        }
    }

    void RedrawChunk(Chunk c)
    {
        DestroyImmediate(c.GetComponent<MeshFilter>());
        DestroyImmediate(c.GetComponent<MeshRenderer>());
        DestroyImmediate(c.GetComponent<Collider>());
        c.CreateChunk(chunkDimensions, c.location, false);
    }

    WaitForSeconds threeSeconds = new WaitForSeconds(3);
    public IEnumerator HealBlock(Chunk c, int blockIndex)
    {
        yield return threeSeconds;
        if (c.chunkData[blockIndex] != MeshUtils.BlockType.AIR)
        {
            c.healthData[blockIndex] = MeshUtils.BlockType.NOCRACK;
            RedrawChunk(c);
        }
    }

    void BuildChunkColumn(int x, int z, bool meshEnabled = true)
    {
        for (int y = 0; y < worldDimensions.y; y++)
        {
            Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);
            if (!chunkChecker.Contains(position))
            {
                GameObject chunk = Instantiate(chunkPrefab);
                chunk.name = "Chunk_" + position.x + "_" + position.y + "_" + position.z;
                Chunk c = chunk.GetComponent<Chunk>();
                c.CreateChunk(chunkDimensions, position);
                chunkChecker.Add(position);
                chunks.Add(position, c);
            }
            chunks[position].meshRenderer.enabled = meshEnabled;
            
            
        }
        chunkColumns.Add(new Vector2Int(x, z));
    }

    IEnumerator BuildExtraWorld()
    {
        int zEnd = worldDimensions.z + extraWorldDimensions.z;
        int zStart = worldDimensions.z - 1;
        int xEnd = worldDimensions.x + extraWorldDimensions.x;
        int xStart = worldDimensions.x - 1;


        for (int z = zStart; z < zEnd; z++)
        {
            for (int x = 0; x < xEnd; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z, false);
                yield return null;
            }

        }

        for (int z = 0; z < zEnd; z++)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z, false);
                yield return null;
            }

        }


    }


    IEnumerator BuildWorld()
    {
        for (int z = 0; z < worldDimensions.z; z++)
        {
            for (int x = 0; x < worldDimensions.x; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z);
                loadingBar.value++;
                yield return null;
            }
            
        }

        mCamera.SetActive(false);

        int xpos = (worldDimensions.x * chunkDimensions.x) / 2;
        int zpos = (worldDimensions.z * chunkDimensions.z) / 2;

        int ypos = (int)MeshUtils.fBM(xpos, zpos, surfaceSettings.octaves, surfaceSettings.scale, surfaceSettings.heightScale, surfaceSettings.heightOffset) + 10;
        fpc.transform.position = new Vector3Int(xpos, ypos, zpos);
        loadingBar.gameObject.SetActive(false);
        fpc.SetActive(true);
        lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
        StartCoroutine(BuildCoordinator());
        StartCoroutine(UpdateWorld());
        StartCoroutine(BuildExtraWorld());
    }

    WaitForSeconds wfs = new WaitForSeconds(0.5f);
    IEnumerator UpdateWorld()
    {
        while (true)
        {
            if ((lastBuildPosition - fpc.transform.position).magnitude > (chunkDimensions.x))
            {
                lastBuildPosition = Vector3Int.CeilToInt(fpc.transform.position);
                int posx = (int)(fpc.transform.position.x / chunkDimensions.x) * chunkDimensions.x;
                int posz = (int)(fpc.transform.position.z / chunkDimensions.z) * chunkDimensions.z;
                buildQueue.Enqueue(BuildRecursiveWorld(posx, posz, drawRadius));
                buildQueue.Enqueue(HideColumns(posx, posz));
            }
            yield return wfs;
        }
    }

    public void HideChunkColumn(int x, int z)
    {
        for (int y = 0; y < worldDimensions.y; y++)
        {
            Vector3Int pos = new Vector3Int(x, y * chunkDimensions.y, z);
            if (chunkChecker.Contains(pos))
            {
                chunks[pos].meshRenderer.enabled = false;
            }
        }
    }

    IEnumerator HideColumns(int x, int z)
    {
        Vector2Int fpcPos = new Vector2Int(x, z);
        foreach (Vector2Int cc in chunkColumns)
        {
            if ((cc - fpcPos).magnitude >= drawRadius * chunkDimensions.x)
            {
                HideChunkColumn(cc.x, cc.y);
            }
        }
        yield return null;
    }

    IEnumerator BuildRecursiveWorld(int x, int z, int rad)
    {
        int nextrad = rad - 1;
        if (rad <= 0) yield break;

        BuildChunkColumn(x, z + chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z + chunkDimensions.z, nextrad));
        yield return null;

        BuildChunkColumn(x, z - chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z - chunkDimensions.z, nextrad));
        yield return null;

        BuildChunkColumn(x + chunkDimensions.x, z);
        buildQueue.Enqueue(BuildRecursiveWorld(x + chunkDimensions.x, z, nextrad));
        yield return null;

        BuildChunkColumn(x - chunkDimensions.x, z);
        buildQueue.Enqueue(BuildRecursiveWorld(x - chunkDimensions.x, z, nextrad));
        yield return null;
    }

}
