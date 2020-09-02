using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DB_Blocks : MonoBehaviour
{
    public static List<Block> blocksList = new List<Block>();

    //This is the block database, if we want to add a new block we add it to the blockList and set its propierties
    public void GenerateDB()
    {
        blocksList.Add(new Block());

        blocksList.Add(new Block("Dirt", true, false, new Vector2(0, 0), false, true));

        blocksList.Add(new Block("Grass", true, false, true, new Vector2(1, 0), new Vector2(2, 0), new Vector2(0, 0)));

        blocksList.Add(new Block("Stone", true, false, new Vector2(0, 2), false, true));

        blocksList.Add(new Block("Bedrock", true, false, new Vector2(0, 1), false, false));

        blocksList.Add(new Block("Diamond", true, false, new Vector2(1, 1), false, true));

        blocksList.Add(new Block("Grass_bill", false, true, new Vector2(2, 1), true, true));
    }

    public static Block GetBlockByName(string name)
    {
        for (int i = 0; i < blocksList.Count; i++)
        {
            if (blocksList[i].name == name)
                return blocksList[i];
        }
        return null;
    }


}
