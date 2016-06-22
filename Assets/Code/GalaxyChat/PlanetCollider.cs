using UnityEngine;
using System;

public class PlanetCollider : MonoBehaviour 
{
	public float SecondsToActivateTouch = 0.3f;

	private bool m_touching = false;
	private GameObject m_touchObject;
	private Vector3 m_lastTouchPosition;
	private float m_touchTimer; // How long the touch is performing
	private bool m_touchActivated = false; // Whether time has passed to activate the touch
	public Planet m_galaxyPlanet; // Galaxy Planet component to control
	public CentralButton m_centalButton; // Central Button component to synchronize with

	public bool IsActive {
		get { return m_touchActivated; }
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (m_touching) {
			m_touchTimer += Time.fixedDeltaTime;
			if (!m_touchActivated && m_touchTimer > SecondsToActivateTouch) {
				m_touchActivated = true;
				m_lastTouchPosition = m_touchObject.transform.position;
			} else if (m_touchActivated) {
				float dist = m_lastTouchPosition.x - m_touchObject.transform.position.x;
				float dY = Mathf.Abs(m_lastTouchPosition.y - m_touchObject.transform.position.y);
				if (dY < 0.2f) { 
					m_galaxyPlanet.Scroll(dist * 5.0f, 0.0f);
					m_lastTouchPosition = m_touchObject.transform.position;
				}
			}
		}
	}

	void OnTriggerEnter(Collider other) {
		if (!m_touching) {
			if (!m_centalButton.IsActive) {
				m_touching = true;
				m_galaxyPlanet.EnterScrollCollider();
				m_touchObject = other.gameObject;
				m_touchTimer = 0.0f;
			}
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject == m_touchObject) {
			if (m_touchActivated) {
				m_galaxyPlanet.ExitScrollCollider();
			}
			m_touching = false;
			m_touchActivated = false;
			m_touchObject = null;
		}
	}
}
