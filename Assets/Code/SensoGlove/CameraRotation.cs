using UnityEngine;

public class CameraRotation : MonoBehaviour {

    public Transform sensoManager;

    private Vector3 prevRotation;
    public void Start()
    {
        prevRotation = transform.localEulerAngles;
    }

    public void FixedUpdate() 
    {
        if (Input.GetKey ("a"))
        {
            transform.Rotate(new Vector3(0, -1.0f, 0), Space.World);
        }
        if (Input.GetKey ("d"))
        {
            transform.Rotate(new Vector3(0, 1.0f, 0), Space.World);
        }
        // float dy = prevRotation.y - transform.localEulerAngles.y;
        prevRotation = transform.localEulerAngles;
        // sensoManager.transform.RotateAround(transform.parent.position, Vector3.down, dy);
    }

}