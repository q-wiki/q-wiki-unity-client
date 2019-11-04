using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WikidataGame.Models;

/// <summary>
///     This class handles the behaviour of a single tile.
///     Several values from the backend are stored and updated in place to ensure accessibility.
/// </summary>
public class TileController : MonoBehaviour
{
    private MeshRenderer _circleRenderer;
    private bool _direction;
    private Game _game;
    private bool _isHighlight;

    /**
     * private fields
     */
    private string _myId;
    public IList<Category> availableCategories;
    public string chosenCategoryId;
    public int difficulty;
    public GameObject grid;
    public string id;

    /**
     * public fields
     */

    public long internalId;
    public string ownerId;
    public Material[] tileMaterials;

    private GameController GameController => GameObject.Find("GameController").GetComponent<GameController>();

    /// <summary>
    ///     When a TileController is instantiated, its material is set.
    /// </summary>
    private void Start()
    {
        _myId = GameController.PlayerId();
        _circleRenderer = transform
            .Find("TileBase/OuterCircle")
            .GetComponent<MeshRenderer>();

        if (ownerId == _myId)
            _circleRenderer.material = tileMaterials[0];
        else if (ownerId != _myId && ownerId != null)
            _circleRenderer.material = tileMaterials[1];
    }

    /// <summary>
    ///     The update function checks if a tile needs to be highlighted.
    ///     It is highlighted if its a possible move for the current user.
    ///     The highlighting looks like a glow.
    ///     Directions of alpha value: true = up, false = down
    /// </summary>
    private void Update()
    {
        if (_isHighlight)
        {
            var color = _circleRenderer.material.color;
            _direction = Math.Abs(color.a) <= 0.1f || Math.Abs(color.a) >= 0.3f ? !_direction : _direction;
            color.a += _direction ? 0.02f : -0.02f;
            _circleRenderer.material.color = color;
        }
    }

    /// <summary>
    ///     This function is used to activate or deactivate all children of a Transform object.
    /// </summary>
    /// <param name="transform">The Transform object to be modified.</param>
    /// <param name="value">Should children be set active or inactive?</param>
    private void SetActiveAllChildren(Transform transform, bool value)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(value);
            SetActiveAllChildren(child, value);
        }
    }

    /// <summary>
    ///     When the user clicks on a tile that's accessible and a possible move,
    ///     a canvas is shown depending on the state of the tile.
    /// </summary>
    private void OnMouseDown()
    {
        if (IsPointerOverUIObject())
            return;

        var gridController = grid.GetComponent<GridController>();

        if (!gridController.IsPossibleMove(this))
            return;

        var previousTile = GameController.selectedTile;
        if (previousTile != null)
        {
            var circleRenderer = GameController.selectedTile.transform.Find("TileBase/OuterCircle")
                .GetComponent<MeshRenderer>();

            if (string.IsNullOrEmpty(previousTile.GetComponent<TileController>().ownerId))
                circleRenderer.material = tileMaterials[3];
            else if (previousTile.GetComponent<TileController>().ownerId == _myId)
                circleRenderer.material = tileMaterials[0];
            else
                circleRenderer.material = tileMaterials[1];
        }

        GameController.selectedTile = gameObject;

        gameObject.transform.Find("TileBase/OuterCircle").GetComponent<MeshRenderer>().material = tileMaterials[2];


        //Owned
        if (ownerId == _myId && difficulty < 2)
        {
            SetActiveAllChildren(gridController.actionCanvas.GetComponent<Transform>(), true);
            gridController.actionCanvas.SetActive(true);
            if (gridController.captureButton.activeSelf &&
                gridController.attackButton.activeSelf)
            {
                gridController.captureButton.SetActive(false);
                gridController.attackButton.SetActive(false);
            }

            GameController.levelText.text = "Tile Level: " + (difficulty + 1);
        }
        //Enemy
        else if (ownerId != _myId && !string.IsNullOrEmpty(ownerId))
        {
            SetActiveAllChildren(gridController.actionCanvas.GetComponent<Transform>(), true);
            gridController.actionCanvas.SetActive(true);
            if (gridController.captureButton.activeSelf &&
                gridController.levelUpButton.activeSelf)
            {
                gridController.captureButton.SetActive(false);
                gridController.levelUpButton.SetActive(false);
            }

            GameController.levelText.text = "Tile Level: " + (difficulty + 1);
        }
        //Empty
        else if (string.IsNullOrEmpty(ownerId))
        {
            gridController.categoryCanvas.SetActive(false);
            SetActiveAllChildren(gridController.actionCanvas.GetComponent<Transform>(), true);
            gridController.actionCanvas.SetActive(true);
            if (gridController.attackButton.activeSelf &&
                gridController.levelUpButton.activeSelf)
            {
                gridController.attackButton.SetActive(false);
                gridController.levelUpButton.SetActive(false);
            }

            GameController.levelText.text = "Tile Level: " + (difficulty + 1);
        }
        else
        {
            Debug.Log("Tile already max Level, no actions left here");
        }
    }

    /// <summary>
    ///     This function is used to check if there are any UI objects showing above the tile.
    /// </summary>
    /// <returns></returns>
    private bool IsPointerOverUIObject()
    {
        var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    /// <summary>
    ///     This function is used to set whether a tile should be highlighted.
    /// </summary>
    /// <param name="isHighlight">Should a tile be highlighted?</param>
    public void SetHighlight(bool isHighlight)
    {
        _isHighlight = isHighlight;
    }
}