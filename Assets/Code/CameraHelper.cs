using UnityEngine;

public class CameraHelper : MonoBehaviour {

	Vector2 prev;
	
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
	
}
