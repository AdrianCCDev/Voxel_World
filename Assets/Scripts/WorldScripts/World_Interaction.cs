using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World_Interaction : MonoBehaviour
{
    public GameObject selector;

    Vector3 playerPosBody;
    Vector3 playerPosHead;




    void Update()
    {
        //Get position of player
        Vector3 playerPos = gameObject.transform.position;
        playerPosBody = new Vector3(Mathf.Round(playerPos.x), Mathf.Round(playerPos.y), Mathf.Round(playerPos.z));
        playerPosHead = new Vector3(Mathf.Round(playerPos.x), Mathf.Round(playerPos.y + 1), Mathf.Round(playerPos.z));

        //Check which block is the player looking at and move the selector object to that position
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 10f))
        {
            Vector3 hitBlock;
            hitBlock = hit.point - hit.normal / 2.0f;

            Vector3 placeBlock;
            placeBlock = hit.point + hit.normal / 2.0f;


            hitBlock.x = (int)(Mathf.Round(hitBlock.x));
            hitBlock.y = (int)(Mathf.Round(hitBlock.y));
            hitBlock.z = (int)(Mathf.Round(hitBlock.z));

            placeBlock.x = (int)(Mathf.Round(placeBlock.x));
            placeBlock.y = (int)(Mathf.Round(placeBlock.y));
            placeBlock.z = (int)(Mathf.Round(placeBlock.z));

            selector.transform.position = hitBlock;



            Interaction(hit, hitBlock, placeBlock);

        }
        else
        {
            selector.transform.position = new Vector3(0, -5000, 0);
        }


    }
    //Manage right-click and left-click interaction with the world and update the surrounding meshes
    void Interaction(RaycastHit hit, Vector3 hitPosition, Vector3 placePosition)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 chunkBlockPos = hitPosition - hit.collider.gameObject.transform.position;

            Chunk chunkAtPos = World.GetChunkAtPos(hit.collider.transform.position);

            Chunk chunkUp;

            int x = (int)chunkBlockPos.x;
            int y = (int)chunkBlockPos.y;
            int z = (int)chunkBlockPos.z;



            ClearChunk(chunkAtPos);

            //Check if the block that the player is trying to remove is bedrock. If that is not the case then remove that block by changing its value to be an Air block
            if (chunkAtPos.chunkMap[(int)chunkBlockPos.x, (int)chunkBlockPos.y, (int)chunkBlockPos.z] != DB_Blocks.GetBlockByName("Bedrock"))
            {
                chunkAtPos.chunkMap[(int)chunkBlockPos.x, (int)chunkBlockPos.y, (int)chunkBlockPos.z] = DB_Blocks.GetBlockByName("Air");
            }



            // We want to see if the block above it is a billboard to remove it alongside the block the player wants to remove
            if ((int)chunkBlockPos.y + 1 < World.chunkSize)
            {
                if (chunkAtPos.BillboardExistAtPos(new Vector3((int)chunkBlockPos.x, (int)chunkBlockPos.y + 1, (int)chunkBlockPos.z)))
                {
                    chunkAtPos.chunkMap[(int)chunkBlockPos.x, (int)chunkBlockPos.y + 1, (int)chunkBlockPos.z] = DB_Blocks.GetBlockByName("Air");
                }
                //But sometimes that block is in another chunk so we have to look at that as well  
            }
            else
            {
                chunkUp = World.GetNeighbour(chunkAtPos, "top");

                Vector3 pos = World.GetBlockMapPosition(new Vector3((int)chunkBlockPos.x, (int)chunkBlockPos.y + 1, (int)chunkBlockPos.z));
                if (chunkUp.chunkMap[(int)pos.x, (int)pos.y, (int)pos.z].isBillboard)
                {
                    chunkUp.chunkMap[(int)pos.x, (int)pos.y, (int)pos.z] = DB_Blocks.GetBlockByName("Air");
                }



            }

            UpdateClosestNeighbours(chunkBlockPos, chunkAtPos);

            chunkAtPos.GenerateBlocksMap();


        }
        //Place block where player is looking but not inside itself
        if (Input.GetMouseButtonDown(1))
        {
            if (placePosition != playerPosBody && placePosition != playerPosHead)
            {
                Chunk chunkAtPos = World.GetChunkAtPos(placePosition);

                ClearChunk(chunkAtPos);

                Vector3 pos = World.GetBlockMapPosition(placePosition);
                chunkAtPos.chunkMap[(int)pos.x, (int)pos.y, (int)pos.z] = DB_Blocks.GetBlockByName("Stone");
                chunkAtPos.GenerateBlocksMap();

                UpdateClosestNeighbours(pos, chunkAtPos);
            }

        }

    }



    void ClearChunk(Chunk chunk)
    {
        DestroyImmediate(chunk.chunkObj.GetComponent<MeshFilter>());
        DestroyImmediate(chunk.chunkObj.GetComponent<MeshRenderer>());
        DestroyImmediate(chunk.chunkObj.GetComponent<MeshCollider>());
        DestroyImmediate(chunk.chunkNoCollideObj.GetComponent<MeshFilter>());
        DestroyImmediate(chunk.chunkNoCollideObj.GetComponent<MeshRenderer>());
        DestroyImmediate(chunk.chunkNoCollideObj.GetComponent<MeshCollider>());
        chunk.ClearChunk();
    }

    void UpdateClosestNeighbours(Vector3 position, Chunk actualChunk)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        if (x == 0)
        {
            RegenerateNeighbour(World.GetNeighbour(actualChunk, "left"));

        }
        else if (x == World.chunkSize - 1)
        {
            RegenerateNeighbour(World.GetNeighbour(actualChunk, "right"));

        }

        if (y == 0)
        {
            RegenerateNeighbour(World.GetNeighbour(actualChunk, "bottom"));

        }
        else if (y == World.chunkSize - 1)
        {
            RegenerateNeighbour(World.GetNeighbour(actualChunk, "top"));

        }

        if (z == 0)
        {
            RegenerateNeighbour(World.GetNeighbour(actualChunk, "back"));

        }
        else if (z == World.chunkSize - 1)
        {
            RegenerateNeighbour(World.GetNeighbour(actualChunk, "front"));

        }

    }

    void RegenerateNeighbour(Chunk neighbour)
    {
        if (neighbour != null)
        {
            ClearChunk(neighbour);
            neighbour.GenerateBlocksMap();

        }
    }
    int ConvertIndexToLocal(int i)
    {
        if (i == -1)
            i = World.chunkSize - 1;
        else if (i == World.chunkSize)
            i = 0;
        return i;
    }
}
