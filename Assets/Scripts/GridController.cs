using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WikidataGame;
using WikidataGame.Models;

public class GridController: MonoBehaviour
{
    public GameObject[] baseTiles;
    public GameObject hexPrefabSpawn;

    public GameObject captureButton, attackButton, levelUpButton;
    public GameObject categoryCanvas, actionCanvas;


    public bool addGap = true;
    public float gap = 0.0f;

    private float hexWidth = 1.732f;
    private float hexHeight = 2.0f;

    private GameObject[,] tileArray;

    private IList<IList<Tile>> tileSystem;

    Vector3 startPos;

    public void GenerateGrid(IList<IList<Tile>> tiles)
    {

        /**
         * because the grid is currently rebuilt from scratch after each action,
         * hexWidth and hexHeight need to be set to default so the tiles do not
         * grow further apart
         */

        hexWidth = 1.732f;
        hexHeight = 2.0f;

        // Tiles from Backend
        tileSystem = tiles;
        // Gameobject Tile array with right length and depth
        tileArray = new GameObject[tileSystem.Count, tileSystem[0].Count];

        if (addGap)
        {
            AddGap();
        }

        // Calculate the starting position for grid
        CalcStartPos();

        CreateGrid();

    }

    public void DestroyGrid ()
    {
        // TODO: This causes an error in the Reporter because of null references
        for (int z = 0; z < tileSystem[0].Count; z++)
        {
            for (int x = 0; x < tileSystem.Count; x++)
            {
                Destroy(tileArray[x, z]);
            }
        }
    }

    void CreateGrid()
    {
        for (int z = 0; z<tileSystem[0].Count; z++)
        {
            for (int x = 0; x< tileSystem.Count; x++)
            {

                GameObject tile;

                if (tileSystem[x][z] != null)
                {
                    // Choose random tile design for right level
                    int random = UnityEngine.Random.Range(0, baseTiles.Length);
                    GameObject baseT;
                    baseT = baseTiles[random];
                    tile = Instantiate(baseT);

                    TileController tileController = tile.GetComponent<TileController>();
                    
                    tileController.difficulty = tileSystem[x][z].Difficulty ?? 0;
                    tileController.availableCategories = tileSystem[x][z].AvailableCategories;
                    tileController.chosenCategoryId = tileSystem[x][z].ChosenCategoryId;
                    tileController.id = tileSystem[x][z].Id;
                    tileController.ownerId = tileSystem[x][z].OwnerId;
                    tileController.grid = gameObject;

                    if (tileSystem[x][z].Difficulty > 0)
                    {
                        tile.GetComponent<Animator>().SetBool("BaseA", true);
                    }
                    if (tileSystem[x][z].Difficulty > 1)
                    {
                        tile.GetComponent<Animator>().SetBool("BaseA2", true);
                    }


                    float height = (float)tileSystem[x][z].Difficulty;

                    Vector3 gridPos = new Vector3(x, 0, z);

                    tile.transform.position = CalcWorldPos(gridPos);
                    tile.transform.parent = this.transform;
                    tile.name = "GridTile" + x + "|" + z;

                    float randRoationY = UnityEngine.Random.Range(0, 360);
                    Vector3 rotation = new Vector3(0, randRoationY, 0);
                    //Quaternion rotation = Quaternion.Euler(gameObject.transform.rotation.x, randRoationY, gameObject.transform.rotation.z);
                    tile.transform.Rotate(rotation);

                    tileArray[x, z] = tile;
                }



            }
        }

    }

    Vector3 CalcWorldPos(Vector3 gridPos)
    {
        float offset = 0;
        float y = gridPos.y;
        if (gridPos.z % 2 != 0)
            offset = hexWidth / 2;

        float z = startPos.x + gridPos.x * hexWidth + offset;
        float x = startPos.z - gridPos.z * hexHeight * 0.75f;
        return new Vector3(x, y, z);
    }

    void CalcStartPos()
    {
        float offset = 0;
        if (tileSystem.Count / 2 % 2 != 0)
            offset = hexWidth / 2;

        float x = -hexWidth * (tileSystem[0].Count / 2) - offset;
        float z = hexHeight * 0.75f * (tileSystem.Count / 2);

        startPos = new Vector3(x, 0, z);
    }


    void AddGap()
    {
        hexWidth += hexWidth * gap;
        hexHeight += hexHeight * gap;
    }
}
