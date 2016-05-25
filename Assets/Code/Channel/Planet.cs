﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class Planet : MonoBehaviour {

	List<User> users = new List<User>();
	public string Name {get;private set;}
	public delegate void ValueChangedFunc(int value);
	public ValueChangedFunc OnUsersCountChanged;
	
	void Start(){
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
			var pos = u.transform.position;
			user.SetPos(pos.x, pos.y);
		}
		InvokeUsersCountChanged();
	}
	
	User RemoveUser(int userId){
		var u = GetUserByID(userId);
		if (u != null){
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
	
	float GetUserPosX(){
		return (float)(users.Count)*1.7f;
	}
	
	public void SetName(string value){
		Name = value;
	}
	
	public void JoinUsers(string line){
		var param = line.SplitBySpace();
		int len = param.Length;
		for (int k = 0 ; k < len ; /*пусто*/) {
            User user = NewUser();
			User.createUser(user, true, param, k, true, false, false);
			// ставим
			user.SetPos(GetUserPosX(), 0);
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
		User.createUser(user, true, param, 0, true, true, true);
		// ставим
		user.SetPos(GetUserPosX(), 0);
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