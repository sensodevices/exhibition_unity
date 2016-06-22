using UnityEngine;
using System;
using System.Collections;

public class HandNetworkData {

	public class SingleHandData {
		public Vector3 palmPosition;
		public Vector3 palmRotation;
		public Vector3 wristRotation;
		public Vector3[] fingerPositions = new Vector3[5];

		public void setNewData(SingleHandData aData) {
			palmPosition = aData.palmPosition;
			palmRotation = aData.palmRotation;
			wristRotation = aData.wristRotation;
			for (int i = 0; i < 5; ++i) {
				fingerPositions[i] = aData.fingerPositions[i];
			}
		}
	}

	public enum DataType: int {
		RightHand, LeftHand
	}

	public enum FingerType: int {
		Thumb, Index, Middle, Ring, Pinky
	}

	public SingleHandData leftHand;
	public SingleHandData rightHand;

	public HandNetworkData() {
		leftHand = new SingleHandData ();
		rightHand = new SingleHandData ();
	}

	public void ParseRawData(ref Byte[] data, int offset) {
		DataType handType = (DataType)data[offset];
		SingleHandData setData = new SingleHandData();

		int start = offset + 1;
		float x,y,z;
		x = BitConverter.ToSingle(data, start) * Mathf.Rad2Deg; start += 4;
		y = BitConverter.ToSingle(data, start) * Mathf.Rad2Deg; start += 4;
		z = BitConverter.ToSingle(data, start) * Mathf.Rad2Deg; start += 4;
		if (Single.IsNaN(x) || Single.IsNaN(y) || Single.IsNaN(z)) return;
		setData.palmPosition = new Vector3(x, y, z);

		x = BitConverter.ToSingle(data, start); start += 4;
		x *= Mathf.Rad2Deg; if (x < 0.0f) x += 360.0f;
		y = BitConverter.ToSingle(data, start); start += 4;
		y *= Mathf.Rad2Deg; if (z < 0.0f) z += 360.0f;
		z = BitConverter.ToSingle(data, start); start += 4;
		z *= Mathf.Rad2Deg; if (y < 0.0f) y += 360.0f;
		if (Single.IsNaN(x) || Single.IsNaN(y) || Single.IsNaN(z)) return;
		setData.palmRotation = new Vector3(x, y, z);

		x = BitConverter.ToSingle(data, start); start += 4;
		z = BitConverter.ToSingle(data, start); start += 4;
		y = BitConverter.ToSingle(data, start); start += 4;
		if (Single.IsNaN(x) || Single.IsNaN(y) || Single.IsNaN(z)) return;
		setData.wristRotation = new Vector3(x, y, z);

		for (int i = 0; i < 5; ++i) {
			x = BitConverter.ToSingle(data, start); start += 4;
			z = BitConverter.ToSingle(data, start); start += 4;
			y = BitConverter.ToSingle(data, start); start += 4;
			if (Single.IsNaN(x) || Single.IsNaN(y) || Single.IsNaN(z)) return;
			setData.fingerPositions[i] = new Vector3(x, y, z);
		}

		if (handType == DataType.RightHand) 
			rightHand.setNewData(setData);
		else
			leftHand.setNewData(setData);
	}

}
