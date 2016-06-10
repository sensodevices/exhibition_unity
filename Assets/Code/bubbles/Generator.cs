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
	
	// Update is called once per frame
	void Update () {
		if (spawnedObject == null) {
			var newInd = numberGenerator.Next(spawnableObjects.Length);
			spawnedObject = (GameObject)Instantiate(spawnableObjects[newInd], spawnPosition, spawnRotation);
			spawnedObject.transform.parent = transform;
		}
	}
}
