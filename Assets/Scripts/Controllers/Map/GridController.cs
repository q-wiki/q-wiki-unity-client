using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WikidataGame.Models;
using Random = UnityEngine.Random;

namespace Controllers.Map
{
    /// <summary>
    ///     This class is used to control grid / map behaviours.
    /// </summary>
    public class GridController : MonoBehaviour
    {
        /**
         * * public fields
         * */

        [HideInInspector] public TileController selectedTile;

        public GameObject[] baseTiles;
        public GameObject[] categoryObjects;
        public bool addGap = true;
        public float gap = 0.3f;

        /**
         * * private fields
         * */

        private IList<TileController> _possibleMoves;
        private float hexWidth = 1.732f;
        private long rows;
        private Vector3 startPos;
        private GameObject[,] tileArray;
        private IList<IList<Tile>> tileSystem;
        private long columns;
        private float hexHeight = 2.0f;

        /// <summary>
        ///     This function generates a gid out of information about the tiles.
        /// </summary>
        /// <param name="tiles">A two dimensional array of the tiles</param>
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

            /**
            * * set row and column count depending on tile system
            * */

            rows = tileSystem[0].Count;
            columns = tileSystem.Count;

            if (addGap) AddGap();

            // Calculate the starting position for grid
            CalcStartPos();

            // build tiles
            CreateGrid();
        }

        /// <summary>
        ///     This function is used to destroy the grid.
        /// </summary>
        private void DestroyGrid()
        {
            for (var z = 0; z < tileSystem[0].Count; z++)
            for (var x = 0; x < tileSystem.Count; x++)
                Destroy(tileArray[x, z]);
        }

        /// <summary>
        ///     This function is used to create the grid in Unity.
        ///     It assigns a TileController to each of the tiles.
        /// </summary>
        private void CreateGrid()
        {
            long count = 0;


            for (var z = 0; z < tileSystem[0].Count; z++)
            for (var x = 0; x < tileSystem.Count; x++)
            {
                GameObject tile;

                if (tileSystem[x][z] != null)
                {
                    // Choose random tile design for right level
                    var random = Random.Range(0, baseTiles.Length);
                    GameObject baseT;
                    baseT = baseTiles[random];
                    tile = Instantiate(baseT, transform, true);

                    /**
                     * initialize tileController for current tile
                     */
                    var tileController = tile.GetComponent<TileController>();
                    tileController.difficulty = tileSystem[x][z].Difficulty ?? 0;
                    tileController.availableCategories = tileSystem[x][z].AvailableCategories;
                    tileController.chosenCategoryId = tileSystem[x][z].ChosenCategoryId;
                    tileController.id = tileSystem[x][z].Id;
                    tileController.ownerId = tileSystem[x][z].OwnerId;
                    tileController.internalId = count;

                    if (tileController.chosenCategoryId != null)
                    {
                        var categoryPlaceholder = tile
                            .transform
                            .Find("TileAssetsL0/CategoryPlaceholder")
                            .gameObject;

                        /*
                         * get name of category by looking up ID in available categories
                         */
                        var categoryName = tileController
                            .availableCategories
                            .First(c => c.Id == tileController.chosenCategoryId)
                            .Title;

                        foreach (var cat in categoryObjects)
                            if (cat.name == categoryName)
                            {
                                var categoryItem = Instantiate(cat, categoryPlaceholder.transform, true);
                                tile.transform.position = new Vector3(0, 0, 0);
                            }
                    }

                    if (tileSystem[x][z].Difficulty > 0) tile.GetComponent<Animator>().SetBool("BaseA", true);
                    if (tileSystem[x][z].Difficulty > 1) tile.GetComponent<Animator>().SetBool("BaseA2", true);

                    var gridPos = new Vector3(x, 0, z);

                    tile.transform.position = CalcWorldPos(gridPos);
                    tile.name = "GridTile" + x + "|" + z;

                    float randRotationY = Random.Range(0, 360);
                    var rotation = new Vector3(0, randRotationY, 0);
                    tile.transform.Rotate(rotation);

                    tileArray[x, z] = tile;
                }

                count++;
            }
        }

        /// <summary>
        ///     This function is used to calculate the world position of a tile.
        /// </summary>
        /// <param name="gridPos">The position of the grid.</param>
        /// <returns>A calculated vector.</returns>
        private Vector3 CalcWorldPos(Vector3 gridPos)
        {
            float offset = 0;
            var y = gridPos.y;
            if (gridPos.z % 2 != 0)
                offset = hexWidth / 2;

            var z = startPos.x + gridPos.x * hexWidth + offset;
            var x = startPos.z - gridPos.z * hexHeight * 0.75f;
            return new Vector3(x, y, z);
        }

        /// <summary>
        ///     This function is used to calculate the start position of the grid.
        /// </summary>
        private void CalcStartPos()
        {
            float offset = 0;
            if (tileSystem.Count / 2 % 2 != 0)
                offset = hexWidth / 2;

            var x = -hexWidth * (tileSystem[0].Count / 2) - offset;
            var z = hexHeight * 0.75f * (tileSystem.Count / 2);

            startPos = new Vector3(x, 0, z);
        }

        /// <summary>
        ///     This function is used to add a gap between the tiles.
        /// </summary>
        private void AddGap()
        {
            hexWidth += hexWidth * gap;
            hexHeight += hexHeight * gap;
        }

        /// <summary>
        /// Use this function to clear the selected tile.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void ClearSelection()
        {
            if (selectedTile == null)
                throw new Exception(
                    "There is not selected tile at the moment - this function should have not been called.");

            var tile = selectedTile;
            tile.Deselect();
            selectedTile = null;
        }

        /// <summary>
        ///     This function is used to show all possible moves to a user.
        /// </summary>
        /// <param name="ownerId">ID of the user</param>

        public void ShowPossibleMoves(Guid? ownerId)
        {
            var possibleMoves = new List<TileController>();

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

        /// <summary>
        ///     This function is used to check if the selected tile is a neighbor of a currently owned tile.
        /// </summary>
        /// <param name="tile">A tile from the grid.</param>
        /// <returns>A list of neighboring tiles.</returns>
        /// <exception cref="Exception">No tile was provided when calling this function.</exception>
        public IList<TileController> GetNeighbors(TileController tile)
        {
            if (tile == null)
                throw new Exception("There was no tile provided");

            var tiles = tileArray.Cast<GameObject>().ToArray();

            // find all neighbors of given tile
            IList<TileController> neighbors = tiles.Select(t =>
            {
                if (t == null)
                    return null;

                var tileController = t.GetComponentInChildren<TileController>();
                return AreNeighbors(tile.internalId, tileController.internalId) ? tileController : null;
            }).Where(t => t != null).ToList();

            return neighbors;
        }

        /// <summary>
        ///     This function is used to determine if an index of a tile can be a neighbor of another tile.
        ///     It's a little different depending on the row.
        /// </summary>
        /// <param name="first">Index of the first tile</param>
        /// <param name="second">Index of the second tile</param>
        /// <returns>Whether the two tiles are neighbors</returns>
        private bool AreNeighbors(long first, long second)
        {
            // is row odd or even?
            var firstRow = first / rows;
            var secondRow = second / rows;

            // if tiles' rows are not next to each other, it's not possible they are neighbors
            if (Math.Abs(firstRow - secondRow) > 1)
                return false;

            // if tiles are in the same row, neighbors can be found easily
            if (firstRow == secondRow && (first - 1 == second ||
                                          first + 1 == second))
                return true;

            // if tiles are not in the same row, there need to be a few checks depending on the type of the row
            if (firstRow != secondRow)
            {
                // row is odd
                if (firstRow % 2 != 0 && (first - (columns - 1) == second ||
                                          first - columns == second ||
                                          first + columns == second ||
                                          first + columns + 1 == second))
                    return true;

                // row is even
                if (firstRow % 2 == 0 && (first + (columns - 1) == second ||
                                          first - columns == second ||
                                          first + columns == second ||
                                          first - (columns + 1) == second))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     This function is used to determine if a tile is a possible move.
        /// </summary>
        /// <param name="tile">The tile to check</param>
        /// <returns>If tile is a possible move</returns>
        public bool IsPossibleMove(TileController tile)
        {
            return _possibleMoves.Contains(tile);
        }
    }
}