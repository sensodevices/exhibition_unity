using UnityEngine;
using System.Collections.Generic;

public class GlobalFuncs : MonoBehaviour {
	
	public GameObject Cross;

	public static GlobalFuncs Me {get; private set;}
	
	void Awake () {
		Me = this;
	}
	
	public static void ToggleCross(bool toggle) {
        if (Me != null) 
            Me.Cross.SetActive(toggle);
	}
	
	
}
