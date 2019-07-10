using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{

    public float yTranslocation = 0.5f;
    public float zTranslocation = 0.5f;
    public float moveSpeed = 0.15f;

    public float heightLimitMin = 3f;
    public float heightLimitMax = 14f;

    public int rotationDegree = 2;


    public float dragSpeed = 0.5f;
    private Vector3 dragOrigin;

    private bool zoomedIn = true;

    public float MIN_X = -8.4f;
    public float MAX_X = 11.2f;
    public float MIN_Z = -18.4f;
    public float MAX_Z = 4.2f;

    Vector2 firstTouchPrevPos, secondTouchPrevPos;

    float touchesPrevPosDifference, touchesCurPosDifference, zoomModifier;

    
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
            transform.Rotate(-rotationDegree, 0, 0);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && transform.position.y <= heightLimitMax)
        {
            GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y + yTranslocation, transform.position.z - zTranslocation);
            transform.Rotate(rotationDegree, 0, 0);
        }
        //Touch
        else if (Input.touchCount == 2)
        {

            Touch firstTouch = Input.GetTouch(0);
            Touch secondTouch = Input.GetTouch(1);

            firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
            secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;

            touchesPrevPosDifference = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
            touchesCurPosDifference = (firstTouch.position - secondTouch.position).magnitude;

            //Zoom in
            if (touchesPrevPosDifference > touchesCurPosDifference && transform.position.y <= heightLimitMax)
            {
                GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y + yTranslocation, transform.position.z - zTranslocation);
                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x + rotationDegree, 0, 0);
            }

            //Zoom out
            if (touchesPrevPosDifference < touchesCurPosDifference && transform.position.y >= heightLimitMin)
            {
                GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y - yTranslocation, transform.position.z + zTranslocation);
                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x - rotationDegree, 0, 0);
            }
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

         Vector3 posCur = transform.position;
         posCur.x = Mathf.Clamp(transform.position.x, MIN_X, MAX_X);
         posCur.z = Mathf.Clamp(transform.position.z, MIN_Z, MAX_Z);

        transform.position = new Vector3(posCur.x, transform.position.y, posCur.z);


    }

    public bool IsZoomedIn()
    {
        return zoomedIn;
    }
}
