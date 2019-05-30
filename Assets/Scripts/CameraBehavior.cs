using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{

    public float yTranslocation = 0.2f;
    public float zTranslocation = 0.2f;
    public float moveSpeed = 0.15f;

    public float heightLimitMin = 3f;
    public float heightLimitMax = 14f;

    public int rotationDegree = 2;


    public float dragSpeed = 0.5f;
    private Vector3 dragOrigin;

    private bool zoomedIn = true;

    void Start()
    {
        
    }
    
    void Update()
    {
        // Key Movement
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            transform.position += moveSpeed * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        }

        // Curved Zoom
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && transform.position.y >= heightLimitMin)
        {
            GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y - yTranslocation, transform.position.z + zTranslocation);
            transform.Rotate(-rotationDegree / 2f, 0, 0);
        }
        else

        if (Input.GetAxis("Mouse ScrollWheel") < 0 && transform.position.y < heightLimitMax)
        {
            GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y + yTranslocation, transform.position.z - zTranslocation);
            transform.Rotate(rotationDegree / 2f, 0, 0);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && transform.position.y >= heightLimitMax)
        {
            zoomedIn = false;
        }

            // Mouse Movement
            if (Input.GetMouseButtonDown(0))
         {
             dragOrigin = Input.mousePosition;
             return;
         }

         if (!Input.GetMouseButton(0)) return;

         Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
         Vector3 move = new Vector3(pos.x * - dragSpeed, 0, pos.y * - dragSpeed);

         transform.Translate(move, Space.World);/**/

    }

    public bool IsZoomedIn()
    {
        return zoomedIn;
    }
}
