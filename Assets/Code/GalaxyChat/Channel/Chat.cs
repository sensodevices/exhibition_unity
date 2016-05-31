using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Chat : MonoBehaviour {

	public Transform chatArea, chatTitle;
	public ScrollRect scrollRect;
	public int historySize = 30;
	
	List<GameObject> items = new List<GameObject>();
	
	public void AddMessage(string nick, string message){
		
		GameObject go;
		
		if (items.Count == historySize){// первое сообщение ставим в конец
			go = items[0];
			go.transform.SetParent(null);
			items.RemoveAt(0);
			//Destroy(i);
			print("reuse chat item");
		} else { // если не достигли истории, то создаём
			go = Instantiate(Prefabs.Current.MsgItem) as GameObject;
		}
		
		var v = scrollRect.verticalNormalizedPosition;
		var scrollToEnd = (v < 0.01f);
		
		items.Add(go);
		go.transform.SetParent(chatArea);
		go.transform.localScale = Vector3.one;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localPosition = Vector3.zero;
		var txt = go.transform.Find("Text").GetComponent<Text>();
		txt.text = message;
		txt = go.transform.Find("Nick").GetComponent<Text>();
		if (nick != null)
			txt.text = nick;
		else
			txt.gameObject.SetActive(false);
			
		if (scrollToEnd){
			// если скроллим сразу после добавления элемента, то ничего не происходит
			// нужно подождать немного
			Invoke("ScrollChatToEnd", 0.1f);
		}
	}
		
	void ScrollChatToEnd(){
		StartCoroutine(ScrollChatToEndInternal());
	}
	
	IEnumerator ScrollChatToEndInternal(){
		var v = scrollRect.verticalNormalizedPosition;
		var step = v/10f;
		while (scrollRect.verticalNormalizedPosition > 0){
			yield return new WaitForSeconds(0.03f);
			scrollRect.verticalNormalizedPosition -= step; 
		}
		scrollRect.verticalNormalizedPosition = 0;
		yield break;
	}
	
	public void Scroll(float dx, float dy){
		scrollRect.verticalScrollbar.value -= 0.5f*dy;
	}
	
}
