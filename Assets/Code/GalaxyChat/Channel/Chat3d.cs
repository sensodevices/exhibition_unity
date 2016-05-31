using UnityEngine;

public class Chat3d : MonoBehaviour {

	public RenderTexture renderTexture;
	public Camera cam;
	Color defColor;
	
	void Start () {
		var mesh = GetComponent<MeshRenderer>();
		mesh.material.mainTexture = renderTexture;
		defColor = cam.backgroundColor;
	}
	
	public void EnterCollider(){
		cam.backgroundColor = Color.gray;
	}
	public void ExitCollider(){
		cam.backgroundColor = defColor;
	}
	
}
