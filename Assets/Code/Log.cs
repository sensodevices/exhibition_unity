using UnityEngine;

public class Log : MonoBehaviour {

	public static Log Me {get; private set;}

	public bool EnableGalaxy;
	public bool EnableGlove;
	public bool EnablePot;

	void Awake () {
		Me = this;
	}
	
	public static void Galaxy(string message){
		if (Me.EnableGalaxy)
			Debug.Log(message);
	}
	public static void Glove(string message){
		if (Me.EnableGlove)
			Debug.Log(message);
	}
	public static void Pot(string message){
		if (Me.EnablePot)
			Debug.Log(message);
	}

}
