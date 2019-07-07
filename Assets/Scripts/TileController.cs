using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WikidataGame;
using WikidataGame.Models;

public class TileController : MonoBehaviour
{
    public string id;
    public string ownerId;
    public int difficulty;
    public IList<Category> availableCategories;
    public string chosenCategoryId;
    public GameObject grid;
    public Material[] tileMaterials;
    
    private Game game;

    private MenuController menuController => GameObject.Find("MenuController").GetComponent<MenuController>();

    void Start()
    {
        string myId = menuController.PlayerId();
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

        menuController.selectedTile = gameObject;

        string myId = menuController.PlayerId();

        var gridController = grid.GetComponent<GridController>();

        //Owned
        if (ownerId == myId)
        {
            SetActiveAllChildren(gridController.actionCanvas.GetComponent<Transform>(), true);
            Debug.Log("Red");
            //Instantiate(actionPanelPrefab, GameObject.FindGameObjectWithTag("Canvas").transform);
            gridController.actionCanvas.SetActive(true);
            if (gridController.captureButton.activeSelf &&
                gridController.attackButton.activeSelf)
            {
                gridController.captureButton.SetActive(false);
                gridController.attackButton.SetActive(false);
            }
        }
        //Enemy
        else if (ownerId != myId && !string.IsNullOrEmpty(ownerId))
        {
            SetActiveAllChildren(gridController.actionCanvas.GetComponent<Transform>(), true);
            Debug.Log("Blue");
            gridController.actionCanvas.SetActive(true);
            if (gridController.captureButton.activeSelf &&
                gridController.levelUpButton.activeSelf)
            {
                gridController.captureButton.SetActive(false);
                gridController.levelUpButton.SetActive(false);
            }
        }
        //Empty
        else if (string.IsNullOrEmpty(ownerId))
        {
            gridController.categoryCanvas.SetActive(false);
            SetActiveAllChildren(gridController.actionCanvas.GetComponent<Transform>(), true);
            Debug.Log("Null");
            gridController.actionCanvas.SetActive(true);
            if (gridController.attackButton.activeSelf &&
                gridController.levelUpButton.activeSelf)
            {
                gridController.attackButton.SetActive(false);
                gridController.levelUpButton.SetActive(false);
            }
        }
        else
        {
            throw new Exception("Tile should not be like this");
        }
    }
    
    private bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
