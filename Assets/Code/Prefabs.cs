using UnityEngine;

public class Prefabs : MonoBehaviour {
	
	//public GameObject UserNameText;
	public GameObject MsgItem;
	public GameObject UserObj;


	public static Prefabs Current {get;private set;}
	
	void Awake () {
		Current = this;
	}
	
	public static GameObject NewInstantce(GameObject prefab){
		return (Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject);
	}
	
	
}
