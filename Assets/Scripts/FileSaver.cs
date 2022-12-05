using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using S2_Quad;
using Scripts;
using UnityEngine;

[Serializable]
public class WorldData
{
    // private HashSet<Vector3Int> _chunkChecker = new();
    // private HashSet<Vector2Int> _chunkColumns = new();
    // private Dictionary<Vector3Int, Chunk> _chunks = new();

    public int[] chunkCheckerValue;
    public int[] chunkColumnValues;
    public int[] allChunkData;

    public int fpcX;
    public int fpcY;
    public int fpcZ;

    public WorldData()
    {
        
    }

    public WorldData(HashSet<Vector3Int> cc, HashSet<Vector2Int> cCols, Dictionary<Vector3Int, Chunk> chks, Vector3 fpc)
    {
        chunkCheckerValue = new int[cc.Count * 3];
        int index = 0;
        foreach (Vector3Int vector3Int in cc)
        {
            chunkCheckerValue[index] = vector3Int.x;
            chunkCheckerValue[index + 1] = vector3Int.y;
            chunkCheckerValue[index + 2] = vector3Int.z;
            index += 3;
        }

        index = 0;
        chunkColumnValues = new int[cCols.Count * 2];
        foreach (Vector2Int vector2Int in cCols)
        {
            chunkColumnValues[index] = vector2Int.x;
            chunkColumnValues[index + 1] = vector2Int.y;
            index += 2;
        }

        index = 0;
        int dimensionsSize = World.ChunkDimensions.x * World.ChunkDimensions.y * World.ChunkDimensions.z;
        allChunkData = new int[chks.Count * dimensionsSize];
        foreach (Chunk c in chks.Values)
        {
            foreach (BlockAtlas.EBlockType blockType in c.chunkData)
            {
                allChunkData[index] = (int)blockType;
                index++;
            }
        }

        fpcX = (int) fpc.x;
        fpcY = (int) fpc.y;
        fpcZ = (int) fpc.z;
    }
}

public static class FileSaver
{
    private static WorldData wd;

    static string BuildFilename()
    {
        return
            $"{Application.persistentDataPath}/savedata/World_{World.ChunkDimensions.x}_{World.ChunkDimensions.y}_{World.ChunkDimensions.z}{World.WorldDimensions.x}_{World.WorldDimensions.x}_{World.WorldDimensions.x}.dat";
    }

    public static void Save(World world)
    {
        string filename = BuildFilename();

        if (!File.Exists(filename))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        }

        BinaryFormatter bf = new();
        FileStream file = File.Open(filename, FileMode.OpenOrCreate);
        wd = new WorldData(world.ChunkChecker, world.ChunkColumns, world.Chunks, world.fpc.transform.position);
        bf.Serialize(file, wd);
        file.Close();
        Debug.Log($"Saving World to File: {filename}");
    }
}