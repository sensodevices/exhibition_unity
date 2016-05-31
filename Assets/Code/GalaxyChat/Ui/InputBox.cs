using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class InputBox : MonoBehaviour {

	private static InputBox Current;
	
	public Text textTitle, textContent;
	public InputField inputField;
		
	public delegate void ButtonOkFunc(string value);
	public delegate void ButtonCancelFunc();
	public static ButtonOkFunc OnButtonOkPressed;
	public static ButtonCancelFunc OnButtonCancelPressed;
	
	void Awake(){
		Current = this;
		DismissLocal();// прячем
	}
	
	public static void Show (string title, string prompt, string placeholder) {
		Current.textTitle.text = title;
		Current.textContent.text = prompt;
		Current.inputField.placeholder.GetComponent<Text>().text = placeholder;
		Current.gameObject.SetActive(true);
	}
	
	public void PressOk () {
		if (OnButtonOkPressed != null)
			OnButtonOkPressed(inputField.text);
		DismissLocal();
	}
	
	public void PressCancel () {
		if (OnButtonCancelPressed != null)
			OnButtonCancelPressed();
		DismissLocal();
	}
	
	public void DismissLocal () {
		inputField.text = "";
		gameObject.SetActive(false);
	}
}
