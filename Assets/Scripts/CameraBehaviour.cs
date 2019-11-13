using UnityEngine;

/// <summary>
///     This class is used to handle the special camera movement used in the game.
/// </summary>
public class CameraBehaviour : Singleton<CameraBehaviour>
{
    /*
     * private fields
     */

    private Vector3 dragOrigin;
    public float dragSpeed = 0.5f;
    private Vector2 firstTouchPrevPos;
    public float heightLimitMax = 14f;
    public float heightLimitMin = 3f;
    public float MAX_X = 11.2f;
    public float MAX_Z = 4.2f;
    public float MIN_X = -8.4f;
    public float MIN_Z = -18.4f;
    public float moveSpeed = 0.15f;
    public int rotationDegree = 2;
    private Vector2 secondTouchPrevPos;
    private float touchesCurPosDifference;
    private float touchesPrevPosDifference;

    /*
     * public fields 
     */

    public bool active = true;
    public float yTranslocation = 0.5f;
    private bool zoomedIn = true;
    private float zoomModifier;
    public float zTranslocation = 0.5f;


    /// <summary>
    ///     The update function handles all input by the user and transforms it to camera positions.
    /// </summary>
    private void Update()
    {
        if (!active) return;
        
        // Key Movement
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            transform.position +=
                moveSpeed * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        // Curved Zoom
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && transform.position.y >= heightLimitMin)
        {
            GetComponent<Transform>().position = new Vector3(transform.position.x,
                transform.position.y - yTranslocation, transform.position.z + zTranslocation);
            transform.Rotate(-rotationDegree, 0, 0);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && transform.position.y <= heightLimitMax)
        {
            GetComponent<Transform>().position = new Vector3(transform.position.x,
                transform.position.y + yTranslocation, transform.position.z - zTranslocation);
            transform.Rotate(rotationDegree, 0, 0);
        }
        //Touch
        else if (Input.touchCount == 2)
        {
            var firstTouch = Input.GetTouch(0);
            var secondTouch = Input.GetTouch(1);

            firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
            secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;

            touchesPrevPosDifference = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
            touchesCurPosDifference = (firstTouch.position - secondTouch.position).magnitude;

            //Zoom in
            if (touchesPrevPosDifference > touchesCurPosDifference && transform.position.y <= heightLimitMax)
            {
                GetComponent<Transform>().position = new Vector3(transform.position.x,
                    transform.position.y + yTranslocation, transform.position.z - zTranslocation);
                gameObject.transform.eulerAngles =
                    new Vector3(gameObject.transform.eulerAngles.x + rotationDegree, 0, 0);
            }

            //Zoom out
            if (touchesPrevPosDifference < touchesCurPosDifference && transform.position.y >= heightLimitMin)
            {
                GetComponent<Transform>().position = new Vector3(transform.position.x,
                    transform.position.y - yTranslocation, transform.position.z + zTranslocation);
                gameObject.transform.eulerAngles =
                    new Vector3(gameObject.transform.eulerAngles.x - rotationDegree, 0, 0);
            }
        }

        // Mouse Movement
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        var pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        var move = new Vector3(pos.x * -dragSpeed, 0, pos.y * -dragSpeed);

        transform.Translate(move, Space.World); /**/

        var posCur = transform.position;
        posCur.x = Mathf.Clamp(transform.position.x, MIN_X, MAX_X);
        posCur.z = Mathf.Clamp(transform.position.z, MIN_Z, MAX_Z);

        transform.position = new Vector3(posCur.x, transform.position.y, posCur.z);
    }

    public void Toggle()
    {
        active = !active;
    }
}