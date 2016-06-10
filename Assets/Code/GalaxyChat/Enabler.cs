using UnityEngine;

public class Enabler : MonoBehaviour {

	public GameObject[] Array;
	public bool active = true;

	void Awake () {
	
		if (Array != null) {
			foreach (var i in Array) {
				if (i != null)
					i.SetActive(active);
			}
		}

	}

}
