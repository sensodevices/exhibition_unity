using UnityEngine;
using System.Collections;

public class HandManager : MonoBehaviour {

	public NetworkManager netMan;

	public GameObject[] fingerTargets;
	public GameObject[] fingerRoots;
	
	public Transform wrist;
	public Transform targetContainer;

	private float netDataFactor = 0.5f; //0.015f;
	private Vector3 palmPositionCorrection = new Vector3 (0.0f, 0.0f, 0.0f);
	private Vector3 palmAnglesCorrection = new Vector3 (0.0f, 0.0f, 0.0f);
	private Vector3 wristAnglesCorrection = new Vector3 (0.0f, 0.0f, 0.0f);

	private Vector3 startHandPosition;
	// private Vector3 moveToPosition;

	public HandNetworkData.DataType handType;

	private bool needReset = true;

	private float[] fingerLengths = {0, 0, 0, 0, 0};


	// Use this for initialization
	void Start ()
  {
		startHandPosition = new Vector3 (this.transform.localPosition.x, this.transform.localPosition.y, this.transform.localPosition.z);
		// moveToPosition = new Vector3 (this.transform.localPosition.x, this.transform.localPosition.y, this.transform.localPosition.z);

		for (int i = 0; i < fingerRoots.Length; ++i) {
			fingerLengths [i] = Vector3.Distance (fingerRoots [i].transform.localPosition, fingerTargets[i].transform.localPosition);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Hand position
		HandNetworkData.SingleHandData aData = netMan.GetHandData(handType);
			
		if (aData != null) {
			if (Input.GetKeyDown("space")) {
				needReset = true;
			}

			Vector3 palmPosition = aData.palmPosition;
			if (needReset) {
				palmPositionCorrection = startHandPosition - palmPosition * netDataFactor;
			}
			palmPosition = palmPosition * netDataFactor + palmPositionCorrection;
			if (this.transform.localPosition != palmPosition) {
				this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, palmPosition, Time.deltaTime * 10);
			}
			
			// Rotation
			// X   Z
			// | /
			// . - Y
			Vector3 palmRotation = aData.palmRotation;
			palmRotation += palmAnglesCorrection;

			this.transform.eulerAngles = new Vector3(0, 0, 0);
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
				Vector3 fingerPosition = calcFingerPosition ((uint)i, aData.fingerPositions[i]);
				if (fingerPosition != fingerTargets[i].transform.localPosition) {
					fingerTargets[i].transform.localPosition = Vector3.Lerp(fingerTargets[i].transform.localPosition, fingerPosition, Time.deltaTime * 10);
				}
			}

			needReset = false;
		}
	}


	Vector3 calcFingerPosition (uint fingerId, Vector3 netData) {
		if (fingerId >= fingerRoots.Length)
			return new Vector3 (0.0f, 0.0f, 0.0f);
		return netData * fingerLengths [fingerId] + fingerRoots[fingerId].transform.localPosition;
	}

}
