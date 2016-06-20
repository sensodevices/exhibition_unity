using UnityEngine;
using System;

public class FingerTarget : MonoBehaviour
{
	public HandNetworkData.FingerType fingerId;
	private HandNetworkData.DataType m_handType;
	public HandNetworkData.DataType HandType { get { return m_handType; } }
	private SensoEventEmitter m_emitter;
	private NetworkManager m_netMan;

	private bool isVibrating { get { return m_isVibrating; } }
	private bool m_isVibrating = false;
	private byte m_vibrateStrength = 2;

	private DateTime lastSent;

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
	}

	public void SetNetworkManager(NetworkManager netMan) {
		m_netMan = netMan;
	}

	public void SetHandType(HandNetworkData.DataType handType)
	{
		m_handType = handType;
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
}
