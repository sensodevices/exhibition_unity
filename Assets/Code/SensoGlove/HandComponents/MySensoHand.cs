using UnityEngine;
using System.Collections.Generic;

public class MySensoHand : SensoHand {

    public Transform ClaviclePivot;
    public Transform ShoulderPivot;
    public Transform ElbowPivot;
	public Transform WristPivot;

	public Transform[] thumbBones;
	public Transform[] indexBones;
	public Transform[] middleBones;
	public Transform[] thirdBones;
	public Transform[] littleBones;
	
	void Start ()
	{
		base.Start();
	}

	void Update ()
	{
		if (sampleChanged) {
			SetSensoPose(latestSample);
			sampleChanged = false;
		}
	}

    public void SetSensoPose (SensoHandData aData)
	{
        if (ClaviclePivot != null && aData.hasClavicle)
        {
            ClaviclePivot.localRotation = aData.clavicleRotation;
        }
        if (ShoulderPivot != null && aData.hasShoulder)
        {
            ShoulderPivot.localRotation = aData.shoulderRotation;
        }
        ElbowPivot.localRotation = aData.wristRotation;
        
		// Wrist rotation
		Quaternion wq = new Quaternion((HandType == ESensoPositionType.RightHand ? -1 : 1) * aData.wristRotation.z, (HandType == ESensoPositionType.RightHand ? 1 : -1) * aData.wristRotation.y, aData.wristRotation.x, aData.wristRotation.w);
        Quaternion pq = new Quaternion((HandType == ESensoPositionType.RightHand ? -1 : 1) * aData.palmRotation.z, (HandType == ESensoPositionType.RightHand ? 1 : -1) * aData.palmRotation.y, aData.palmRotation.x, aData.palmRotation.w);
        WristPivot.localRotation = (Quaternion.Inverse(wq) * pq);
        
		//Fingers
		setFingerBones(ref thumbBones, aData.fingerAngles[0], ESensoFingerType.Thumb);
		setFingerBones(ref indexBones, aData.fingerAngles[1], ESensoFingerType.Index);
		setFingerBones(ref middleBones, aData.fingerAngles[2], ESensoFingerType.Middle);
		setFingerBones(ref thirdBones, aData.fingerAngles[3], ESensoFingerType.Third);
		setFingerBones(ref littleBones, aData.fingerAngles[4], ESensoFingerType.Little);
	}

	private static void setFingerBones(ref Transform[] bones, Vector3 angles, ESensoFingerType fingerType)
	{
		if (fingerType == ESensoFingerType.Thumb) setThumbBones(ref bones, ref angles);
		else {
			if (angles.z < 0.0f) {
				bones[0].localEulerAngles = angles;
			} else {
				angles.z /= 2.0f;
				for (int j = 0; j < bones.Length; ++j) {
					bones[j].localEulerAngles = angles;
					if (j == 0) angles.y = 0.0f;
				}
			}
			if (ESensoFingerType.Little == fingerType) {
				bones[0].Rotate(new Vector3(0.0f, -15.0f, 0));
			}
		}
	}

	private static void setThumbBones(ref Transform[] bones, ref Vector3 angles) {
		bones[0].localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
		float t = angles.y;
		angles.y = -angles.z;
		angles.z = t;
		
		angles.z += 30.0f;
		bones[0].Rotate(angles);
	}
}
