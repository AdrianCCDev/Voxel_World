using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material terrainMaterial;

    [Range(0.0f, 30.0f)]
    public int worldSize = 20;

    public static int columnheight = 30;
    public static int chunkSize = 4;

    public static Dictionary<Vector3, Chunk> chunkList;

    void Start()
    {
        //Make a Dictionary to store chunks positions and initiate the block database
        chunkList = new Dictionary<Vector3, Chunk>();
        GetComponent<DB_Blocks>().GenerateDB();



        //Generate world based on world settings
        for (int x = 0; x <= worldSize * chunkSize; x += chunkSize)
        {
            for (int z = 0; z <= worldSize * chunkSize; z += chunkSize)
            {
                GenerateColumn(x, z);
            }
        }


        foreach (KeyValuePair<Vector3, Chunk> c in chunkList)
        {
            c.Value.GenerateBlocksMap();
        }

    }

    public void GenerateColumn(int x, int z)
    {
        for (int y = 0; y < columnheight * chunkSize; y += chunkSize)
        {
            CreateChunk(new Vector3(x, y, z));

        }
    }
    void CreateChunk(Vector3 position)
    {
        Chunk newChunk = new Chunk(position, terrainMaterial);

        chunkList.Add(position, newChunk);
    }

    //Functions to acces the surrounding chunks if necessary
    public static Chunk GetChunkAtPos(Vector3 position)
    {
        float x = position.x;
        float y = position.y;
        float z = position.z;

        x = x / chunkSize;
        y = y / chunkSize;
        z = z / chunkSize;

        x = Mathf.FloorToInt(x);
        y = Mathf.FloorToInt(y);
        z = Mathf.FloorToInt(z);

        x = x * chunkSize;
        y = y * chunkSize;
        z = z * chunkSize;

        Vector3 chunkPos = new Vector3(x, y, z);

        Chunk foundChunk;

        if (chunkList.TryGetValue(chunkPos, out foundChunk))
        {
            return foundChunk;
        }
        else
        {
            return null;
        }


    }

    public static Chunk GetNeighbour(Chunk actualChunk, string side)
    {
        Vector3 actualPos = actualChunk.chunkObj.transform.position;

        switch (side)
        {
            case "front":
                return GetChunkAtPos(new Vector3(actualPos.x, actualPos.y, actualPos.z + chunkSize));
            case "back":
                return GetChunkAtPos(new Vector3(actualPos.x, actualPos.y, actualPos.z - 1));
            case "top":
                return GetChunkAtPos(new Vector3(actualPos.x, actualPos.y + chunkSize, actualPos.z));
            case "bottom":
                return GetChunkAtPos(new Vector3(actualPos.x, actualPos.y - 1, actualPos.z));
            case "right":
                return GetChunkAtPos(new Vector3(actualPos.x + chunkSize, actualPos.y, actualPos.z));
            case "left":
                return GetChunkAtPos(new Vector3(actualPos.x - 1, actualPos.y, actualPos.z));



        }
        return null;

    }

    public static Block GetBlockInWorld(Vector3 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        Chunk chunkAtPos = GetChunkAtPos(position);

        int blockPosX = x - (int)chunkAtPos.chunkObj.transform.position.x;
        int blockPosY = y - (int)chunkAtPos.chunkObj.transform.position.y;
        int blockPosZ = z - (int)chunkAtPos.chunkObj.transform.position.z;

        return chunkAtPos.chunkMap[blockPosX, blockPosY, blockPosZ];
    }

    public static Vector3 GetBlockMapPosition(Vector3 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        Chunk chunkAtPos = GetChunkAtPos(position);

        int blockPosX = x - (int)chunkAtPos.chunkObj.transform.position.x;
        int blockPosY = y - (int)chunkAtPos.chunkObj.transform.position.y;
        int blockPosZ = z - (int)chunkAtPos.chunkObj.transform.position.z;

        return new Vector3(blockPosX, blockPosY, blockPosZ);
    }
}
