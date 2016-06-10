using UnityEngine;
using UnityEngine.UI;

public class ChatSettings : MonoBehaviour {

	public static ChatSettings Me {get;private set;}

	public string CustomPlanetName = "";
	public string RecoveryCode;
	public string Host = "galaxy.mobstudio.ru";
	public int Port = 6667;

	public InputField inputServer, inputRecovery;

	void Awake () {
		Me = this;
		LoadPrefs();
		gameObject.SetActive(false);
	}
	
	public void Save(){
		var serv = inputServer.text;
		var p = serv.Split(':');
		Host = p[0];
		Port = p[1].ToInt();
		RecoveryCode = inputRecovery.text;
		SavePrefs();
		Hide();
	}

	public void Show(){
		inputServer.text = Host+":"+Port;
		inputRecovery.text = RecoveryCode;
		gameObject.SetActive(true);
	}
	public void Hide(){
		gameObject.SetActive(false);
	}

	void LoadPrefs(){
		Host = PlayerPrefs.GetString("host", Host);
		Port = PlayerPrefs.GetInt("host", Port);
		RecoveryCode = PlayerPrefs.GetString("recovery", RecoveryCode);
	}

	void SavePrefs(){
		PlayerPrefs.SetString("host", Host);
		PlayerPrefs.SetInt("host", Port);
		PlayerPrefs.SetString("recovery", RecoveryCode);
		PlayerPrefs.Save();
	}

}
