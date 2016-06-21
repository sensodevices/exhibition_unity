using UnityEngine;

public class Chat3d : MonoBehaviour {

	public MeshRenderer meshAsDisplay, meshAsSubDisplay;
	public RenderTexture renderTexture;
	public Camera cam;
	Color defColor;
	// 448491FF
	void Start () {
		meshAsDisplay.material.mainTexture = renderTexture;
		meshAsSubDisplay.material.mainTexture = renderTexture;
		defColor = cam.backgroundColor;
	}
	
	public void EnterCollider(){
		cam.backgroundColor = Color.gray;
	}
	public void ExitCollider(){
		cam.backgroundColor = defColor;
	}
	
}
