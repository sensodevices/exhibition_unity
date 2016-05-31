using UnityEngine;

public class Chat3d : MonoBehaviour {

	public RenderTexture renderTexture;
	public Camera cam;
	
	void Start () {
		var mesh = GetComponent<MeshRenderer>();
		mesh.material.mainTexture = renderTexture;
	}
	
	void Update () {
	
	}
}
