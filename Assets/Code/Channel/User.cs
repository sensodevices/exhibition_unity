using System;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour {

	public TextMesh textName;
	public int id;
	
	
	const float SCALE = 100f;
	List<View> views = new List<View>();
	List<ImageObject> images = new List<ImageObject>();
	string clan;
	
	int sex;
		
	void SetId(int value){
		id = value;
	}
	void SetSex(int value){
		sex = value;
	}
	void SetName(string value){
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
	
	void SetClan(string value){
		clan = value;
	}
	public void SetPos(float px, float py){
		transform.localPosition = new Vector2(px, py);
	}
	void AddView(View v){
		views.Add(v);
	}
	
	void OnImageLoaded(ImageObject image){
		print("image.loaded: "+image.url);
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
		StartCoroutine( image.LoadImage(OnImageLoaded) );
	}
	
	ImageObject GetImageByUrl(string url){
		foreach (var i in images){
			if (i.url == url)
				return i;
		}
		return null;
	}
		
	public static void createUser(User user, bool checkClanNameId, string[] param, int start, bool checkPos, bool checkSex, bool checkColors) {
        float px, py;
		int anchor;
        int ind = start;
        string imgId, extra;
        try {
            if (checkClanNameId) {
                user.SetClan(param[ind++]);
                user.SetName(param[ind++]);
                user.SetId(param[ind++].ToInt());
            }
            if (checkSex) {
                user.SetSex(param[ind++].ToInt());
            }
            View view = new View();
			user.AddView(view);
			
			int val = Math.Abs(param[ind++].ToInt());
			for (int k = 0 ; k < val ; ++k) {
				imgId = param[ind++];
				if (imgId == "@") {
					ind += 4;
					//Midlet.sout("set offsets");
					/*founderDyBig = Integer.parseInt(params[ind++]);
					founderDySmall = Integer.parseInt(params[ind++]);
					smotrDyBig = Integer.parseInt(params[ind++]);
					smotrDySmall = Integer.parseInt(params[ind++]);
					isSetDy = true;*/
				} else {
					if (imgId.StartsWith("+")) {
						imgId = imgId.Substring(1);
					}
					px = Coord(param[ind++].ToDouble());
					py = -Coord(param[ind++].ToDouble());
					
					anchor = param[ind++].ToInt();
					extra = param[ind++];
					ImageObject img = user.NewImageObject(imgId);
					img.SetPos(px, py);
					img.SetAnchor(anchor);
					img.layer = -k;
					view.AddImage(img);
					//
					user.LoadImage(img);
				}
			}
						
			if (checkPos) {
				px = Coord(param[ind++].ToDouble());
				//user.SetPos(px*2, 0);
			}
            //проверяем есть ли цвета для телепорта
            /*if (checkColors) {
                //teleportColorMain = Utils.hexToInt(params[ind++]);
                //teleportColorBorder = Utils.hexToInt(params[ind++]);
                //teleportColorLines = Utils.hexToInt(params[ind++]);
                user.teleportSet(params, ind);
            }*/
        } catch(Exception e) {
        }

    }
	
	static float Coord(double value){
		return (float)value/SCALE;
	}
	
}
