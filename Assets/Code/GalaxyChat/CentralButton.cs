using UnityEngine;
using System.Collections;

public class CentralButton : MonoBehaviour {

	public float maxButtonMotion = 0.08f;
	public float pressedLevel = 0.06f;
	public float returnSpeed = 10.0f;
	private bool m_touching = false;
	private Transform m_touchObject = null;
	private Vector3 m_touchOffset = Vector3.zero;

	private Vector3 m_startPosition;
	private Vector3 m_touchReleasePosition;
	private float returningTime;
	private bool m_pressed = false;

	public PlanetCollider m_planetCollider; // Planet Collider component to synchronize with

	public bool IsActive {
		get { return m_touching; }
	}

	void Start () {
		m_startPosition = transform.position;
	}

	void Update () {
		if (m_touching) {
			Vector3 currentPos = transform.position;
			currentPos.y = m_touchObject.position.y + m_touchOffset.y;
			if (currentPos.y < transform.position.y) {
				if (currentPos.y < (m_startPosition.y - maxButtonMotion)) currentPos.y = (m_startPosition.y - maxButtonMotion);
				transform.position = currentPos;
				if (!m_pressed && (m_startPosition.y - transform.position.y) >= pressedLevel) {
					Debug.Log("pressed");
					m_pressed = true;
				}
			}
		} else {
			// Drag up
			if (transform.position.y < m_startPosition.y) {
				Vector3 currentPos = transform.position;
				returningTime += (Time.deltaTime * returnSpeed);
				currentPos.y = Mathf.Lerp(m_touchReleasePosition.y, m_startPosition.y, returningTime);
				transform.position = currentPos;
				if (m_pressed && (m_startPosition.y - transform.position.y) < pressedLevel) {
					Debug.Log("unpressed");
					m_pressed = false;
				}
			}
		} 
	}

	void OnTriggerEnter (Collider other) {
		if (!m_touching) {
			if (!m_planetCollider.IsActive) {
				m_touching = true;
				m_touchObject = other.gameObject.transform;
				m_touchOffset = transform.position - m_touchObject.position;
			}
		}
	}

	void OnTriggerExit (Collider other) {
		if (other.gameObject.transform == m_touchObject) {
			m_touching = false;
			m_touchObject = null;
			m_touchReleasePosition = transform.position;
			float dy = m_startPosition.y - m_touchReleasePosition.y;
			returningTime = 1.0f - dy / maxButtonMotion;
		}
	}
}
