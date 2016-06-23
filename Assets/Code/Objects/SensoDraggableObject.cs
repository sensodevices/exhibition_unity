using UnityEngine;
using System.Collections.Generic;

public class SensoDraggableObject : MonoBehaviour
{
    private List<FingerTarget> fingersAttached;
    private PalmTarget m_palmAttached = null;
    private bool m_attached = false;

    private Transform m_parent;
    private Queue<Vector3> instantSpeeds;
    private Vector3 previousPosition;
    
    private bool m_touched = false;
    private Rigidbody m_rb;

    void Start()
    {
        fingersAttached = new List<FingerTarget>();
        m_parent = transform.parent;
        m_rb = GetComponent<Rigidbody>();
        instantSpeeds = new Queue<Vector3>();
    }

    void Update()
    {
        if (m_attached) {
            instantSpeeds.Enqueue((transform.position - previousPosition) / Time.deltaTime);
            previousPosition = transform.position;
            if (instantSpeeds.Count > 40) {
                instantSpeeds.Dequeue();
            }
        } else {
            if (!m_touched ) {
                // TODO: levitate :)
            }
        }
    }


    void OnTriggerEnter(Collider other) 
    {
        if (m_palmAttached == null) {
            var palm = other.gameObject.GetComponent<PalmTarget>();
            if (palm != null) {
                m_palmAttached = palm;
                m_palmAttached.onPalmGraspStart += onPalmGrasped;
                m_palmAttached.onPalmGraspEnd += onPalmGraspRelease;
                if (m_palmAttached.Grasping) {
                    do_attach();
                }
            }
        }
    }

    void OnTriggerExit(Collider other) 
    {
        var palm = other.gameObject.GetComponent<PalmTarget>();
        if (palm != null && palm == m_palmAttached) {
            m_palmAttached.onPalmGraspStart -= onPalmGrasped;
            m_palmAttached.onPalmGraspEnd -= onPalmGraspRelease;
            m_palmAttached = null;
        }
    }    

    void onPalmGrasped(object sender, PalmGraspedArgs args)
    {
        do_attach();
    }
    void onPalmGraspRelease(object sender, PalmGraspedArgs args)
    {
        do_detach();
    }

    private void do_attach()
    {
        if (!m_touched) {
            m_rb.useGravity = true;
            m_touched = true;
        }
        m_attached = true;
        transform.parent = m_palmAttached.transform;
        var rb = GetComponent<Rigidbody>();
        m_rb.isKinematic = true;
        m_palmAttached.ToggleVibrateAll(true);
    }

    private void do_detach()
    {
        m_attached = false;
        transform.parent = m_parent;
        m_palmAttached.ToggleVibrateAll(false);
        m_rb.isKinematic = false;
        Vector3 instantSpeed = Vector3.zero;
        int counter = 0;
        while (instantSpeeds.Count > 0) {
            instantSpeed += instantSpeeds.Dequeue();
            ++counter;
        }
        instantSpeed /= counter;
        m_rb.velocity = instantSpeed * 2.0f;

        // m_rb.AddForce(instantSpeed * 2.0f, ForceMode.Impulse);
    }

}