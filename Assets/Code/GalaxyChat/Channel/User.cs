using System;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour {

	public TextMesh textName;
	public int id;
	
	public Place place;
	
	List<View> views = new List<View>();
	List<ImageObject> images = new List<ImageObject>();
	string clan;
	
	int sex;
		
	public void SetId(int value){
		id = value;
	}
	public void SetSex(int value){
		sex = value;
	}
	public void SetName(string value){
		if (value.StartsWith("+")) {
            //setStatusSimple(UserState.STATUS_OPERATOR);
            value = value.Substring(1);
        }
        if (value.StartsWith("@")) {
            //setStatusSimple(UserState.STATUS_FOUNDER);
            value = value.Substring(1);
        }
		name = value;
		
		/*if (textName == null){
			// 1.72 - высота планеты
			//var go = Instantiate(Prefabs.Current.UserNameText, new Vector3(0,1.72f,0), Quaternion.identity) as GameObject;
			//go.transform.SetParent(transform);
			textName = GetComponent<TextMesh>();
		}*/
		textName.text = name;
	}
	
	public void SetClan(string value){
		clan = value;
	}
	public void SetPos(Vector3 pos){
		transform.localPosition = pos;
	}
	public void AddView(View v){
		views.Add(v);
	}
	
	void OnImageLoaded(ImageObject image){
		//print("image.loaded: "+image.url);
		var go = new GameObject();
		go.name = image.url;
		var rend = go.AddComponent<SpriteRenderer>();
		rend.sortingOrder = image.layer;
		rend.sprite = Sprite.Create(image.tex2d,new Rect(0,0,image.tex2d.width,image.tex2d.height),new  Vector2(0.5f,0));
		go.transform.SetParent(transform);
		go.transform.localPosition = new Vector2(image.x, image.y);
	}
	
	public ImageObject NewImageObject(string url){
		return new ImageObject(url);
		/*var go = new GameObject();
		go.name = image.url;
		go.transform.SetParent(transform); 
		var rend = go.AddComponent<SpriteRenderer>();*/		
	}
	
	public void LoadImage(ImageObject image){
		var io = GetImageByUrl(image.url);
		if (io != null)
			return;
		images.Add(image);
		if (gameObject.activeInHierarchy)
			StartCoroutine( image.LoadImage(OnImageLoaded) );
	}
	
	ImageObject GetImageByUrl(string url){
		foreach (var i in images){
			if (i.url == url)
				return i;
		}
		return null;
	}
	
	// заставляем персов всегда смотреть в одном направлении, на камеру
	void LateUpdate(){
		//var trans = Camera.main.transform;
		//transform.LookAt(trans);
		//var e = transform.rotation.eulerAngles;
		//e.x = e.z = 0;
		transform.rotation = Quaternion.Euler(Vector3.zero);
	}
	
}
