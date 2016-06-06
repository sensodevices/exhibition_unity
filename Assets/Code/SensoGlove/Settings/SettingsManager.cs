using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour {

	public InputField portInput;
	public InputField vibratePortInput;
	public InputField vibrateHostInput;
	public Button saveAndExitButton;
	public NetworkManager netManager = null;
	
	// Use this for initialization
	void Start () {
		if (portInput != null) {
			int currentPort = GameSettings.GetInboundPort();
			portInput.text = currentPort.ToString();
		}
		if (vibratePortInput != null) {
			int currentPort = GameSettings.GetVibratePort();
			Debug.Log(currentPort);
			vibratePortInput.text = currentPort.ToString();
		}
		if (vibrateHostInput != null) {
			string currentHost = GameSettings.GetVibrateHost();
			vibrateHostInput.text = currentHost;
		}
		if (saveAndExitButton != null) {
			saveAndExitButton.onClick.AddListener(() => SaveSettingsAndExit());
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void SaveSettings()
	{
		int newPort = 0;
		bool needSave = false;
		bool parsed = int.TryParse(portInput.text, out newPort);
		if (parsed && newPort != GameSettings.GetInboundPort())
		{
			GameSettings.SetInboundPort(newPort);
			if (!needSave) needSave = true;
		}
		
		parsed = int.TryParse(vibratePortInput.text, out newPort);
		if (parsed && newPort != GameSettings.GetVibratePort())
		{
			GameSettings.SetVibratePort(newPort);
			if (!needSave) needSave = true;
		}
		
		if (!vibrateHostInput.text.Equals(GameSettings.GetVibrateHost()))
		{
			GameSettings.SetVibrateHost(vibrateHostInput.text);
			if(!needSave) needSave = true;
		}
		
		if (needSave)
		{
			GameSettings.Save();
			if (netManager != null) {
				netManager.Restart();
			}	
		}
	}
	
	void SaveSettingsAndExit()
	{
		this.SaveSettings();
		Object.Destroy(this.gameObject);
	}
}
