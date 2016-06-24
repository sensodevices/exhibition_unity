using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


/* Item */

public class MenuItem {
	public string Text, Action;
	public GameObject obj;
}


/* Menu */

public class MenuActions : MonoBehaviour {

	public static MenuActions Me {get; private set;}
	
	public Text textTitle;
	public Transform itemsArea;
	public ScrollRect scrollRect;

	public delegate void ClickItemFunc(MenuItem item);
	public ClickItemFunc OnItemClick;
	
	public int AssociatedUserId {get;private set;}

	List<MenuItem> items = new List<MenuItem>();
	
	
	void Awake(){
		Me = this;
		DismissLocal();// прячем
	}
	
	void Update(){
		var dir = 0f; 
		if (Input.GetKey(KeyCode.UpArrow)){
			dir = 0.1f;
		} else if (Input.GetKey(KeyCode.DownArrow)){
			dir = -0.1f;
		}
		scrollRect.verticalNormalizedPosition += dir*Time.deltaTime;
	}

	public static void AddItem (string text, string action) {
		var i = new MenuItem{
			Text = text, 
			Action = action
		};
		Me.items.Add(i);
	}
	
	public static void Show (int associatedUserId, string userName) {
		Me.ShowLocal(associatedUserId, userName);

		var indexFinger = GlobalFuncs.GetFinger(HandNetworkData.DataType.RightHand, HandNetworkData.FingerType.Index);
		if (indexFinger != null) {
			Debug.Log("index finger found");
		}
	}
	
	public void ShowLocal (int associatedUserId, string userName) {
		AssociatedUserId = associatedUserId;
		var s = string.Format("Actions on {0}", userName);
		textTitle.text = s;
		gameObject.SetActive(true);
	}
	
	public static void Dismiss () {
		Me.DismissLocal();
	}
	
	public void DismissLocal () {
		gameObject.SetActive(false);
	}
	
	public static void Clear () {
		var list = Me.items;
		foreach (var item in list){
			Destroy(item.obj);
		}
		list.Clear();
	}
	
	public static void Prepare () {
		var list = Me.items;
		var prefab = Prefabs.Me.MenuItem;
		foreach (var item in list){
			var go = Instantiate(prefab) as GameObject;
			go.transform.SetParent(Me.itemsArea);
			go.transform.localScale = Vector3.one;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localPosition = Vector3.zero;
			var txt = go.transform.Find("Text").GetComponent<Text>();
			txt.text = item.Text;
			// клик
			var btn = go.GetComponent<Button>();
			var i = item;
			btn.onClick.AddListener( () => {
				Me.InvokeItemClick(i);
				
			} );
		}
	}
	
	void InvokeItemClick(MenuItem item){
		if (OnItemClick != null)
			OnItemClick(item);
	}
}
