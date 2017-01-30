using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public CameraState cameraState;
    private MouseOrbit orbitScript;

    public enum CameraState {Free, Orbit, Pan, Fly}

    // Use this for initialization
	void Awake ()
	{
	    if (instance == null)
	        instance = this;
	    else if (instance != this)
	        Destroy(gameObject);

	    DontDestroyOnLoad(gameObject);

	    //Save the inital target as the empty game object
	    orbitScript = GetComponent<MouseOrbit>();
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void SetTarget(Transform _target)
    {
        orbitScript.Focus(_target);
    }

    public Vector3 GetTarget()
    {
        return orbitScript.target;
    }
}