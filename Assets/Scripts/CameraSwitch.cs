using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraSwitch : MonoBehaviour { 

    public Camera cameraTopDown;
    public Camera cameraFirstPerson;


    // Start is called before the first frame update
    void Start()
    {
        cameraFirstPerson.enabled = true;
        cameraTopDown.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (!cameraFirstPerson.GetComponentInChildren<CameraBehavior>().IsZoomedIn()) {
            ShowTopDownView();
        }

    }

    void ShowTopDownView()
    {
        cameraTopDown.enabled = true;
        cameraFirstPerson.enabled = false;
    }

    void ShowFirstPersonView()
    {
        cameraFirstPerson.enabled = true;
        cameraTopDown.enabled = false;
    }
}
