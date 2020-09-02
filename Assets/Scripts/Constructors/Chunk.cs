using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Chunk
{

    public Block[,,] chunkMap;
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> UVs = new List<Vector2>();
    public List<Vector3> noCollVertices = new List<Vector3>();
    public List<int> noCollTriangles = new List<int>();
    public List<Vector2> noCollUVs = new List<Vector2>();
    Mesh nocollideMesh;
    Mesh chunkMesh;
    public Material chunkMaterial;
    public GameObject chunkObj;
    public GameObject chunkNoCollideObj;

    //For each normal chunk there's a chunk in another layer that does not collide with the player character so it can walk through billboards like grass 
    //but the interaction system still detects it
    public Chunk(Vector3 position, Material mat)
    {
        chunkObj = new GameObject("Chunk");
        chunkNoCollideObj = new GameObject("Chunk");
        chunkObj.transform.position = position;
        chunkNoCollideObj.transform.position = position;
        chunkMaterial = mat;

        MakeChunk();
    }

    void MakeChunk()
    {
        chunkMap = new Block[World.chunkSize, World.chunkSize, World.chunkSize];

        GenerateVirtualMap();
    }

    //Here we set the probabilities of each type on block in each position based on the noise library
    void GenerateVirtualMap()
    {
        for (int x = 0; x < World.chunkSize; x++)
            for (int y = 0; y < World.chunkSize; y++)
                for (int z = 0; z < World.chunkSize; z++)
                {
                    int offset = 4 * World.chunkSize;

                    int worldX = (int)(x + chunkObj.transform.position.x);
                    int worldY = (int)((y + offset) + chunkObj.transform.position.y);
                    int worldZ = (int)(z + chunkObj.transform.position.z);

                    if (worldY <= Noise.GenerateStoneHeight(worldX, worldZ))
                    {
                        if (Noise.fBM3D(worldX, worldY, worldZ, 0.3f, 2) < 0.19 && worldY - offset < 5)
                        {

                            chunkMap[x, y, z] = DB_Blocks.GetBlockByName("Diamond");
                        }
                        else
                        {

                            chunkMap[x, y, z] = DB_Blocks.GetBlockByName("Stone");

                        }
                    }
                    else if (worldY < Noise.GenerateHeight(worldX, worldZ))
                    {

                        chunkMap[x, y, z] = DB_Blocks.GetBlockByName("Dirt");
                    }
                    else if (worldY == Noise.GenerateHeight(worldX, worldZ))
                    {

                        chunkMap[x, y, z] = DB_Blocks.GetBlockByName("Grass");

                    }
                    else
                    {

                        chunkMap[x, y, z] = DB_Blocks.GetBlockByName("Air");
                    }

                    if (Noise.fBM3D(worldX, worldY, worldZ, 0.1f, 3) < 0.35f)
                    {

                        chunkMap[x, y, z] = DB_Blocks.GetBlockByName("Air");

                    }

                    if (worldY - offset == 0)
                    {

                        chunkMap[x, y, z] = DB_Blocks.GetBlockByName("Bedrock");
                    }
                    else if (worldY == Noise.GenerateGrassHeight(worldX, worldZ) && worldY > Noise.GenerateStoneHeight(worldX, worldZ) + 1 && worldY != Noise.GenerateStoneHeight(worldX, worldZ) + 1 && Random.Range(0, 100) > 90)
                    {
                        if (!BlockExistsAtPos(new Vector3(x, y - 1, z)))
                        {
                            chunkMap[x, y, z] = DB_Blocks.GetBlockByName("Air");
                        }
                        else
                        {
                            chunkMap[x, y, z] = DB_Blocks.GetBlockByName("Grass_bill");
                        }

                    }

                }

    }

    public void GenerateBlocksMap()
    {
        for (int x = 0; x < World.chunkSize; x++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int z = 0; z < World.chunkSize; z++)
                {
                    if (chunkMap[x, y, z] != DB_Blocks.GetBlockByName("Air"))
                    {
                        Block_Cube newCube = new Block_Cube(this, new Vector3(x, y, z), chunkMap[x, y, z]);
                    }
                }
            }
        }
        GeneratePhysicalChunk();
    }

    //Once every chunk has the information of which block goes where the mesh is generated only rendering the sides of the blocks that the player would be able to see
    //(which is the same to say that the faces touching an air block or a transparent block get rendered)
    void GeneratePhysicalChunk()
    {
        chunkMesh = new Mesh();

        MeshFilter mf = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();
        MeshCollider mc = chunkObj.AddComponent<MeshCollider>();

        chunkMesh.vertices = vertices.ToArray();
        chunkMesh.triangles = triangles.ToArray();
        chunkMesh.uv = UVs.ToArray();

        chunkMesh.RecalculateBounds();
        chunkMesh.RecalculateNormals();

        mf.mesh = chunkMesh;
        mr.material = chunkMaterial;

        mc.sharedMesh = chunkMesh;

        //NO COLLIDE OBJ
        nocollideMesh = new Mesh();

        MeshFilter mfnc = chunkNoCollideObj.AddComponent<MeshFilter>();
        MeshRenderer mrnc = chunkNoCollideObj.AddComponent<MeshRenderer>();
        MeshCollider mcnc = chunkNoCollideObj.AddComponent<MeshCollider>();

        nocollideMesh.vertices = noCollVertices.ToArray();
        nocollideMesh.triangles = noCollTriangles.ToArray();
        nocollideMesh.uv = noCollUVs.ToArray();

        nocollideMesh.RecalculateBounds();
        nocollideMesh.RecalculateNormals();

        mfnc.mesh = nocollideMesh;
        mrnc.material = chunkMaterial;

        mcnc.sharedMesh = nocollideMesh;
        chunkNoCollideObj.layer = 8;
    }


    //This are the functions used to see which block face need to be rendered by nowing which blocks are next to the block we are checking
    public bool BlockExistsAtPos(Vector3 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        if (x < 0 || x >= World.chunkSize ||
            y < 0 || y >= World.chunkSize ||
            z < 0 || z >= World.chunkSize)
        {
            Chunk neighbourChunk = World.GetChunkAtPos(chunkObj.transform.position + position);

            x = ConvertIndexToLocal(x);
            y = ConvertIndexToLocal(y);
            z = ConvertIndexToLocal(z);

            if (neighbourChunk != null)
                return neighbourChunk.BlockExistsAtPos(new Vector3(x, y, z));
            else
                return false;
        }
        else if (chunkMap[x, y, z].isTransparent)
            return false;

        else
            return true;
    }
    public bool BillboardExistAtPos(Vector3 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        if (x < 0 || x >= World.chunkSize ||
            y < 0 || y >= World.chunkSize ||
            z < 0 || z >= World.chunkSize)
        {
            Chunk neighbourChunk = World.GetChunkAtPos(chunkObj.transform.position + position);

            x = ConvertIndexToLocal(x);
            y = ConvertIndexToLocal(y);
            z = ConvertIndexToLocal(z);

            if (neighbourChunk != null)
                return neighbourChunk.BillboardExistAtPos(new Vector3(x, y, z));
            else
                return false;
        }
        else if (chunkMap[x, y, z].isBillboard)
            return true;

        else
            return false;
    }
    public string BlockAtPos(Vector3 chunkPosition, Vector3 blockPosition)
    {
        int x = (int)chunkPosition.x;
        int y = (int)chunkPosition.y;
        int z = (int)chunkPosition.z;

        if (x < 0 || x >= World.chunkSize ||
            y < 0 || y >= World.chunkSize ||
            z < 0 || z >= World.chunkSize)
        {
            Chunk neighbourChunk = World.GetChunkAtPos(chunkObj.transform.position + chunkPosition);

            x = ConvertIndexToLocal(x);
            y = ConvertIndexToLocal(y);
            z = ConvertIndexToLocal(z);

            if (neighbourChunk != null)
            {
                return neighbourChunk.chunkMap[x, y, z].name;
            }
            else
            {
                return null;
            }
        }
        return null;

    }

    int ConvertIndexToLocal(int i)
    {
        if (i == -1)
            i = World.chunkSize - 1;
        else if (i == World.chunkSize)
            i = 0;
        return i;
    }

    public void ClearChunk()
    {
        vertices.Clear();
        triangles.Clear();
        UVs.Clear();

        noCollVertices.Clear();
        noCollTriangles.Clear();
        noCollUVs.Clear();
    }

}