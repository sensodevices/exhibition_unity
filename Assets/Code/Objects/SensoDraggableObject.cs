using UnityEngine;
using System.Collections.Generic;

public class SensoDraggableObject : MonoBehaviour
{
    private SensoHand m_handAttached = null;
    private bool m_attached = false;

    private Transform m_parent;
    private Queue<Vector3> instantSpeeds;
    private Vector3 previousPosition;
    
    private bool m_touched = false;
    private Rigidbody m_rb;

    private float m_attachedTime = 0.0f;

    void Start()
    {
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

    void FixedUpdate()
    {
        if (m_attached) {
            /*m_attachedTime += Time.fixedDeltaTime;
            if (m_attachedTime >= 15.0f) {
                do_detach();
            }
            if (!m_palmAttached.Grasping && (m_palmAttached.transform.eulerAngles.z < 140 || m_palmAttached.transform.eulerAngles.z > 220)) {
                do_detach();
            }*/
        }
    }


    void OnTriggerEnter(Collider other) 
    {
        if (m_handAttached == null) {
            var palm = other.gameObject.GetComponent<SensoPalm>();
            if (palm != null) {
                m_handAttached = palm.Hand;
                if (m_handAttached != null) {
                    m_handAttached.OnGrabStart += onGrabStart;
                    m_handAttached.OnGrabEnd += onGrabEnd;
                }
                if (m_handAttached.Grabbing) {
                    do_attach();
                } else {
                    m_handAttached.ToggleVibrateAll(2);
                }
            }
        }
    }

    void OnTriggerExit(Collider other) 
    {
        var palm = other.gameObject.GetComponent<SensoPalm>();
        if (palm != null) {
            var h = palm.Hand;
            if (h != null && h == m_handAttached) {
                if (m_handAttached != null) {
                    m_handAttached.OnGrabStart -= onGrabStart;
                    m_handAttached.OnGrabEnd -= onGrabEnd;
                }
                if (m_attached) {
                    do_detach();
                }
                m_handAttached.ToggleVibrateAll(0);
                m_handAttached = null;
            }
        }
    }    

    void onGrabStart(object sender, SensoGrabArgs args)
    {
        do_attach();
    }
    void onGrabEnd(object sender, SensoGrabArgs args)
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
        transform.parent = m_handAttached.GetPalm().transform;
        m_rb.isKinematic = true;
        if (m_handAttached != null)
            m_handAttached.ToggleVibrateAll(7);
        m_attachedTime = 0.0f;
    }

    private void do_detach()
    {
        m_attached = false;
        transform.parent = m_parent;
        if (m_handAttached != null)
            m_handAttached.ToggleVibrateAll(2);
        m_rb.isKinematic = false;
        Vector3 instantSpeed = Vector3.zero;
        int counter = 0;
        while (instantSpeeds.Count > 0) {
            instantSpeed += instantSpeeds.Dequeue();
            ++counter;
        }
        instantSpeed /= counter;
        m_rb.velocity = instantSpeed * 1.5f;
    }

}