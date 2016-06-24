using UnityEngine;
using System;

public class FingerMovedArgs : EventArgs {
	public Vector3 deltaMove { get; private set; }
	public FingerMovedArgs(Vector3 delta) {
		deltaMove = delta;
	}
}

public class FingerTarget : MonoBehaviour
{
	public HandNetworkData.FingerType fingerId;
	private HandNetworkData.DataType m_handType;
	public HandNetworkData.DataType HandType { get { return m_handType; } }
	private SensoEventEmitter m_emitter;
	private NetworkManager m_netMan;
	WeakReference m_palm;

	private bool isVibrating { get { return m_isVibrating; } }
	private bool m_isVibrating = false;
	private byte m_vibrateStrength = 2;
	private DateTime lastSent;
	private Vector3 m_lastPosition;
	private int m_moveSubscribed = 0;
	

	public event EventHandler<FingerMovedArgs> OnMove; // Is fired when finger has moved

	public void RegisterEventEmitter(SensoEventEmitter evEmitter)
	{
		m_emitter = evEmitter;
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (isVibrating) {
			var diff = DateTime.Now.Subtract (lastSent);
			if (diff.Milliseconds * 1000 + diff.Milliseconds > 300) {
				m_netMan.VibrateFinger(m_handType, (byte)fingerId, m_vibrateStrength);
				lastSent = DateTime.Now;
			}
		}
		if (true) /*m_moveSubscribed > 0)*/ {
			Vector3 palmPos = Vector3.zero;
			if (m_palm.IsAlive) {
				PalmTarget p = m_palm.Target as PalmTarget;
				palmPos = p.transform.position;
			}
			Vector3 myPos = (transform.position - palmPos);
			Vector3 deltaPos = myPos - m_lastPosition;
			/*var arg = new FingerMovedArgs(deltaPos);
			OnMove(this, arg);*/
			m_lastPosition = myPos;
		}

	}

	public void AddMoveSubscriber() { ++m_moveSubscribed; }
	public void RemoveMoveSubscriber() { --m_moveSubscribed; if (m_moveSubscribed < 0) m_moveSubscribed = 0; }

	public void SetNetworkManager(NetworkManager netMan) {
		m_netMan = netMan;
	}

	public void SetHandType(HandNetworkData.DataType handType)
	{
		m_handType = handType;
	}

	public void SetPalm(PalmTarget palm) 
	{
		m_palm = new WeakReference(palm);
	}

	void OnTriggerEnter(Collider other) {
		if (m_emitter != null) {
			var otherFingerTarget = other.gameObject.GetComponent<FingerTarget>();
			if (otherFingerTarget != null) {
				m_emitter.fingerCollided(this, otherFingerTarget);
			}
		}
	}

	void OnTriggerExit(Collider other) {
		if (m_emitter != null) {
			var otherFingerTarget = other.gameObject.GetComponent<FingerTarget>();
			if (other.gameObject.GetComponent<FingerTarget>() != null) {
				m_emitter.fingerReleased(this, otherFingerTarget);
			}
		}
	}

	public GameObject GetGObject() {
		return this.gameObject;
	}

	public void StartVibrate(byte strength = 2)
	{
		m_isVibrating = true;
		m_vibrateStrength = strength;
	}

	public void StopVibrate()
	{
		m_isVibrating = false;
	}

	/// <summary>Returns an angle of the finger relative to the palm</summary>
	public float GetRelativeAngle() 
	{
		if (m_palm.IsAlive) {
			PalmTarget p = m_palm.Target as PalmTarget;
			float diffZ = Quaternion.Angle(p.transform.rotation, transform.rotation);
			if (diffZ > 180) diffZ = (360.0f - diffZ);
			return diffZ;
		}  
		return 0.0f;
	}

	
}
