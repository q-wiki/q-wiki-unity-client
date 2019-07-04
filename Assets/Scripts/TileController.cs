using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WikidataGame.Models;

public class TileController : MonoBehaviour
{
    public string id;
    public string ownerId;
    public string myId;
    public int difficulty;
    public IList<Category> availableCategories;
    public Category chosenCategory;
    public GameObject grid;
    public GameObject menuController;
    private Game game;

    void Start()
    {
        menuController = GameObject.Find("MenuController");
        myId = menuController.GetComponent<MenuController>().PlayerId();
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
        Debug.Log(ownerId);
        if (EventSystem.current.IsPointerOverGameObject()) return;
        /*foreach (WikidataGame.Models.Category cat in availableCategories)
        {
            Debug.Log(cat.Title);
        }*/
        menuController.GetComponent<MenuController>().selectedTile = gameObject;
        GridController gridController = grid.GetComponent<GridController>();

        /**
         * tile is owned
         */
        
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
        
        /**
         * tile belongs to opponent
         */
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
        /**
         * tile is empty / null
         */
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
            throw new Exception("This is not a tile");
    }
}