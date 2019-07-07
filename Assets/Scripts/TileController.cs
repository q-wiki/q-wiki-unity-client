using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WikidataGame;
using WikidataGame.Models;

public class TileController : MonoBehaviour
{
    public long internalId;
    
    public string id;
    public string ownerId;
    public int difficulty;
    public IList<Category> availableCategories;
    public string chosenCategoryId;
    public GameObject grid;
    public Material[] tileMaterials;
    private string myId;


    private Game game;

    private MenuController menuController => GameObject.Find("MenuController").GetComponent<MenuController>();

    void Start()
    {
        myId = menuController.PlayerId();
        if (ownerId == myId)
            gameObject.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[0];
        else if (ownerId != myId && ownerId != null)
            gameObject.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[1];
    }

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

        GameObject previousTile = menuController.selectedTile;
        if (previousTile != null)
        {
            if (string.IsNullOrEmpty(previousTile.GetComponent<TileController>().ownerId))
            {
                menuController.selectedTile.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[3];
            }
            else if (previousTile.GetComponent<TileController>().ownerId == myId)
            {
                menuController.selectedTile.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[0];
            }
            else
            {
                menuController.selectedTile.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[1];
            }
        }

        menuController.selectedTile = gameObject;

        gameObject.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[2];

        IList<TileController> neighbors = grid.GetComponent<GridController>().GetNeighbors(this);
        foreach (var tile in neighbors)
        {
            tile.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[4];
        }

        var gridController = grid.GetComponent<GridController>();

        //Owned
        if (ownerId == myId && difficulty < 2)
        {
            SetActiveAllChildren(gridController.actionCanvas.GetComponent<Transform>(), true);
            gridController.actionCanvas.SetActive(true);
            if (gridController.captureButton.activeSelf &&
                gridController.attackButton.activeSelf)
            {
                gridController.captureButton.SetActive(false);
                gridController.attackButton.SetActive(false);
            }
            menuController.levelText.text = "Level: " + (difficulty + 1);
        }
        //Enemy
        else if (ownerId != myId && !string.IsNullOrEmpty(ownerId))
        {
            SetActiveAllChildren(gridController.actionCanvas.GetComponent<Transform>(), true);
            gridController.actionCanvas.SetActive(true);
            if (gridController.captureButton.activeSelf &&
                gridController.levelUpButton.activeSelf)
            {
                gridController.captureButton.SetActive(false);
                gridController.levelUpButton.SetActive(false);
            }
            menuController.levelText.text = "Level: " + (difficulty + 1);
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
            menuController.levelText.text = "Level: " + (difficulty + 1);
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

}
