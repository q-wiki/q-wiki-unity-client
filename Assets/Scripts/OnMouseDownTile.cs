using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnMouseDownTile : MonoBehaviour
{

    Renderer objectRenderer;

    public GameObject captureButton, attackButton, levelUpButton;
    public GameObject categoryCanvas, actionCanvas;


    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
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
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (objectRenderer.sharedMaterial.name == "Red")
        {
            SetActiveAllChildren(actionCanvas.GetComponent<Transform>(), true);
            Debug.Log("Red");
            //Instantiate(actionPanelPrefab, GameObject.FindGameObjectWithTag("Canvas").transform);
            actionCanvas.SetActive(true);

            if(captureButton.activeSelf && attackButton.activeSelf)
            {
                captureButton.SetActive(false);
                attackButton.SetActive(false);
            }
        }
        else if (objectRenderer.sharedMaterial.name == "Blue")
        {
            SetActiveAllChildren(actionCanvas.GetComponent<Transform>(), true);
            Debug.Log("Blue");
            actionCanvas.SetActive(true);

            if (captureButton.activeSelf && levelUpButton.activeSelf)
            {
                captureButton.SetActive(false);
                levelUpButton.SetActive(false);
            }
        }
        else if (objectRenderer.sharedMaterial.name == "White")
        {
            categoryCanvas.SetActive(false);
            SetActiveAllChildren(actionCanvas.GetComponent<Transform>(), true);
            Debug.Log("White");
            actionCanvas.SetActive(true);

            if (attackButton.activeSelf && levelUpButton.activeSelf)
            {
                attackButton.SetActive(false);
                levelUpButton.SetActive(false);
            }
        } else
        {
            Debug.Log("Invalid");
        }
    }
}
