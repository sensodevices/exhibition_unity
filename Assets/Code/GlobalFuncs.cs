using UnityEngine;

public class GlobalFuncs : MonoBehaviour {
	
	public GameObject Cross;


	public static GlobalFuncs Me {get; private set;}
	
	void Awake () {
		Me = this;
	}
	
	public static void ToggleCross(bool toggle) {
		Me.Cross.active = true;
	}

    // public static FingerTarget GetFinger(HandNetworkData.DataType handType, HandNetworkData.FingerType finger) {

    // }
	
	
}
