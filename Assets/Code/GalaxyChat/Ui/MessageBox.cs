using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {

	public Text textTitle, textContent;
	
	private static MessageBox Current;
	
	void Awake(){
		Current = this;
		DismissLocal();// прячем
	}
	
	public static void Show (string title, string content) {
		Current.textTitle.text = title;
		Current.textContent.text = content;
		Current.gameObject.SetActive(true);
	}
	
	public static void Dismiss () {
		Current.DismissLocal();
	}
	
	public void DismissLocal () {
		gameObject.SetActive(false);
	}
}
