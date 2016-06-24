using UnityEngine;
using System.Collections;

public class HandManager : MonoBehaviour {

	public NetworkManager netMan;
	public HandNetworkData.DataType handType;

	public GameObject[] fingerTargets;
	public GameObject[] fingerRoots;

	public Transform wrist;
	public Transform targetContainer;

	private float netDataFactor = 0.015f;
	private Vector3 startHandPosition;
	private Vector3 startHandRotation;
	private float[] fingerLengths = {0, 0, 0, 0, 0};


	void Start ()
	{
		startHandPosition = new Vector3 (this.transform.localPosition.x, this.transform.localPosition.y, this.transform.localPosition.z);
		startHandRotation = new Vector3 (this.transform.eulerAngles.x, this.transform.eulerAngles.y, this.transform.eulerAngles.z);

		for (int i = 0; i < fingerRoots.Length; ++i) {
			fingerLengths [i] = Vector3.Distance (fingerRoots [i].transform.localPosition, fingerTargets[i].transform.localPosition);
		}
		BroadcastMessage("SetHandType", handType);
	}

	void Update ()
	{
		// Hand position
		HandNetworkData.SingleHandData aData = netMan.GetHandData(handType);
		if (aData != null)
		{
			Vector3 palmPosition = aData.palmPosition;
			palmPosition = palmPosition * netDataFactor + startHandPosition;
			if (this.transform.localPosition != palmPosition) {
				this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, palmPosition, Time.deltaTime * 10);
			}

			// Rotation
			// X   Z
			// | /
			// . - Y
			Vector3 palmRotation = aData.palmRotation;
			this.transform.eulerAngles = startHandRotation;
			this.transform.Rotate(Vector3.right, palmRotation.x);
			this.transform.Rotate(Vector3.back, palmRotation.y);
			this.transform.Rotate(Vector3.down, palmRotation.z);

			// Wrist rotation
			//Vector3 wristRotation = aData.wristRotation;
			//wristRotation += wristAnglesCorrection;

			// wrist.transform.localEulerAngles = new Vector3(0.0f, wristRotation.y, -wristRotation.x);
			// targetContainer.transform.localEulerAngles = new Vector3(-wristRotation.x, wristRotation.y, 0.0f);

			//Fingers
			for (var i = 0; i < fingerTargets.Length; ++i) {
				Vector3 fingerPosition = calcFingerPosition((uint)i, aData.fingerPositions[i]);
				if (fingerPosition != fingerTargets[i].transform.localPosition) {
					fingerTargets[i].transform.localPosition = Vector3.Lerp(fingerTargets[i].transform.localPosition, fingerPosition, Time.deltaTime * 10);
				}
			}
		}
	}


	Vector3 calcFingerPosition(uint fingerId, Vector3 netData) {
		if (fingerId >= fingerRoots.Length)
		return new Vector3 (0.0f, 0.0f, 0.0f);
		return netData * fingerLengths [fingerId] + fingerRoots[fingerId].transform.localPosition;
	}

}
