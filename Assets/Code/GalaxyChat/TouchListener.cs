using UnityEngine;
using System.Collections;


public class TouchListener : MonoBehaviour {

	public Camera usedCamera;
	
	private bool isTouching, isUpped, isPressed;
	private int touchCounter = 0;

	private Vector2 screenTouchPoint = new Vector2();
	
	private Vector2 touchPoint = new Vector2();
	private Vector2 touchPointPressed = new Vector2();
	private Vector2 touchPointUpped = new Vector2();
	private Vector2 touchPointStored = new Vector2();
	private Vector2 touchDelta = new Vector2();

	private Touch[] touches;

	private float touchTimer;

	void Start () {
	}

	void Update () {
		
		isPressed = false;
		bool storedState = isTouching;
		// mouse
		//#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBPLAYER
		if (!isTouching && Input.GetMouseButtonDown (0)) {
			isPressed = true;
			isTouching = true;
			touchPoint = usedCamera.ScreenToWorldPoint (Input.mousePosition);
			screenTouchPoint = Input.mousePosition;
		} else if (Input.GetMouseButtonUp (0)) {
			isTouching = false;
		} else if (isTouching) {
			touchPoint = usedCamera.ScreenToWorldPoint (Input.mousePosition);
			screenTouchPoint = Input.mousePosition;
		}
		//#else
		// fingers
		if (Input.touchCount > 0) {
			touches = Input.touches;
			isTouching = true;
			Touch t = Input.GetTouch(0);
			if (t.phase == TouchPhase.Began) {
				isPressed = true;
				touchPoint = usedCamera.ScreenToWorldPoint(touches[0].position);
				screenTouchPoint = touches[0].position;
			} else if (t.phase == TouchPhase.Moved) {
				touchPoint = usedCamera.ScreenToWorldPoint(touches[0].position);
				screenTouchPoint = touches[0].position;
			} else if (t.phase == TouchPhase.Ended) {
				isTouching = false;
			}
		}
		//#endif
		//
		if (isTouching != storedState) {
			touchPointStored.Set(touchPoint.x, touchPoint.y);
			if (isTouching) {
				touchPointPressed.Set(touchPoint.x, touchPoint.y);
			} else {
				touchPointUpped.Set(touchPoint.x, touchPoint.y);
			}
			isUpped = !isTouching;
		}

		if (isTouching) {
			touchCounter = (++touchCounter)%5;
			if (touchCounter == 0) {
				touchDelta = touchPoint-touchPointStored;
				touchPointStored.Set(touchPoint.x, touchPoint.y);
			}
		}

	}

	public bool IsUpped {
		get {
			bool v = isUpped;
			isUpped = false;
			return v;
		}
	}

	public bool IsPressed {
		get { return isPressed; }
	}

	public bool IsDowned {
		get { return isTouching; }
	}

	public float X {
		get { return touchPoint.x; }
	}

	public float Y {
		get { return touchPoint.y; }
	}

	public Vector2 Pos {
		get { return touchPoint; }
	}

	public Vector2 ScreenPos {
		get { return screenTouchPoint; }
	}
	
}
