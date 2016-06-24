using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/* Место стояния юзера */

public class Place {
	public Vector3 pos;
	public User user;
	public bool full;
}


/**/

public interface IMenuRequester{
	void RequestMenu();
}


/* Планета с персами */

public class Planet : MonoBehaviour, IMenuRequester {

	public MeshRenderer mesh;
	public Color highlightColor = new Color(1.0f, 0.3960f, 0.0666f, 1.0f);
	public float rotateSpeed = 15.5f;
	public float minimalScrollDelta;
	public float persDistance = 5.2f;
	public int persCount = 55;
	public int myUserId;
	
	List<User> users = new List<User>();
	public string Name {get;private set;}
	public delegate void ValueChangedFunc(int value);
	public ValueChangedFunc OnUsersCountChanged;
	
	List<Place> places = new List<Place>();
	Color defColor;
	Place selPlace;
	Quaternion initialRotation;

	private float m_colorChangeDt = 1.0f;
	private int m_colorChangingDir = 0; // 1 = def to highlight, -1 = highlight to def
	


	void Start(){
		InitPlaces();
		defColor = mesh.material.color;
		initialRotation = transform.rotation;
	}

	void Update() {
		if (m_colorChangingDir != 0) {
			m_colorChangeDt += Time.deltaTime;
			Color c1 = m_colorChangingDir == 1 ? defColor : highlightColor;
			Color c2 = m_colorChangingDir == 1 ? highlightColor : defColor; 

			if (m_colorChangeDt > 1.0f) {
				mesh.material.color = c2;
				m_colorChangingDir = 0;
			} else {
				Color curColor = mesh.material.color;
				curColor.r = Mathf.Lerp(c1.r, c2.r, m_colorChangeDt); // FIXME: change to Vector3.Lerp if need to change more than 1 component
				mesh.material.color = curColor;
			}
		}
	}
	
	public void RequestMenu(){
		var u = GetSelected();
		if (u == null) // чел улетел с переднего места
			return;
		MenuActions.Show(u.id, u.name);
	}

	void InitPlaces(){
		InitPlacesInternal(persCount, persDistance);
		//InitPlacesInternal(persCountInner, persDistanceInner);
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
	
	User GetSelected(){
		if (selPlace == null)
			selPlace = places[0];
		return selPlace.user;
	}
	
	Place GetEmptyPlace(){
		foreach (var item in places){
			if (!item.full){
				item.full = true;
				return item;
			}
		}
		return null;
	}
	
	public void Scroll(float dx, float dy){
		scrollDelta += dx;
		if (!HasMinScroll())
			return;
		transform.Rotate(Vector3.up, -rotateSpeed*dx);
	}
	
	bool HasMinScroll(){
		return Mathf.Abs(scrollDelta) >= minimalScrollDelta;
	}
	
	float autoScrollAngle;
	bool isAutoscroll;
	float scrollDelta;
	public void EnterPersCollider(){
		
	}
	public void ExitPersCollider(){
		if (HasMinScroll()) // при прокрутке не вызываем меню на персе
			return;
		// меню было тут
	}
	
	public void EnterScrollCollider(){
		isAutoscroll = false;
		// Change color
		if (m_colorChangeDt >= 0.7f) {
			m_colorChangeDt = 0.0f;
			m_colorChangingDir = 1;
		} else {
			mesh.material.color = highlightColor;
		}
		scrollDelta = 0;
	}
	public void ExitScrollCollider(){
		// Change color
		if (m_colorChangeDt >= 0.7f) {
			m_colorChangeDt = 0.0f;
			m_colorChangingDir = -1;
		} else {
			mesh.material.color = defColor;	
		}
		RotateToNearUser();
	}
	
	void RotateToNearUser(){
		var orient = new Vector3(0,0,1);

		// находим ближайшее к камере занятое место
		Place near = null;
		float min = float.MaxValue;
		foreach (var item in places){
			if (item.user == null)
				continue;
			var pos = item.user.transform.position - transform.position;
			var d = Vector3.Distance(orient, pos);
			if (d < min){
				min = d;
				near = item;
			}
		}
		var p = near.user.transform.position - transform.position;
		float a = Vector3.Angle(orient, p);
		if (p.x > orient.x)
			a = -a;
		autoScrollAngle = a;
		isAutoscroll = true;
		StartCoroutine(AutoScrollInternal());
		selPlace = near;
	}

	IEnumerator AutoScrollInternal(){
		var step = autoScrollAngle/10f;
		for (int k = 0; k < 10; ++k){
			if (!isAutoscroll)
				yield break;
			transform.Rotate(Vector3.up, step);
			yield return new WaitForSeconds(0.02f);
		}
		UpdateNicks();
		yield break;
	}

	// смотрим какие юзеры ближе к камере, ники только у них рисуем 
	void UpdateNicks(){
		var maxDist = 1.4f;
		var orient = transform.position + new Vector3(0,0,2);
		foreach (var item in places){
			if (item.user == null)
				continue;
			var d = Vector3.Distance(orient, item.user.transform.position);
			//Log.Galaxy("dist: "+d);
			var vis = (d < maxDist);
			item.user.SetNickVisibility(vis);
		}
	}	

	User NewUser(){
		var go = Prefabs.NewInstantce(Prefabs.Me.UserObj);
		go.transform.SetParent(transform);
		var u = go.GetComponent<User>();
		go.transform.localScale = u.PrefferedScale;
		return u;
	}
	
	public User GetUserByID(int id){
		foreach (var u in users){
			if (u.id == id)
				return u;
		}
		return null;
	}
	
	bool Filtering(User user){
		// залипуха, чтобы ящик от "пушек и бочек" не добавлять, он вид портит
		return !user.HasImageWithUrl("temp/c/box_", true);
	}

	void AddUser(User user){
		var ok = Filtering(user);
		if (!ok)
			return;
		RemoveUser(user.id);
		SetUserOnEmptyPlace(user);
		users.Add(user);
		InvokeUsersCountChanged();
		//UpdateNicks();
	}
	
	User RemoveUser(int userId){
		var u = GetUserByID(userId);
		if (u != null){
			RemoveUser(u);
		}
		return u;
	}
	
	void RemoveUser(User u){
		var p = u.place;
		p.user = null; // "освободили" место
		p.full = false;
		users.Remove(u);
		Destroy(u.gameObject);
		InvokeUsersCountChanged();
	}

	void InvokeUsersCountChanged(){
		if (OnUsersCountChanged != null)
			OnUsersCountChanged(users.Count);
	}
	
	public void Reset(){
		foreach (var u in users){
			var p = u.place;
			p.user = null; // "освободили" место
			p.full = false;
			Destroy(u.gameObject);
		}
		users.Clear();
		selPlace = null;
		transform.rotation = initialRotation;
	}
	
	public void SetName(string value){
		Name = value;
	}
	
	void SetUserOnEmptyPlace(User u){
		var p = GetEmptyPlace();
		if (p == null){// этому юзеру не хватило места на планете
			u.gameObject.SetActive(false);
			return;
		}
		u.SetPos(p.pos);
		u.place = p;
		p.user = u;
	}
	
	public void JoinUsers(string line){
		var param = line.SplitBySpace();
		int len = param.Length;
		for (int k = 0 ; k < len ; /*пусто*/) {
            try {
				User user = NewUser();
				UsersFactory.createUser(user, true, param, k, true, false, false);
				AddUser(user);
				int head = param[k+3].ToInt();
				if (head < 0) {
					int val = Math.Abs(head);
					k += 5+5*val;
				} else {
					k += 6;
				}
			} catch (Exception e){
				print("param[3]: "+param[k+3]);
				throw new Exception("PARSE ERROR");
			}
        }
		UpdateNicks();
	}
	
	public void JoinUser(string[] param){
		User user = NewUser();
		UsersFactory.createUser(user, true, param, 0, true, true, true);
		if (user.id == myUserId) {// своего заново не добавляем
			Destroy(user.gameObject);
			return;
		}
		// ставим
		AddUser(user);
		UpdateNicks();
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