using UnityEngine;
using System.Collections;

public class ExtendedFlycam : MonoBehaviour
{

    /*
    EXTENDED FLYCAM
        Desi Quintans (CowfaceGames.com), 17 August 2012.
        Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.

    LICENSE
        Free as in speech, and free as in beer.

    FEATURES
        WASD/Arrows:    Movement
                  Q:    Climb
                  E:    Drop
                      Shift:    Move faster
                    Control:    Move slower
                        End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
    */

    public float cameraSensitivity = 90;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;
    public bool mouseEnabled = false;
    public bool moveEnabled = true;

    private float rotationX;
    private float rotationY;
    private float moveFactor;

    void Start ()
    {

    }

    void Update ()
    {
        if (mouseEnabled)
        {
            if (CameraManager.instance.cameraState != CameraManager.CameraState.Fly)
            {
                rotationX = transform.eulerAngles.y;
                rotationY = -transform.eulerAngles.x;
            }
            CameraManager.instance.cameraState = CameraManager.CameraState.Fly;
            rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
            rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
            rotationY = Mathf.Clamp (rotationY, -90, 90);
            transform.rotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.rotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
        }
        else
        {
            CameraManager.instance.cameraState = CameraManager.CameraState.Free;
        }

        if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
        {
            moveFactor = fastMoveFactor;
        }
        else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl))
        {
            moveFactor = slowMoveFactor;
        }
        else
        {
            moveFactor = 1;
        }

        if (moveEnabled)
        {
            transform.position += transform.forward * (normalMoveSpeed * moveFactor) *
                                  System.Convert.ToInt32(Input.GetKey(KeyCode.W));
            transform.position -= transform.forward * (normalMoveSpeed * moveFactor) *
                                  System.Convert.ToInt32(Input.GetKey(KeyCode.S));
            transform.position += transform.right * (normalMoveSpeed * moveFactor) *
                                  System.Convert.ToInt32(Input.GetKey(KeyCode.D));
            transform.position -= transform.right * (normalMoveSpeed * moveFactor) *
                                  System.Convert.ToInt32(Input.GetKey(KeyCode.A));
        }

        if (Input.GetKey (KeyCode.Q)) {transform.position += transform.up * climbSpeed * Time.deltaTime;}
        if (Input.GetKey (KeyCode.E)) {transform.position -= transform.up * climbSpeed * Time.deltaTime;}

        if (Input.GetKeyDown (KeyCode.Z))
        {
            if (mouseEnabled)
            {
                mouseEnabled = false;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                mouseEnabled = true;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}