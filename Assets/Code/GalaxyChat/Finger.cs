using UnityEngine;


public enum ColType{
	Planet, Chat, Pers, Undefined
}


public class Finger : MonoBehaviour {

	public float speed = 1;

	public bool IsPlanetCollision {get;private set;}
	public bool IsChatCollision {get;private set;}
	
	public delegate void MoveFunc(float dx, float dy);
	public MoveFunc OnSwipeChat, OnSwipePlanet;
	public delegate void CollisionFunc(ColType type);
	public CollisionFunc OnExitCollision, OnEnterCollision;
	
	bool isPressed;
	float posZ;
	
	void Start () {
		posZ = transform.localPosition.z;
	}
	
	void Update () {
		
		if (Input.GetMouseButtonDown(0)){
			print("mouse: "+Input.mousePosition);
		}
		
		var xl = Input.GetKey(KeyCode.LeftArrow);
		var xr = Input.GetKey(KeyCode.RightArrow);
		var yu = Input.GetKey(KeyCode.UpArrow);
		var yd = Input.GetKey(KeyCode.DownArrow);
		var pressed = Input.GetKey(KeyCode.X);
		float dx=0, dy=0, dz=0;
		
		if (pressed != isPressed){
			if (pressed){
				posZ = transform.localPosition.z;
				
				var pos = Camera.main.WorldToScreenPoint(transform.position);
				pos.z = -10000;
				print("finger at: "+pos);
				/*var ray = Camera.main.ScreenPointToRay(pos);
				RaycastHit hit;
				var res = Physics.RaycastAll(ray);
				if (res.Length > 0){
					foreach (var h in res){
						print("hit: "+h.collider.name);
					}
				}*/
				var hit2d = Physics2D.Raycast(pos, Vector2.zero);
				if (hit2d.collider != null){
					print("hit2d: "+hit2d.collider.name);
				}
		
			} else {
				var pp = transform.localPosition;
				pp.z = posZ;
				transform.position = pp;
			}
			isPressed = pressed;
		}
		
		dz = pressed ? 0.15f : 0;
		//dz=0;
		
		if (xl)
			dx = -1;
		else if (xr)
			dx = 1;
		else if (yu)
			dy = 1;
		else if (yd)
			dy = -1;
		var k = Time.deltaTime * speed;  
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
		ColType type;
		if (t == "PlanetCollider"){
			IsPlanetCollision = true;
			type = ColType.Planet;
		} else if (t == "ChatCollider"){
			IsChatCollision = true;
			type = ColType.Chat;
		} else if (t == "PersCollider"){
			type = ColType.Pers;
		} else {
			type = ColType.Undefined;
		}
		if (OnEnterCollision != null)
			OnEnterCollision(type);
    }
	
	void OnTriggerExit(Collider other) {
        var t = other.tag;
		ColType type;
		if (t == "PlanetCollider"){
			IsPlanetCollision = false;
			type = ColType.Planet;
		} else if (t == "ChatCollider"){
			IsChatCollision = false;
			type = ColType.Chat;
		} else if (t == "PersCollider"){
			type = ColType.Pers;
		} else {
			type = ColType.Undefined;
		}
		if (OnExitCollision != null)
			OnExitCollision(type);
    }

}
