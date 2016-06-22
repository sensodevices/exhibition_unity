using UnityEngine;
using UnityEngine.UI;
using System;

public class MessageBox : MonoBehaviour {

	public Text textTitle, textContent;
	public GameObject showHideMe;
	public Button buttonSpecial;

	private static MessageBox Me;
	
	Text buttonSpecialText;

	void Awake(){
		Me = this;
		buttonSpecialText = buttonSpecial.transform.Find("Text").GetComponent<Text>();
		DismissLocal();// прячем
	}
	
	public static void Show (string title, string content) {
		Me.ShowLocal(title, content);
	}
	
	public static void EnableSpecialButton (string text, Action OnClick) {
		Me.buttonSpecialText.text = text;
		Me.buttonSpecial.onClick.AddListener(()=>OnClick());
		Me.buttonSpecial.gameObject.SetActive(true);
	}

	public void ShowLocal (string title, string content) {
		textTitle.text = title;
		textContent.text = content;
		gameObject.SetActive(true);
		if (showHideMe != null)
			showHideMe.SetActive(false);
	}
	
	public static void Dismiss () {
		Me.DismissLocal();
	}
	
	public void DismissLocal () {
		gameObject.SetActive(false);
		if (showHideMe != null)
			showHideMe.SetActive(true);
		buttonSpecial.gameObject.SetActive(false);
	}
}
