using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseDownTile : MonoBehaviour
{

    Renderer objectRenderer;

    public GameObject actionPanelPrefab;
    public GameObject captureButton, attackButton, levelUpButton;
   

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
        if (objectRenderer.sharedMaterial.name == "Red")
        {
            SetActiveAllChildren(actionPanelPrefab.GetComponent<Transform>(), true);
            Debug.Log("Red");
            //Instantiate(actionPanelPrefab, GameObject.FindGameObjectWithTag("Canvas").transform);
            actionPanelPrefab.SetActive(true);

            if(captureButton.activeSelf && attackButton.activeSelf)
            {
                captureButton.SetActive(false);
                attackButton.SetActive(false);
            }
        }
        else if (objectRenderer.sharedMaterial.name == "Blue")
        {
            SetActiveAllChildren(actionPanelPrefab.GetComponent<Transform>(), true);
            Debug.Log("Blue");
            actionPanelPrefab.SetActive(true);

            if (captureButton.activeSelf && levelUpButton.activeSelf)
            {
                captureButton.SetActive(false);
                levelUpButton.SetActive(false);
            }
        }
        else if (objectRenderer.sharedMaterial.name == "White")
        {

            SetActiveAllChildren(actionPanelPrefab.GetComponent<Transform>(), true);
            Debug.Log("White");
            actionPanelPrefab.SetActive(true);

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
