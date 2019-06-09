using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WikidataGame;
using WikidataGame.Models;

public class GridController: MonoBehaviour
{
    public GameObject[] hexPrefabFree;
    public GameObject[] hexPrefabOcc;
    public GameObject[] hexPrefabSpawn;

    public float gap = 0.0f;

    private float hexWidth = 1.732f;
    private float hexHeight = 2.0f;
    
    private GameObject[,] tileArray;

    private IList<IList<Tile>> tileSystem;

    Vector3 startPos;

    public void GenerateGrid(IList<IList<Tile>> tiles)
    {
        tileSystem = tiles;
        tileArray = new GameObject[tileSystem.Count, tileSystem[0].Count];
        AddGap();
        CalcStartPos();
        CreateGrid();
    }

    void AddGap()
    {
        hexWidth += hexWidth * gap;
        hexHeight += hexHeight * gap;
    }

    void CreateGrid()
    { 
        for (int z = 0; z<tileSystem[0].Count; z++)
        {
            for (int x = 0; x< tileSystem.Count; x++)
            {
                int randomIndexFree = Random.Range(0, hexPrefabFree.Length);
                int randomIndexOcc= Random.Range(0, hexPrefabOcc.Length);
                GameObject tile;
                float height = 0;
        
                if (tileSystem[x][z] != null)
                {

                    tile = Instantiate(hexPrefabFree[randomIndexFree]) as GameObject;
                    //tile = Instantiate(tilePrefab) as GameObject;
                    height = (float)tileSystem[x][z].Difficulty;
                }
                else
                {
                    tile = Instantiate(hexPrefabOcc[randomIndexOcc]) as GameObject;
                }

                tileArray[x, z] = tile;                                                  

                Vector3 gridPos = new Vector3(x, height, z);

                tile.transform.position = CalcWorldPos(gridPos);
                tile.transform.parent = this.transform;
                tile.name = "GridTile" + x + "|" + z;
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
    
}
