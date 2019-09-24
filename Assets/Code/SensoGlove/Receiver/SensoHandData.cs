using UnityEngine;
using SimpleJSON;

///
/// @brief Enumeration for all Senso position types
public enum ESensoPositionType
{
	Unknown, RightHand, LeftHand, PositionsCount
};

///
/// @brief Enumeration for fingers
public enum ESensoFingerType
{
	Thumb, Index, Middle, Third, Little
};

///
/// @brief Implements a container for Senso hand pose information.
///
public class SensoHandData {
	public Vector3 palmPosition;
	public Quaternion palmRotation;
	public Quaternion wristRotation;
	public Vector3[] fingerAngles = new Vector3[5];

    public Quaternion shoulderRotation;
    public bool hasShoulder;
    public Quaternion clavicleRotation;
    public bool hasClavicle;

    ///
    /// @brief Default constructor
    ///
    public SensoHandData () {
        palmRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        wristRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        for (int i = 0; i < 5; ++i)
            fingerAngles[i] = new Vector3(0.0f, 0.0f, 0.0f);

        hasShoulder = false;
        shoulderRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        hasClavicle = false;
        clavicleRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
    }

	///
	/// @brief Copy constructor
	///
	public SensoHandData (SensoHandData old)
    {
		palmPosition = old.palmPosition;
		palmRotation = old.palmRotation;
		wristRotation = old.wristRotation;
		for (int i = 0; i < 5; ++i) 
			fingerAngles[i] = old.fingerAngles[i];

        hasShoulder = old.hasShoulder;
        if (hasShoulder) shoulderRotation = old.shoulderRotation;

        hasClavicle = old.hasClavicle;
        if (hasClavicle) clavicleRotation = old.clavicleRotation;
    }

	///
	/// @brief Parses JSON node into internal properties
	///
	public void parseJSONNode (JSONNode data)
    {
		JSONArray anArr;

		// Wrist parsing
		anArr = data["wrist"]["quat"].AsArray;
		arrToQuat(ref anArr, ref wristRotation);

		// Palm parsing
		var palmNode = data["palm"];
		anArr = palmNode["quat"].AsArray;
		arrToQuat(ref anArr, ref palmRotation);
		anArr = palmNode["pos"].AsArray;
		arrToVec3(ref anArr, ref palmPosition);
        
		// Fingers parsing
		JSONArray fingersJArr = data["fingers"].AsArray;
        float lastZ = fingerAngles[0].z;
        for (int i = 0; i < 5; ++i) {
			anArr = fingersJArr[i]["ang"].AsArray;
			arrToFingerAngles(ref anArr, ref fingerAngles[i]);
		}

        // Shoulder parsing
        if (data.KeyExists("shoulder"))
        {
            var shoulderNode = data["shoulder"];
            anArr = shoulderNode["quat"].AsArray;
            arrToQuat(ref anArr, ref shoulderRotation);
            hasShoulder = true;
        }
        else hasShoulder = false;

        // Clavicle parsing
        if (data.KeyExists("clavicle"))
        {
            var clavicleNode = data["clavicle"];
            anArr = clavicleNode["quat"].AsArray;
            arrToQuat(ref anArr, ref clavicleRotation);
            hasClavicle = true;
        }
        else hasClavicle = false;
    }

	static private void arrToQuat (ref JSONArray arr, ref Quaternion quat) {
		quat.w = arr[0].AsFloat;
		quat.x = arr[1].AsFloat;
		quat.y = -arr[3].AsFloat;
		quat.z = arr[2].AsFloat;
	}
	static private void arrToVec3 (ref JSONArray arr, ref Vector3 vec) {
		vec.x = arr[0].AsFloat;
		vec.y = arr[2].AsFloat;
		vec.z = arr[1].AsFloat;
	}

	static private void arrToFingerAngles (ref JSONArray arr, ref Vector3 angles) {
		angles.x = 0.0f;
		angles.y = arr[1].AsFloat * Mathf.Rad2Deg;
        angles.z = arr[0].AsFloat * Mathf.Rad2Deg;
	}
}