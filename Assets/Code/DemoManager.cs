using UnityEngine;
using System.Collections;

public class DemoManager : MonoBehaviour {

	public Transform VRCamera;

	public GameObject SensoHandsPrefab;
	public Vector3 SensoHandsPosition;
	public Vector3 SensoHandsRotation;

	public GameObject ShapesPrefab;
	public Vector3 ShapesPosition;
	public Vector3 ShapesRotation;

	public GameObject BubblesPrefab;
	public Vector3 BubblesPosition;
	public Vector3 BubblesRotation;

	public GameObject MakeAPotPrefab;
	public Vector3 MakeAPotPosition;
	public Vector3 MakeAPotRotation;

	// Use this for initialization
	void Start () {
		if (SensoHandsPrefab != null)
		{
			GameObject sensoMan = (GameObject)Instantiate(SensoHandsPrefab, SensoHandsPosition, Quaternion.Euler(SensoHandsRotation));
			if (sensoMan != null && VRCamera != null) {
				var netMan = sensoMan.GetComponent<NetworkManager>();
				netMan.VRCamera = VRCamera;
			}
		}

		if (ShapesPrefab != null)
		{
			Instantiate(ShapesPrefab, ShapesPosition, Quaternion.Euler(ShapesRotation));
		}

		if (BubblesPrefab != null)
		{
			Instantiate(BubblesPrefab, BubblesPosition, Quaternion.Euler(BubblesRotation));
		}

		if (MakeAPotPrefab != null)
		{
			Instantiate(MakeAPotPrefab, MakeAPotPosition, Quaternion.Euler(MakeAPotRotation));
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
