using UnityEngine;
using System.Collections;

public class MouseOrbit : MonoBehaviour
{

    public Vector3 target;
    public float rotateSpeed = 12.0f;
    public float panSpeed = 12.0f;
    public float scrollSpeed = 10.0f;

    public float zoomMin = 1.0f;
    public float zoomMax = 20.0f;
    public float distance = 10;
    public bool isActivated;

    private float x = 0.0f;
    private float y = 0.0f;

    // Use this for initialization
    void Start ()
    {
        target = transform.position + (transform.forward * 10);
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void Update () {

        //Update position to target position


        // only update if the mousebutton is held down
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftControl))
        {
            isActivated = true;
        }

        // if mouse button is let UP then stop rotating camera
        if (Input.GetMouseButtonUp(0))
        {
            isActivated = false;
        }

        if (isActivated)
        {
            CameraManager.instance.cameraState = CameraManager.CameraState.Orbit;
            //  get the distance the mouse moved in the respective direction
            x += Input.GetAxis("Mouse X") * rotateSpeed;
            y -= Input.GetAxis("Mouse Y") * rotateSpeed;

            // when mouse moves left and right we actually rotate around local y axis
            transform.RotateAround(target * distance, transform.up, x);

            // when mouse moves up and down we actually rotate around the local x axis
            transform.RotateAround(target * distance, transform.right, y);

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);

            // reset back to 0 so it doesn't continue to rotate while holding the button
            x=0;
            y=0;
        }
        else
        {
            // see if mouse wheel is used
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                // get mouse wheel info to zoom in and out
                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel")*scrollSpeed, 0, zoomMax);
                if (distance < zoomMin)
                {
                    target += transform.forward;
                    distance = zoomMin;
                }
            }
            
            //Pan camera with scrollwheel
            if (Input.GetMouseButton(2) || Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))
            {
                CameraManager.instance.cameraState = CameraManager.CameraState.Pan;
                x = (Input.GetAxis("Mouse X") * panSpeed) * (distance / 12);
                y = (Input.GetAxis("Mouse Y") * panSpeed) * (distance / 12);
                target -= transform.right * x;
                target -= transform.up * y;
            }
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            target = transform.position + (transform.forward * distance);
        }

        UpdateCameraPos();
    }

    public void Focus(Transform _target)
    {
        //Set distance depending on target scale
        distance = Mathf.Clamp(((_target.localScale.x + _target.localScale.y + _target.localScale.z) / 3) * 10, zoomMin, zoomMax);
        target = _target.position;
    }

    void UpdateCameraPos()
    {
        // position the camera FORWARD the right distance towards target
        Vector3 position = -(transform.forward*distance) + target;

        // move the camera
        transform.position = position;
    }
}