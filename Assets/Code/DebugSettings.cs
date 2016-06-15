using UnityEngine;

public class DebugSettings : MonoBehaviour {

	public static DebugSettings Me {get; private set;}

	public bool DebugForGalaxy;
	public bool DebugForSenso;
	public bool DebugForPot;

	void Awake () {
		Me = this;
	}
		
}
