using UnityEngine;
using System;

public class PotModuleCollider : MonoBehaviour
{
    private bool m_touching = false;
    private GameObject m_touchObject = null;
    private GameObject sensoMan = null;

    void Start () {
        sensoMan = GameObject.FindWithTag("SensoManager");
    }

    void OnTriggerEnter(Collider other) {
        if (!m_touching) {
            m_touchObject = other.gameObject;
            m_touching = true;
            if (sensoMan != null) BroadcastMessage("SubscribeEvents", sensoMan);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject == m_touchObject) {
            m_touching = false;
            m_touchObject = null;
            if (sensoMan != null) BroadcastMessage("UnsubscribeEvents", sensoMan);
        }
    }

}

