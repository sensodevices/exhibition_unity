using UnityEngine;

public class CameraRotation : MonoBehaviour {

    private GameObject m_sensoManager;

    private Vector3 prevRotation;
    public void Start()
    {
        prevRotation = transform.localEulerAngles;
        m_sensoManager = (GameObject)GameObject.FindGameObjectWithTag("SensoManager");
    }

    public void FixedUpdate() 
    {
        float dy = prevRotation.y - transform.localEulerAngles.y;
        prevRotation = transform.localEulerAngles;
        m_sensoManager.transform.RotateAround(transform.parent.position, Vector3.down, dy);
    }

}