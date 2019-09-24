using UnityEngine;
using System;

public class FingerMovedArgs : EventArgs {
	public Vector3 deltaMove { get; private set; }
	public FingerMovedArgs(Vector3 delta) {
		deltaMove = delta;
	}
}

///
/// @brief Component that handles logic for finger tips
///
public class MyFingerTip : SensoFingerTip
{
	// private NetworkManager m_netMan;
	WeakReference m_palm;

	private DateTime lastSent;
	private Vector3 m_lastPosition;
	private int m_moveSubscribed = 0;
	

	public event EventHandler<FingerMovedArgs> onMove; // Is fired when finger has moved

	// Update is called once per frame
	override protected void FixedUpdate () {
		base.FixedUpdate();

		/*if (m_moveSubscribed > 0) {
			Vector3 deltaPos = transform.position - m_lastPosition;
			var arg = new FingerMovedArgs(deltaPos);
			onMove(this, arg);
			m_lastPosition = transform.position;
		}*/
	}

	public void AddMoveSubscriber() { ++m_moveSubscribed; }
	public void RemoveMoveSubscriber() { --m_moveSubscribed; }


	void OnTriggerEnter(Collider other) {
		/*if(vibrator == null && !other.GetComponent<SensoFingerTip>()) {
			if (other.gameObject.layer != LayerMask.NameToLayer("Interactable_novibrate")) {
				vibrator = other.gameObject;
				Vibrate();
			}
		}*/
		/*if (m_emitter != null) {
			var otherFingerTarget = other.gameObject.GetComponent<FingerTip>();
			if (otherFingerTarget != null) {
				m_emitter.fingerCollided(this, otherFingerTarget);
			}
		}*/
	}

	void OnTriggerExit(Collider other) {
		/*if (other.gameObject == vibrator) {
			StopVibrate();
			vibrator = null;
		}*/
		/*if (m_emitter != null) {
			var otherFingerTarget = other.gameObject.GetComponent<FingerTip>();
			if (other.gameObject.GetComponent<FingerTip>() != null) {
				m_emitter.fingerReleased(this, otherFingerTarget);
			}
		}*/
	}

}
