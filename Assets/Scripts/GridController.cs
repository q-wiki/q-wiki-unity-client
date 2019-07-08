using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WikidataGame;
using WikidataGame.Models;

public class GridController: MonoBehaviour
{
    /**
     * public fields
     */
    
    public GameObject[] baseTiles;
    public GameObject[] categoryObjects;
    public GameObject hexPrefabSpawn;
    public GameObject captureButton;
    public GameObject attackButton;
    public GameObject levelUpButton;
    public GameObject categoryCanvas;
    public GameObject actionCanvas;
    public bool addGap = true;
    public float gap = 0.0f;

    /**
     * private fields
     */
    
    private float hexWidth = 1.732f;
    private float hexHeight = 2.0f;
    private GameObject[,] tileArray;
    private IList<IList<Tile>> tileSystem;
    private Vector3 startPos;
    private IList<TileController> _possibleMoves;

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

        // build tiles
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

        long count = 0;

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
                    
                    /**
                     * initialize tileController for current tile
                     */
                    TileController tileController = tile.GetComponent<TileController>();
                    tileController.difficulty = tileSystem[x][z].Difficulty ?? 0;
                    tileController.availableCategories = tileSystem[x][z].AvailableCategories;
                    tileController.chosenCategoryId = tileSystem[x][z].ChosenCategoryId;
                    tileController.id = tileSystem[x][z].Id;
                    tileController.ownerId = tileSystem[x][z].OwnerId;
                    tileController.grid = gameObject;
                    tileController.internalId = count;

                    if(tileController.chosenCategoryId != null)
                    {
                        GameObject categoryPlaceholder = tile
                            .transform
                            .Find("TileAssetsL0/CategoryPlaceholder")
                            .gameObject;

                        /*
                         * get name of category by looking up ID in available categories
                         */
                        string categoryName = tileController
                            .availableCategories
                            .First(c => c.Id == tileController.chosenCategoryId)
                            .Title;

                        foreach (GameObject cat in categoryObjects)
                        {
                            if (cat.name == categoryName)
                            {
                                var categoryItem = Instantiate(cat, categoryPlaceholder.transform, true);
                                tile.transform.position = new Vector3(0, 0, 0);
                            }
                        }
                    }

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

                count++;

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

    /**
     * function to show all possible moves to user
     */
    public void ShowPossibleMoves(String ownerId)
    {
        List<TileController> possibleMoves = new List<TileController>();

        IList<TileController> tiles = tileArray
            .Cast<GameObject>()
            .Where(t => t != null)
            .Select(t => t.GetComponentInChildren<TileController>())
            .Where(t => t.ownerId == ownerId)
            .ToList();

        foreach (var tile in tiles)
        {
            possibleMoves.Add(tile);
            possibleMoves.AddRange(GetNeighbors(tile));
        }

        foreach (var tile in possibleMoves)
            tile.SetHighlight(true);

        /**
         * store possible moves in variable for later use
         */
        _possibleMoves = possibleMoves;

    }
    
        
    /**
     * function to check if selected tile is a neighbor of a currently owned tile
     */
    public IList<TileController> GetNeighbors(TileController tile)
    {
        if(tile ==  null)
            throw new Exception("There was no tile provided");

        GameObject[] tiles = tileArray.Cast<GameObject>().ToArray();
        
        // find all neighbors of given tile
        IList<TileController> neighbors = tiles.Select(t =>
        {
            if (t == null)
                return null;
            
            TileController tileController = t.GetComponentInChildren<TileController>();
            return AreNeighbors(tile.internalId, tileController.internalId) ? tileController : null;
        }).Where(t => t != null).ToList();

        return neighbors;
    }

    /**
     * this function is used to determine if an index of a tile can be a neighbor of another tile
     * its a little different depending on the row
     * this has to be dependent of the size of the grid which it currently is not
     */
    private bool AreNeighbors(long first, long second)
    {
        long row = first / 10;

        if (row % 2 != 0 && (first - 1 == second ||
                             first + 1 == second ||
                             first - 9 == second ||
                             first - 10 == second ||
                             first + 10 == second ||
                             first + 11 == second))
            return true;

        
        if (row % 2 == 0 && (first - 1 == second ||
                             first + 1 == second ||
                             first + 9 == second ||
                             first - 10 == second ||
                             first + 10 == second ||
                             first - 11 == second))
            return true;

        
        return false;
    }

    public bool IsPossibleMove(TileController tile)
    {
        return _possibleMoves.Contains(tile);
    }
}
