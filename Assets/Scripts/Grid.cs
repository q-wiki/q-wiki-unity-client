using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject hexPrefab;
    public GameObject hexPrefabFree;
    public GameObject hexPrefabOcc;
    public GameObject hexPrefabSpawn;

    public int gridWidth = 11;
    public int gridHeight = 11;

    public int spawnDistance = 1;

    public float gap = 0.0f;

    public bool realNeighbours = false;
    public float chanceAlive = 45f;
    public int deathLimit = 4;
    public int birthLimit = 4;

    public bool generateHeightDiff = false;

    private float hexWidth = 1.732f;
    private float hexHeight = 2.0f;

    private int[,] tileSystem;
    private int[,] tileHeights; 
    private GameObject[,] hexPrefabs;

    private int[] neighboursIndices_x = { 0, 1, 1, 1, 0, -1 };
    private int[] neighboursIndices_y = { -1, -1, 0, 1, 1, 0 };

    Vector3 startPos;

    void Start()
    {
        hexPrefabs = new GameObject[gridWidth, gridHeight];
        tileSystem = new int[gridWidth, gridHeight];
        tileHeights = new int[gridWidth, gridHeight];
        if (spawnDistance < 0)
        {
            spawnDistance = 0;
        }

        AddGap();
        CalcStartPos();
        CreateGrid();
    }

    void AddGap()
    {
        hexWidth += hexWidth * gap;
        hexHeight += hexHeight * gap;
    }

    void CalcStartPos()
    {
        float offset = 0;
        if (gridHeight / 2 % 2 != 0)
            offset = hexWidth / 2;

        float x = -hexWidth * (gridWidth / 2) - offset;
        float z = hexHeight * 0.75f * (gridHeight / 2);

        startPos = new Vector3(x, 0, z);
    }

    Vector3 CalcWorldPos(Vector2 gridPos)
    {
        float offset = 0;
        float y = tileHeights[(int)gridPos.x, (int)gridPos.y]; ;
        if (gridPos.y % 2 != 0)
            offset = hexWidth / 2;

        float x = startPos.x + gridPos.x * hexWidth + offset;
        float z = startPos.z - gridPos.y * hexHeight * 0.75f;

        if (generateHeightDiff)
        {
            generateRandomHeights();
        }

        return new Vector3(x, y, z);
    }

    bool IsSpawnTile(int x, int y)
    {
        if (x == spawnDistance && y == spawnDistance || x == (gridWidth - 1 - spawnDistance) && y == (gridHeight - 1 - spawnDistance))
        {
            return true;
        }
        return false;
    }

    void GenerateTileSystem()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (IsSpawnTile(x, y))
                {
                    // Spawn Tile
                    tileSystem[x, y] = 0;
                }
                else if (Random.Range(0.0f, 1.0f) < (chanceAlive / 100))
                {
                    // Free Tile
                    tileSystem[x, y] = 1;
                }
                else
                {
                    // Occupied Tile
                    tileSystem[x, y] = 2;
                }
            }
        }
    }

    int countFreeNeighbours(int x, int y)
    {
        int count = 0;

        // Use square grid neighbours
        if (!realNeighbours)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int neighbour_x = x + i;
                    int neighbour_y = y + j;

                    // Looking at ourselfs
                    if (i == 0 && j == 0)
                    {
                        // Do nothing
                    }
                    // Looking at Index outside the tilesystem
                    else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= gridWidth || neighbour_y >= gridHeight)
                    {
                        count = count + 1;
                    }
                    // Looking at free neighbour
                    else if (tileSystem[neighbour_x, neighbour_y] == 1 || tileSystem[neighbour_x, neighbour_y] == 0)
                    {
                        count = count + 1;
                    }
                }
            }
        }
        // Use hexagonal grid neighbours
        else
        {
            for (int i = 0; i < neighboursIndices_x.Length; i++)
            {
                int neighbour_x = x + neighboursIndices_x[i];
                int neighbour_y = y + neighboursIndices_y[i];
                
                // Looking at Index outside the tilesystem
                if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= gridWidth || neighbour_y >= gridHeight)
                {
                    count = count + 1;
                }
                // Looking at free neighbour
                else if (tileSystem[neighbour_x, neighbour_y] == 1 || tileSystem[neighbour_x, neighbour_y] == 0)
                {
                    count = count + 1;
                }
            }
        }
        
        return count;
    }

    int[,] DoSimulationStep()
    {
        int[,] newTileSystem = new int[gridWidth, gridHeight];
        int freeNeighbours = 0;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                freeNeighbours = countFreeNeighbours(x,y);

                // Is spawn tile
                if (tileSystem[x, y] == 0)
                {

                }
                // Is free tile
                else if(tileSystem[x, y] == 1 )
                {
                    if (freeNeighbours < deathLimit)
                    {
                        newTileSystem[x, y] = 2;
                    }
                    else
                    {
                        newTileSystem[x, y] = 1;
                    }
                } 
                // Is occupied tile
                else
                {
                    if (freeNeighbours > birthLimit)
                    {
                        newTileSystem[x, y] = 1;
                    }
                    else
                    {
                        newTileSystem[x, y] = 2;
                    }
                }
            }
        }

        return newTileSystem;
    }

    void generateRandomHeights()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                tileHeights[x, y] = Random.Range(0, 3);
            }
        }
    }

    void CreateGrid()
    {
        GenerateTileSystem();
        tileSystem = DoSimulationStep();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {

                // TODO 
                GameObject hex;
                if (tileSystem[x, y] == 0)
                {
                    hex = Instantiate(hexPrefabSpawn) as GameObject;
                }
                else if (tileSystem[x, y] == 1)
                {
                    hex = Instantiate(hexPrefabFree) as GameObject;
                }
                else
                {
                    hex = Instantiate(hexPrefabOcc) as GameObject;
                }

                hexPrefabs[x,y] = hex;

                Vector2 gridPos = new Vector2(x, y);

                hex.transform.position = CalcWorldPos(gridPos);
                hex.transform.parent = this.transform;
                hex.name = "Hexagon" + x + "|" + y;
            }
        }
    }
}
