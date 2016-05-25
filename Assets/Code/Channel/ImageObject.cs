using UnityEngine;
using System.Collections;
using System;

public class ImageObject {

	public Texture2D tex2d;
	public string url;
	public float x, y;
	public int layer;
	int anchor;
	
	public ImageObject(string url=null){
		this.url = url;
	}
	
	public IEnumerator LoadImage(Action<ImageObject> completedCallback){
		var path = GetFullPath(url);
		//Debug.Log("load image: "+path);
		var www = new WWW(path);
        yield return www;
		tex2d = www.texture;
		completedCallback(this);
	}
	
	public void SetPos(float px, float py){
		x = px;
		y = py;
	}

	public void SetAnchor(int value){
		anchor = value;
	}
	
	void Update () {
	
	}
	
	public static string GetFullPath(string url){
		if (!url.StartsWith("http://"))
			url = "http://galaxy.mobstudio.ru/server_pics/"+url;
		if (!(url.EndsWith(".png") || url.EndsWith(".jpg") || url.EndsWith(".jpeg"))){
			if (!url.EndsWith("_"))
				url += "_";
			url += ".png";
		}
		return url;
	}
	
}
