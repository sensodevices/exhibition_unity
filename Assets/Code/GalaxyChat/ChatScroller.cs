using UnityEngine;
using System.Collections;

public class ChatScroller : MonoBehaviour {

	private bool m_touching = false;
	private GameObject m_touchObject;
	private Vector3 m_lastTouchPosition;

	void OnTriggerEnter (Collider other) {
		if (!m_touching) {
			m_touchObject = other.gameObject;
			m_touching = true;
		}

	}

	void FixedUpdate () {
		if (m_touching) {
			float dY = m_lastTouchPosition.y - m_touchObject.transform.position.y;
			// Debug.Log("dY: " + dY);
			m_lastTouchPosition = m_touchObject.transform.position;
		}
	}
	
	void OnTriggerExit (Collider other) {
		if (m_touchObject == other.gameObject) {
			m_touching = false;
		}
	}
}
