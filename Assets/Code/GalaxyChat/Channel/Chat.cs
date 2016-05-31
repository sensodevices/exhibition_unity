using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Chat : MonoBehaviour {

	public Transform chatArea, chatTitle;
	public ScrollRect scrollRect;
	
	public void AddMessage(string nick, string message){
		
		var v = scrollRect.verticalNormalizedPosition;
		var scrollToEnd = (v < 0.01f);
		
		var go = Instantiate(Prefabs.Current.MsgItem) as GameObject;
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
