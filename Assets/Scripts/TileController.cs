using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WikidataGame;
using WikidataGame.Models;

public class TileController : MonoBehaviour
{
    
    /**
     * public fields
     */
    
    public long internalId;
    public string id;
    public string ownerId;
    public int difficulty;
    public IList<Category> availableCategories;
    public string chosenCategoryId;
    public GameObject grid;
    public Material[] tileMaterials;

    /**
     * private fields
     */
    private string _myId;
    private Game _game;
    private bool _isHighlight;
    private bool _direction;
    private MeshRenderer _circleRenderer;
    
    private MenuController menuController => GameObject.Find("MenuController").GetComponent<MenuController>();

    void Start()
    {
        _myId = menuController.PlayerId();
        _circleRenderer = transform
            .Find("TileBase/OuterCircle")
            .GetComponent<MeshRenderer>();

        if (ownerId == _myId)
            _circleRenderer.material = tileMaterials[0];
        else if (ownerId != _myId && ownerId != null)
            _circleRenderer.material = tileMaterials[1];
    }

    
    /**
     * direction: true = up , false = down
     */
    private void Update()
    {
        if (_isHighlight)
        {
            Color color = _circleRenderer.material.color;
            _direction = (Math.Abs(color.a) <= 0.1f || Math.Abs(color.a) >= 0.3f) ? !_direction : _direction;
            color.a += _direction ? 0.02f : -0.02f;
            _circleRenderer.material.color = color;
        }
    }
    
    
    [Obsolete("LevelUp is deprecated and currently not in use")]
    public void LevelUp()
    {
        difficulty++;
        // Trigger next level animation
        if(difficulty > 1)
        {
            gameObject.GetComponent<Animator>().SetBool("BaseA2", true);
        }
        else
        {
            gameObject.GetComponent<Animator>().SetBool("BaseA", true);
        }
    }

    private void SetActiveAllChildren(Transform transform, bool value)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(value);
            SetActiveAllChildren(child, value);
        }
    }

    void OnMouseDown()
    {
        if (IsPointerOverUIObject())
            return;

        GridController gridController = grid.GetComponent<GridController>();

        if (!gridController.IsPossibleMove(this))
            return;
            
        GameObject previousTile = menuController.selectedTile;
        if (previousTile != null)
        {
            var circleRenderer = menuController.selectedTile.transform.Find("TileBase/OuterCircle").GetComponent<MeshRenderer>();
            
            if (string.IsNullOrEmpty(previousTile.GetComponent<TileController>().ownerId))
            {
                circleRenderer.material = tileMaterials[3];
            }
            else if (previousTile.GetComponent<TileController>().ownerId == _myId)
            {
                circleRenderer.material = tileMaterials[0];
            }
            else
            {
                circleRenderer.material = tileMaterials[1];
            }
        }

        menuController.selectedTile = gameObject;

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
            menuController.levelText.text = "Tile Level: " + (difficulty + 1);
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
            menuController.levelText.text = "Tile Level: " + (difficulty + 1);
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
            menuController.levelText.text = "Tile Level: " + (difficulty + 1);
        }
        else
        {
            Debug.Log("Tile already max Level, no actions left here");
        }
    }

    /**
     * function to check if there are any UI objects showing above the tile
     */
    private bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public void SetHighlight(bool isHighlight)
    {
        _isHighlight = isHighlight;
    }

}
