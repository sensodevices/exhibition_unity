using UnityEngine;
using System.Collections.Generic;

public class GlobalFuncs : MonoBehaviour {
	
	public GameObject Cross;

    static private string[] fingerTags = {
        "ThumbFinger", "IndexFinger", "MiddleFinger", "RingFinger", "PinkyFinger"     
    };

	public static GlobalFuncs Me {get; private set;}
	
	void Awake () {
		Me = this;
	}
	
	public static void ToggleCross(bool toggle) {
		Me.Cross.active = true;
	}

    public static FingerTarget GetFinger(HandNetworkData.DataType handType, HandNetworkData.FingerType finger) {
        var fingers = GameObject.FindGameObjectsWithTag(fingerTags[(int)finger]);
        foreach (var f in fingers)
        {
            var trg = f.GetComponent<FingerTarget>();
            if (trg != null && trg.HandType == handType) {
                return trg;
            }
        }
        return null;
    }
	
	
}
