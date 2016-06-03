using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class MenuItem {
	public string Text, Action;
	public GameObject obj;
}

public class MenuActions : MonoBehaviour {

	private static MenuActions Current;
	
	public Text textTitle;
	public Transform itemsArea;
	
	public delegate void ClickItemFunc(MenuItem item);
	public ClickItemFunc OnItemClick;
	
	List<MenuItem> items = new List<MenuItem>();
	int associatedUserId;
	
	void Awake(){
		Current = this;
		DismissLocal();// прячем
	}
	
	public static void AddItem (string text, string action) {
		var i = new MenuItem{
			Text = text, 
			Action = action
		};
		Current.items.Add(i);
	}
	
	public static void Show (int associatedUserId, string userName) {
		Current.ShowLocal(associatedUserId, userName);
	}
	
	public void ShowLocal (int associatedUserId, string userName) {
		this.associatedUserId = associatedUserId;
		var s = string.Format("Actions on {0}", userName);
		textTitle.text = s;
		gameObject.SetActive(true);
	}
	
	public static void Dismiss () {
		Current.DismissLocal();
	}
	
	public void DismissLocal () {
		gameObject.SetActive(false);
	}
	
	public static void Clear () {
		var list = Current.items;
		foreach (var item in list){
			Destroy(item.obj);
		}
		list.Clear();
	}
	
	public static void Prepare () {
		var list = Current.items;
		var prefab = Prefabs.Me.MenuItem;
		foreach (var item in list){
			var go = Instantiate(prefab) as GameObject;
			go.transform.SetParent(Current.itemsArea);
			go.transform.localScale = Vector3.one;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localPosition = Vector3.zero;
			var txt = go.transform.Find("Text").GetComponent<Text>();
			txt.text = item.Text;
		}
	}
	
}
