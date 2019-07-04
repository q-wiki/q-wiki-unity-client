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

    private MenuController menuController => GameObject.Find("MenuController").GetComponent<MenuController>();
    
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
        if (EventSystem.current.IsPointerOverGameObject()) 
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
}