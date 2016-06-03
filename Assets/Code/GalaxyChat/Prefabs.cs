using UnityEngine;

public class Prefabs : MonoBehaviour {
	
	public GameObject MenuItem;
	public GameObject MsgItem;
	public GameObject UserObj;


	public static Prefabs Me {get;private set;}
	
	void Awake () {
		Me = this;
	}
	
	public static GameObject NewInstantce(GameObject prefab){
		return (Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject);
	}
	
	
}
