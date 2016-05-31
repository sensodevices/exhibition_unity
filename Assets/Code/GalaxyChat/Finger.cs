using UnityEngine;

public class Finger : MonoBehaviour {

	public bool IsPlanetCollision {get;private set;}
	public bool IsChatCollision {get;private set;}
	
	public delegate void MoveFunc(float dx, float dy);
	public MoveFunc OnSwipeChat, OnSwipePlanet;
	
	bool isPressed;
	float posZ;
	
	void Start () {
		posZ = transform.localPosition.z;
	}
	
	void Update () {
		var xl = Input.GetKey(KeyCode.LeftArrow);
		var xr = Input.GetKey(KeyCode.RightArrow);
		var yu = Input.GetKey(KeyCode.UpArrow);
		var yd = Input.GetKey(KeyCode.DownArrow);
		var pressed = Input.GetKey(KeyCode.X);
		float dx=0, dy=0, dz=0;
		
		if (pressed != isPressed){
			if (pressed){
				posZ = transform.localPosition.z;
			} else {
				var pp = transform.localPosition;
				pp.z = posZ;
				transform.position = pp;
			}
			isPressed = pressed;
		}
		
		dz = pressed ? 0.5f : 0;
		
		if (xl)
			dx = -1;
		else if (xr)
			dx = 1;
		else if (yu)
			dy = 1;
		else if (yd)
			dy = -1;
		var k = Time.deltaTime * 5f;  
		dx *= k;
		dy *= k;
		var p = transform.localPosition;
		p.x += dx;
		p.y += dy;
		p.z = posZ + dz;
		transform.position = p;
		
		if (IsChatCollision && OnSwipeChat != null){
			OnSwipeChat(dx,dy);
		}
		if (IsPlanetCollision && OnSwipePlanet != null){
			OnSwipePlanet(dx,dy);
		}
	}
	
	void OnTriggerEnter(Collider other) {
        var t = other.tag;
		if (t == "PlanetCollider")
			IsPlanetCollision = true;
		else if (t == "ChatCollider")
			IsChatCollision = true;
    }
	
	void OnTriggerExit(Collider other) {
        var t = other.tag;
		if (t == "PlanetCollider")
			IsPlanetCollision = false;
		else if (t == "ChatCollider")
			IsChatCollision = false;
    }

}
