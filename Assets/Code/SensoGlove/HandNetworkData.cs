using UnityEngine;
using SimpleJSON;
using System;
using System.Collections;

public class HandNetworkData {

	public class SingleHandData {
		public Vector3 palmPosition;
		public Vector3 palmRotation;
		public Vector3 wristRotation;
		public Vector3[] fingerPositions = new Vector3[5];
	}

	public enum DataType: int {
		LeftHand, RightHand
	}

	public SingleHandData leftHand;
	public SingleHandData rightHand;

	public HandNetworkData() {
		leftHand = new SingleHandData ();
		rightHand = new SingleHandData ();
	}

	public void ParseRawData(ref Byte[] data, int offset) {
		int start = offset;
		float x,y,z;
		x = BitConverter.ToSingle(data, start) * Mathf.Rad2Deg; start += 4;
		y = BitConverter.ToSingle(data, start) * Mathf.Rad2Deg; start += 4;
		z = BitConverter.ToSingle(data, start) * Mathf.Rad2Deg; start += 4; 
		rightHand.palmRotation = new Vector3(x, y, z);
		
		x = BitConverter.ToSingle(data, start); start += 4;
		z = BitConverter.ToSingle(data, start); start += 4;
		y = BitConverter.ToSingle(data, start); start += 4;
		rightHand.palmPosition = new Vector3(x, y, z);
		
		for (int i = 0; i < 5; ++i) {
			x = BitConverter.ToSingle(data, start); start += 4;
			z = BitConverter.ToSingle(data, start); start += 4;
			y = BitConverter.ToSingle(data, start); start += 4;
			rightHand.fingerPositions[i] = new Vector3(x, y, z);
		}
	}

	/**
	 * Deserializes hand data
	 */ 
	public void DeserializeHand (HandNetworkData.DataType type, JSONClass aData) {
		SingleHandData netData = type == DataType.LeftHand ? leftHand : rightHand;
		float alpha, beta, gamma;

		// Palm position
		netData.palmPosition = new Vector3(aData ["x"].AsFloat, aData["y"].AsFloat, aData["z"].AsFloat);
	
		// Palm rotation
		alpha = aData ["alpha"].AsFloat * Mathf.Rad2Deg;
		if (alpha < 0.0f) alpha += 360.0f;
		beta = aData ["beta"].AsFloat * Mathf.Rad2Deg;
		if (beta < 0.0f) beta += 360.0f;
		gamma = aData ["gamma"].AsFloat * Mathf.Rad2Deg;
		if (gamma < 0.0f) gamma += 360.0f;
		netData.palmRotation = new Vector3(alpha, beta, gamma);
		
		// Wrist rotation
		if (aData.KeyExists("wa"))
		{
			alpha = aData ["wa"].AsFloat * Mathf.Rad2Deg;
			if (alpha < 0.0f) alpha += 360.0f;
			beta = aData ["wb"].AsFloat * Mathf.Rad2Deg;
			if (beta < 0.0f) beta += 360.0f;
			gamma = aData ["wg"].AsFloat * Mathf.Rad2Deg;
			if (gamma < 0.0f) gamma += 360.0f;
			netData.wristRotation = new Vector3(alpha, beta, gamma);
		}
		else 
		{
			netData.wristRotation = new Vector3(0.0f, 0.0f, 0.0f);
		}
		
		//Fingers
		var fingersArr = aData ["fingers"].AsArray;
		float x, y, z;
		bool isTest = false;
		float testx = 0.0f, testy = 0.0f, testz = -1.0f;

		for (int i = 0; i < 5; ++i) {
			var aFinger = fingersArr[i].AsObject;
			if (!isTest) {
				x = -aFinger["x"].AsFloat;
				y = aFinger ["z"].AsFloat;
				z = aFinger ["y"].AsFloat;
			} else {
				x = -testx;
				y = testz;
				z = testy;
			}
			netData.fingerPositions[i] = new Vector3(x, y, z);
		}
	}
}
