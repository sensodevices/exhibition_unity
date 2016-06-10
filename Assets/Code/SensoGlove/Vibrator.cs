using UnityEngine;
using System.Collections;
using System;

public class Vibrator : MonoBehaviour {

	public int fingerId;
	private NetworkManager m_netMan;
	private bool isVibrating { get { return m_isVibrating; } }
	private bool m_isVibrating = false;
	private byte m_vibrateStrength = 2;

	private DateTime lastSent;

	public HandNetworkData.DataType Type;
	
	// Use this for initialization
	void Start () {
		lastSent = DateTime.Now;
		var anObj = GameObject.FindWithTag("GameController");
		if (anObj != null) {
			m_netMan = anObj.GetComponent<NetworkManager>();
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (isVibrating) {
			var diff = DateTime.Now.Subtract (lastSent);
			if (diff.Milliseconds * 1000 + diff.Milliseconds > 300) {
				m_netMan.VibrateFinger(Type, (byte)fingerId, m_vibrateStrength);
				lastSent = DateTime.Now;
			}
		}
	}

	void OnTriggerEnter(Collider other) {
		if (isVibrating)
			return;
		if (other.gameObject.GetComponent<PotObject>() != null) {
			StartVibrate();
		}
	}

	void OnTriggerExit(Collider other) {
		if (!isVibrating)
			return;
		if (other.gameObject.GetComponent<PotObject>() != null) {
			StopVibrate();
		}
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
