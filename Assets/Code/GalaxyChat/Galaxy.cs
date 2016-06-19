using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections;
using System;

public class Galaxy : MonoBehaviour {

	public Text textEntering, textPlanetName, textUsersCount;
	public Button buttonConnect;
	public InputField inputMessage;
	public ScrollRect chatScrollRect;
	public CameraHelper cameraHelper;
	public Planet planet;
	public Finger finger;
	public Chat chat;
	public Chat3d chat3d;
	
	int userId;
	string userName, userPass, authCode;
	string sessionId;
	DateTime sessionTime;
	TcpSocket socket;
	TouchListener touch;
	bool isWait353 = true;
		
	void Start () {
		
		touch = GetComponent<TouchListener>();
		
		planet.OnUsersCountChanged += OnUsersCountChanged;
		
		finger.OnSwipeChat += OnSwipeChat;
		finger.OnSwipePlanet += OnSwipePlanet;
		finger.OnExitCollision += OnExitCollision;
		finger.OnEnterCollision += OnEnterCollision;
		
		ConnectAsync();
	}
	
	void OnEnterCollision(ColType type){
		if (type == ColType.Planet){
			planet.EnterScrollCollider();
		} else if (type == ColType.Pers){
			planet.EnterPersCollider();
		} else if (type == ColType.Chat){
			chat3d.EnterCollider(); 
		}
		
	}
	
	void OnExitCollision(ColType type){
		if (type == ColType.Planet){
			planet.ExitScrollCollider();
		} else if (type == ColType.Pers){
			planet.ExitPersCollider();
		} else if (type == ColType.Chat){
			chat3d.ExitCollider();
		}
		
	}
	
	void OnSwipeChat(float dx, float dy){
		chat.Scroll(dx, dy);
	}
	
	void OnSwipePlanet(float dx, float dy){
		planet.Scroll(dx, dy);
	}
	
	void OnUsersCountChanged(int value){
		textUsersCount.text = "["+value+"]";
	}
	
	public void ConnectAsync(bool useDelay = false){
		StartCoroutine(ConnectInternal(useDelay));
	} 
	
	bool startConnect;
	int connectCounter;
	IEnumerator ConnectInternal(bool useDelay){
		
		if (useDelay){
			yield return new WaitForSeconds(5f);// немного ждём, полезно для реконнекта
		}

		if (startConnect)
			yield break;

		startConnect = true;

		if (socket == null){
			socket = new TcpSocket();
			socket.OnMessageReceived += OnMessageReceived;
			socket.OnError += OnErrorReceived;
		}
		
		
		var connectInfo = ChatSettings.Me;
		socket.Connect(connectInfo.Host, connectInfo.Port);
		textEntering.gameObject.SetActive(true);
		buttonConnect.gameObject.SetActive(false);
		//chatTitle.gameObject.SetActive(false);

		startConnect = false;
		++connectCounter;
		yield break;
	} 
	
	void Disconnect(){
		cmdQuit();
		textEntering.gameObject.SetActive(false);
		buttonConnect.gameObject.SetActive(true);
		//chatTitle.gameObject.SetActive(false);
		wasConnected = false;
	}
	
	void OnDisable(){
		cmdQuit();
	}
	
	void Update(){
		UpdateNetwork();
		UpdateInput();
		CheckEscape();
	}
	
	void CheckEscape(){
		if (Input.GetKeyDown(KeyCode.Escape)){
			Application.Quit();
		}
	}
	
	bool wasConnected;
	float timeLastCmd;
	void UpdateNetwork(){
		if (socket != null){
			socket.Update();

			var con = socket.IsConnected;
			if (wasConnected){
				var time = Time.realtimeSinceStartup; 
				if (time > timeLastCmd+5f){// раз в 5 сек проверяем коннект
					cmdPong();					
				}
			}
			wasConnected = con;
		}
	}
	
	Vector2 prevPos;
	void UpdateInput(){
		
		if (Input.GetKeyDown(KeyCode.T)){
			// заполняем чат тестовыми сообщениями
			//StartFillChat();
		}
		
		var pressed = touch.IsPressed;
		
		// крутим планету
		if (touch.ScreenPos.y < Screen.height*0.25f){
			/*if (pressed){
				//cameraHelper.StartMove(touch.ScreenPos);
				prevPos = touch.ScreenPos;
			} else if (touch.IsDowned){
				var delta = touch.ScreenPos-prevPos;
				var angle = -delta.x*0.4f;
				planet.transform.Rotate(Vector3.up, angle);
				prevPos = touch.ScreenPos;
				//cameraHelper.Move(touch.ScreenPos);
			}*/
		}
		
		// проверим попадение в юзера
		if (pressed){
			var ray = Camera.main.ScreenPointToRay(touch.ScreenPos);
			var hit = Physics.RaycastAll(ray);
			if (hit.Length > 0){
				var go = hit[0].collider.gameObject;
				var u = go.GetComponent<User>();
				if (u != null){
					var me = (u.id == userId);
					inputMessage.text = me ? "" : u.name+", ";
				}
			}
		}
	}
	
	/*void StartFillChat(){
		StartCoroutine(StartFillChatInternal());
	} 
	
	IEnumerator StartFillChatInternal(){
		for (int k = 0; k < 200; ++k){
			AddMessage(userId, "testing chat message "+UnityEngine.Random.Range(0,100));
			yield return new WaitForSeconds(0.3f);
		}
		yield break;
	}*/
	
	public void SelectPlanet(){
		InputBox.OnButtonOkPressed += OnPlanetNameEntered;
		InputBox.Show("Fly", "Select planet by name", "Enter planet name...");
	}
	
	void OnPlanetNameEntered(string value){
		InputBox.OnButtonOkPressed -= OnPlanetNameEntered;
		value = value.Trim();
		if (value != "")
			cmdJoin(value);
	}
	
	/*void SendMessageFromInput(){
		var s = inputMessage.text.Trim();
		if (s != ""){
			cmdPrivmsg(s);
			chat.AddMessage(userName, s);
			inputMessage.text = "";
		}
	}*/
	
	void AddMessage(int userId, string msg){
		string nick = null;
		if (userId > 0){
			var u = planet.GetUserByID(userId);
			if (u != null){
				nick = u.name;
			}
		}
		chat.AddMessage(nick, msg);
	}
	
	void SendToServer(string message){
		socket.Write(message);
		Log(">> " + message);
		timeLastCmd = Time.realtimeSinceStartup;
	}
	
	void OnEnterChannel(){
		textEntering.gameObject.SetActive(false);
		textPlanetName.text = planet.Name;
		//chatTitle.gameObject.SetActive(true);
		PlayerPrefs.SetString("planetName", planet.Name);
	}
	
	void OnErrorReceived (object sender, TcpErrorEventArgs args) {
		wasConnected = false;
		Log("OnErrorReceived: "+args.Message);
		if (ChatSettings.Me.UseReconnection)
			ConnectAsync(true);
	}

	void ReadPrefs(){
		userName = PlayerPrefs.GetString("galaxy-userName", null);
		userPass = PlayerPrefs.GetString("galaxy-userPass", null);
		userId = PlayerPrefs.GetInt("galaxy-userId", 0);
		sessionId = PlayerPrefs.GetString("galaxy-sessionId", null);
		var t = PlayerPrefs.GetString("galaxy-sessionTime", null);
		sessionTime = (t != null ? DateTime.Parse(t) : DateTime.Now);
	}
	void WriteUserPrefs(){
		PlayerPrefs.SetString("galaxy-userName", userName);
		PlayerPrefs.SetString("galaxy-userPass", userPass);
		PlayerPrefs.SetInt("galaxy-userId", userId);
	}
	void WriteSessionPrefs(bool adjustCurrentTime){
		if (adjustCurrentTime){
			sessionTime = DateTime.Now;
		}
		PlayerPrefs.SetString("galaxy-sessionId", sessionId);
		PlayerPrefs.SetString("galaxy-sessionTime", sessionTime.ToString());
	}

	void OnMessageReceived (string message) {
		timeLastCmd = Time.realtimeSinceStartup;
		if (message.Contains("\r\n")){
			var arr = Regex.Split(message, "\r\n");
			foreach (var m in arr){
				OnMessageReceivedInternal(m);
			}
		} else {
			OnMessageReceivedInternal(message);
		}
	}
	void OnMessageReceivedInternal (string message) {
		message = message.Trim();
		if (message == "")
			return;
		//log("<< '" + message + "'");
		Log("<< " + message);
		GalaCommand c = new GalaCommand(message.Trim());
		/*print("prefix: "+c.Prefix);
		print("name: "+c.Name);
		print("params: "+c.Parameters);
		print("postfix: "+c.Postfix);*/
		
		switch (c.Name)
		{
			case "HAAAPSI": // приветствие
				var code = c.Parameters[0];
				authCode = SessionCodeGenerator.Generate(code);
				cmdIdent();
				ReadPrefs();
				if (!userPass.IsNullOrWhiteSpace()){
					cmdUser();
				} else {
					cmdRecover(ChatSettings.Me.RecoveryCode);
				}
				break;
				
			case "REGISTER": // добро на вход
				userId = c.Parameters[0].Trim().ToInt();
				userPass = c.Parameters[1].Trim();
				userName = c.Parameters[2].Trim();
				cmdUser();
				planet.myUserId = userId;
				WriteUserPrefs();
				break;
				
			case "999": // auth OK
				cmdAddons();
				//PlayerPrefs.GetString("planetName", null);
				string s = null;//"pi100let";
				var custom = ChatSettings.Me.CustomPlanetName;
				if (!custom.IsNullOrWhiteSpace())
					s = custom;
				cmdJoin(s);
				break;
			
			case "PING":
				cmdPong();
				break;
			
			case "353": // список юзеров. может быть несколько таких команд
				if (isWait353){
					planet.Clear();
					isWait353 = false;
				}
				planet.JoinUsers(c.Postfix);
				planet.SetName(c.Parameters[2]);
				break;
			
			case "366":
				isWait353 = true;
				OnEnterChannel();
				break;
				
			case "860": // авторитет юзера / 860 8530196 161 1;aura/0_1;0;0;33
				planet.ParseAuthority(c.Parameters);
				break;
				
			case "JOIN": // вход перса
				planet.JoinUser(c.Parameters);
				break;
			
			case "PART": // выход перса
				planet.PartUser(c.Parameters);
				break;
				
			case "451": // ошибки
			case "452":
			case "432":
			case "433":
			case "601":
			case "403":
				MessageBox.Show("System Notice", c.Postfix);
				Disconnect();
				break;
			
			case "471": // планета переполнена
				MessageBox.Show("System Notice", c.Postfix);
				//Disconnect();
				break;
				
			case "473":
				MessageBox.Show("Fly","Can't fly to closed planet!");
				//Disconnect();
				break;
					
			case "900": // инфа о планете
				planet.SetName(c.Parameters[0]);
				break;
				
			case "PRIVMSG": // инфа о планете
				AddMessage(c.Parameters[0].ToInt(), c.Postfix);
				break;
			
			case "332": // топик
				AddMessage(-1, "Subject: "+c.Postfix);
				break;
			
			case "ACTION":
				ParseAction(c);
				break;
				
			case "KICK":
				ParseKick(c);
				break;
				
			case "852":
				if (isWait852){
					Reset852();
					isWait852 = false;
				}
				Parse852(c);
				break;
			
			case "853":
				isWait852 = true;
				Prepare852();
				break;
				
			default:
				break;
		}
	}
	
	bool isWait852;
	//852 2402 :Set cannon up
	void Parse852(GalaCommand command){
		MenuActions.AddItem(command.Postfix, command.Parameters[0]);
	}
	void Reset852(){
		MenuActions.Clear();
	}
	void Prepare852(){
		MenuActions.Prepare();
	}
	
	// ACTION 8530196 8807955 :БАОБАБ gives autofocused a massage
	void ParseAction(GalaCommand command){
		var senderId = command.Parameters[1].ToInt();
		var s = command.Postfix;
		var i = s.IndexOf(' ');
		s = s.Substring(i+1);
		AddMessage(senderId, s);
	}
	
	// KICK 8530196 0 -1 -1 -1 :You can't fly so frequently. One flight per 10 seconds is allowed.
	void ParseKick(GalaCommand command){
		/*var senderId = command.Parameters[1].ToInt();
		var s = command.Postfix;
		var i = s.IndexOf(' ');
		s = s.Substring(i+1);
		AddMessage(senderId, s);*/
	}
	
	private void cmdPong(){
		SendToServer("PONG");
	}
	
	private void cmdJoin(string planetName=null){
		var s = "JOIN";
		if (planetName != null)
			s += " "+planetName;
		SendToServer(s);
	}
	
	private void cmdAddons(){
		SendToServer("ADDONS 0");
		SendToServer("MYADDONS 0");
	}
	
	private void cmdIdent(){
		SendToServer(":en IDENT 186 -1 4030 1 2 :GALA");
	}
	private void cmdRecover(string recovery){
		SendToServer("RECOVER "+recovery);
	}
	
	/*long Millisecs(){
		return (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
	}*/

	private void cmdUser(){
		
		var time = DateTime.Now;
		var dt = time-sessionTime;
		
		// если ещё нет сессии или она устарела (> 10 мин)
		if (sessionId.IsNullOrWhiteSpace() || dt.TotalMinutes > 10f){
			sessionId = SessionCodeGenerator.RandomAbc(16);
			sessionTime = time;
			WriteSessionPrefs(false);
			Log("galaxy. generate new session id");
		}
		// USER <user_id> <user_password> <user_nick> <session_code> [<auth_code>]
		const string commandFormat = "USER {0} {1} {2} {3} {4}";
		var loginCommand = string.Format(commandFormat,
											userId,
											userPass,
											userName,
											authCode,
											sessionId);
		SendToServer(loginCommand);
	}

	private void cmdQuit(){
		if (connectCounter > 0){
			SendToServer("QUIT :ds");
			WriteSessionPrefs(true);
		}
	}
	
	private void cmdPrivmsg(string message){
		SendToServer("PRIVMSG 0 0 :"+message);
	}
	
	void Log(string message){
		if (DebugSettings.Me.DebugForGalaxy)
			print(message);
	}

}


