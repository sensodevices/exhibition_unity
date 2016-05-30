using UnityEngine;

public class CameraHelper : MonoBehaviour {

	public Camera cam;
	Vector2 prev;
	Vector3 startRot;
	
	void Start(){
		startRot = cam.transform.localRotation.eulerAngles;
	}
	
	public void StartMove(Vector2 point) {
		prev = point;
	}
	
	public void Move (Vector2 newPoint) {
		if (prev == null){
			prev = newPoint;
			return;
		}
		var dx = -(newPoint.x-prev.x)*0.01f;
		transform.Translate(Vector3.right*dx);
		prev = newPoint;
	}
	
	void Update(){
		var L = Input.GetKey(KeyCode.A);
		var R = Input.GetKey(KeyCode.D);
		var U = Input.GetKey(KeyCode.W);
		var D = Input.GetKey(KeyCode.S);
		float dx=0, dy=0;
		if (L)
			dx = -1;
		else if (R)
			dx = 1;
		else if (U)
			dy = 1;
		else if (D)
			dy = -1;
		var k = Time.deltaTime * 20f;  
		dx *= k;
		dy *= k;
		var euler = new Vector3(-dy, dx, 0);
		cam.transform.Rotate(euler, Space.Self);
		
		// сброс на начальные значения
		if (Input.GetKeyUp(KeyCode.R)){
			cam.transform.localRotation = Quaternion.Euler(startRot);
		}
	}
}
