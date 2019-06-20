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
    public string myId;

    public int difficulty;
    public IList<WikidataGame.Models.Category> availableCategories;
    public WikidataGame.Models.Category chosenCategories;

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

            //Owned
            if (ownerId == myId)
            {
                SetActiveAllChildren(grid.GetComponent<GridController>().actionCanvas.GetComponent<Transform>(), true);
                Debug.Log("Red");
            //Instantiate(actionPanelPrefab, GameObject.FindGameObjectWithTag("Canvas").transform);
            grid.GetComponent<GridController>().actionCanvas.SetActive(true);

                if (grid.GetComponent<GridController>().captureButton.activeSelf && grid.GetComponent<GridController>().attackButton.activeSelf)
                {
                grid.GetComponent<GridController>().captureButton.SetActive(false);
                grid.GetComponent<GridController>().attackButton.SetActive(false);
                }
            }
            //Enemy
            else if (ownerId != myId && !string.IsNullOrEmpty(ownerId))
            {
                SetActiveAllChildren(grid.GetComponent<GridController>().actionCanvas.GetComponent<Transform>(), true);
                Debug.Log("Blue");
            grid.GetComponent<GridController>().actionCanvas.SetActive(true);

                if (grid.GetComponent<GridController>().captureButton.activeSelf && grid.GetComponent<GridController>().levelUpButton.activeSelf)
                {
                grid.GetComponent<GridController>().captureButton.SetActive(false);
                grid.GetComponent<GridController>().levelUpButton.SetActive(false);
                }
            }
            //Empty
            else if (string.IsNullOrEmpty(ownerId))
             {
                menuController.GetComponent<MenuController>().availableCategories = availableCategories;
                grid.GetComponent<GridController>().categoryCanvas.SetActive(false);
                

                SetActiveAllChildren(grid.GetComponent<GridController>().actionCanvas.GetComponent<Transform>(), true);
                 Debug.Log("Null");
                grid.GetComponent<GridController>().actionCanvas.SetActive(true);

                 if (grid.GetComponent<GridController>().attackButton.activeSelf && grid.GetComponent<GridController>().levelUpButton.activeSelf)
                 {
                grid.GetComponent<GridController>().attackButton.SetActive(false);
                grid.GetComponent<GridController>().levelUpButton.SetActive(false);
                 }
             }
             else
             {
                 Debug.Log("Invalid");
             }

        }
        



}

