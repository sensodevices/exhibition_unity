using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {

	public Text textTitle, textContent;
	public GameObject showHideMe;
	
	private static MessageBox Current;
	
	void Awake(){
		Current = this;
		DismissLocal();// прячем
	}
	
	public static void Show (string title, string content) {
		Current.ShowLocal(title, content);
	}
	
	public void ShowLocal (string title, string content) {
		textTitle.text = title;
		textContent.text = content;
		gameObject.SetActive(true);
		if (showHideMe != null)
			showHideMe.SetActive(false);
	}
	
	public static void Dismiss () {
		Current.DismissLocal();
	}
	
	public void DismissLocal () {
		gameObject.SetActive(false);
		if (showHideMe != null)
			showHideMe.SetActive(true);
	}
}
