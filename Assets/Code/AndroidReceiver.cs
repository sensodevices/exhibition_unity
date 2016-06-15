using UnityEngine;
using UnityEngine.UI;

public class AndroidReceiver : MonoBehaviour {

	public Text textDebug;

#if UNITY_ANDROID
	
	AndroidJavaClass jc;
	byte[] data;

	void Start () {
		jc = new AndroidJavaClass("com.fingerdev.blereceiver.DataReceiver");
        jc.CallStatic("createInstance");
        data = jc.GetStatic<byte[]>("result");// запоминаем ссылку на буфер
	}
	
	void Update () {
		jc.CallStatic("prepareResult");
		var size = jc.GetStatic<int>("size");
		if (size > 0){
			print("unity.received: "+size);
			
			// тут обработка size байт из data

			if (textDebug != null)
				textDebug.text = size.ToString();
			jc.CallStatic("resetResult");
		}
	}

#endif

}
