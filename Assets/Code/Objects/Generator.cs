using UnityEngine;
using System.Collections;

public class Generator : MonoBehaviour {
	public GameObject[] spawnableObjects;
	public Transform spawnPoint;

	private GameObject spawnedObject;
	private System.Random numberGenerator;
	private Vector3 spawnPosition;
	private Quaternion spawnRotation;

	// Use this for initialization
	void Start () {
		numberGenerator = new System.Random();
		if (spawnPoint != null) {
			spawnPosition = spawnPoint.position;
			spawnRotation = spawnPoint.rotation;
		} else {
			spawnPosition = Vector3.zero;
			spawnRotation = Quaternion.identity;
		}
	}
	

	private bool needReset = false;
	void Update ()
	{
		if (Input.GetKey("r")) {
			needReset = true;
		}
	}









	// Update is called once per frame
	void FixedUpdate () {
		if (spawnedObject == null || needReset) {
			var newInd = numberGenerator.Next(spawnableObjects.Length);
			spawnedObject = (GameObject)Instantiate(spawnableObjects[newInd], spawnPosition, spawnRotation);
			spawnedObject.transform.parent = transform;
			needReset = false;
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject == spawnedObject) {
			spawnedObject = null;
		}
	}
}
