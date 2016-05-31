﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/* Место стояния юзера */

public class Place {
	public Vector3 pos;
	public User user;
}


/* Планета с персами */

public class Planet : MonoBehaviour {

	public float persDistance = 5.2f;
	public float persDistanceInner = 2.5f;
	public int persCount = 55;
	public int persCountInner = 15;
	
	List<User> users = new List<User>();
	public string Name {get;private set;}
	public delegate void ValueChangedFunc(int value);
	public ValueChangedFunc OnUsersCountChanged;
	
	List<Place> places = new List<Place>();
	
	void Start(){
		InitPlaces();
	}
	
	void InitPlaces(){
		InitPlacesInternal(persCount, persDistance);
		InitPlacesInternal(persCountInner, persDistanceInner);
	}
	
	void InitPlacesInternal(int cnt, float dist){
		var stepAngle = 360f/(float)cnt;
		cnt = cnt/2 + cnt%2;
		for (int k = 0; k < cnt; ++k){
			var angle = (float)k * stepAngle + 180f;
			var x = dist * Mathf.Sin(angle*Mathf.Deg2Rad);
			var z = dist * Mathf.Cos(angle*Mathf.Deg2Rad);
			var p = new Vector3(x,0,z);
			var place = new Place();
			place.pos = p;
			places.Add(place);
			if (k == 0)
				continue;
			angle = -(float)k * stepAngle + 180f;
			x = dist * Mathf.Sin(angle*Mathf.Deg2Rad);
			z = dist * Mathf.Cos(angle*Mathf.Deg2Rad);
			p = new Vector3(x,0,z);
			place = new Place();
			place.pos = p;
			places.Add(place);
		}
	}
	
	Place GetEmptyPlace(){
		foreach (var item in places){
			if (item.user == null)
			return item;
		}
		return null;
	}
	
	public void Scroll(float dx, float dy){
		transform.Rotate(Vector3.up, -10f*dx);
	}
	
	float autoScrollAngle;
	bool isAutoscroll;
	public void ResetAutoScrollToUser(){
		isAutoscroll = false;
	}
	public void AutoScrollToUser(){
		var orient = new Vector3(0,0,-12);
		// находим ближайшее к камере занятое место
		Place near = null;
		float min = float.MaxValue;
		foreach (var item in places){
			if (item.user == null)
				continue;
			var d = Vector3.Distance(orient, item.user.transform.position);
			if (d < min){
				min = d;
				near = item;
			}
		}
		var p = near.user.transform.position;
		float a = Vector3.Angle(orient, p);
		if (p.x < orient.x)
			a = -a;
		autoScrollAngle = a;
		isAutoscroll = true;
		StartCoroutine(AutoScrollInternal());
	}
	
	IEnumerator AutoScrollInternal(){
		var step = autoScrollAngle/10f;
		for (int k = 0; k < 10; ++k){
			if (!isAutoscroll)
				yield break;
			transform.Rotate(Vector3.up, step);
			yield return new WaitForSeconds(0.02f);
		}
		yield break;
	}
		
	User NewUser(){
		var go = Prefabs.NewInstantce(Prefabs.Current.UserObj);
		go.transform.SetParent(transform);
		var u = go.GetComponent<User>();
		return u;
	}
	
	public User GetUserByID(int id){
		foreach (var u in users){
			if (u.id == id)
				return u;
		}
		return null;
	}
	
	void AddUser(User user){
		var u = RemoveUser(user.id);
		users.Add(user);
		if (u != null){
			var pos = u.transform.localPosition;
			user.SetPos(pos);
		}
		InvokeUsersCountChanged();
	}
	
	User RemoveUser(int userId){
		var u = GetUserByID(userId);
		if (u != null){
			var p = u.place;
			p.user = null; // "освободили" место
			users.Remove(u);
			Destroy(u.gameObject);
		}
		InvokeUsersCountChanged();
		return u;
	}
	
	void InvokeUsersCountChanged(){
		if (OnUsersCountChanged != null)
			OnUsersCountChanged(users.Count);
	}
	
	public void Clear(){
		foreach (var u in users){
			Destroy(u);
		}
		users.Clear();
	}
	
	public void SetName(string value){
		Name = value;
	}
	
	void SetUserOnEmptyPlace(User u){
		var p = GetEmptyPlace();
		u.SetPos(p.pos);
		u.place = p;
		p.user = u;
	}
	
	public void JoinUsers(string line){
		var param = line.SplitBySpace();
		int len = param.Length;
		for (int k = 0 ; k < len ; /*пусто*/) {
            User user = NewUser();
			UsersFactory.createUser(user, true, param, k, true, false, false);
			// ставим
			SetUserOnEmptyPlace(user);
			AddUser(user);
			int head = param[k+3].ToInt();
            if (head < 0) {
                int val = Math.Abs(head);
                k += 5+5*val;
            } else {
                k += 6;
			}
        }
	}
	
	public void JoinUser(string[] param){
		User user = NewUser();
		UsersFactory.createUser(user, true, param, 0, true, true, true);
		// ставим
		SetUserOnEmptyPlace(user);
		AddUser(user);
	}
	
	public void PartUser(string[] param){
		RemoveUser(param[0].ToInt());
	}
	
	public void ParseAuthority(string[] param){
		// 860 {<userid_1> <rating_1> <aura_view_description_1> …}
		// 860 8530196 161 1;aura/0_1;0;0;33
		var descriptionsCount = param.Length / 3;

		var i = 0;
		for (var n = 0; n < descriptionsCount; n++)
		{
			var userID = param[i++].ToInt();
			var rating = param[i++].ToInt();
			var viewDescription = param[i++];

			var user = GetUserByID(userID);
			if (user == null) continue;
			
			bool noDescr = (viewDescription == "-");

			//user.Authority = rating;
			
			if (noDescr)
				continue;

			var viewElements = viewDescription.Split(';');
			var img = user.NewImageObject(viewElements[1]);
			img.layer = -1000; // аура всегда сзади внешнего вида
			user.LoadImage(img);
		}
	}
	
}

//:- +autofocused 8530196 -3 hb/8501_ 0 -41 33 0 hb/b128_ 0 0 33 0 @ 113 60 123 65 1691 - +flamingo 28613169 -3 hb/43_ 0 -77 33 0 hb/b163_ 0 0 33 0 @ 110 48 120 53 1611 - +chenita 33327583 -2 8843 0 0 33 0 @ 112 53 122 58 1651 Euphoria +Margauxx 34091200 -3 hb/4404_ 0 -3 33 0 hb/b151_ 0 0 33 0 @ 69 34 79 39 1553 - +Nat2 36144451 -2 8845 0 0 33 0 @ 110 53 120 58 1571