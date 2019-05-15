using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseDownTile : MonoBehaviour
{

    Renderer objectRenderer;

    public Transform prefab;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
    }





    void OnMouseDown()
    {
        //Debug.Log(meshRenderer.material.color);

        if (objectRenderer.sharedMaterial.name == "Red")
        {
            Debug.Log("Red");
        }
        else if (objectRenderer.sharedMaterial.name == "Blue")
        {
            Debug.Log("Blue");
        }
        else if (objectRenderer.sharedMaterial.name == "White")
        {
            Debug.Log("White");
            Instantiate(prefab);
        } else
        {
            Debug.Log("Invalid");
        }
    }
}
