using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections;

public class Galaxy : MonoBehaviour {

	public string recoveryCode;
	public string Host = "galaxy.mobstudio.ru";
	public int Port = 6667;
	public Text textEntering, textPlanetName, textUsersCount;
	public Button buttonConnect;
	public InputField inputMessage;
	public ScrollRect chatScrollRect;
	public CameraHelper cameraHelper;
	public Planet planet;
	public Finger finger;
	public Chat chat;
	
	int userId;
	string userName, userPass, authCode;
	TcpSocket socket;
	TouchListener touch;
	
		
	void Start () {
		
		touch = GetComponent<TouchListener>();
		
		//planet = GetComponent<Planet>();
		planet.OnUsersCountChanged += OnUsersCountChanged;
		
		socket = new TcpSocket();
		socket.OnMessageReceived += OnMessageReceived;
		
		finger.OnSwipeChat += OnSwipeChat;
		finger.OnSwipePlanet += OnSwipePlanet;
		
		Connect();
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
	
	public void Connect(){
		ConnectInternal();
	}
	
	public void ConnectAsync(){
		StartCoroutine(ConnectInternal());
	} 
	
	IEnumerator ConnectInternal(){
		socket.Connect(Host, Port);
		textEntering.gameObject.SetActive(true);
		buttonConnect.gameObject.SetActive(false);
		//chatTitle.gameObject.SetActive(false);
		return null;
	} 
	
	void Disconnect(){
		cmdQuit();
		textEntering.gameObject.SetActive(false);
		buttonConnect.gameObject.SetActive(true);
		//chatTitle.gameObject.SetActive(false);
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
	
	void UpdateNetwork(){
		socket.Update();
	}
	
	Vector2 prevPos;
	void UpdateInput(){
		
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
	
	public void SendMessageFromInput(){
		var s = inputMessage.text.Trim();
		if (s != ""){
			cmdPrivmsg(s);
			chat.AddMessage(userName, s);
			inputMessage.text = "";
		}
	}
	
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
		Debug.Log(">> " + message);
	}
	
	void OnEnterChannel(){
		textEntering.gameObject.SetActive(false);
		textPlanetName.text = planet.Name;
		//chatTitle.gameObject.SetActive(true);
		PlayerPrefs.SetString("planetName", planet.Name);
	}
	
	void OnMessageReceived (string message) {
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
		//Debug.Log("<< '" + message + "'");
		Debug.Log("<< " + message);
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
				cmdRecover(recoveryCode);
				break;
				
			case "REGISTER": // добро на вход
				userId = c.Parameters[0].Trim().ToInt();
				userPass = c.Parameters[1].Trim();
				userName = c.Parameters[2].Trim();
				cmdUser();
				break;
				
			case "999": // auth OK
				cmdAddons();
				//PlayerPrefs.GetString("planetName", null);
				string s = null;//"pi100let"
				cmdJoin(s);
				break;
			
			case "PING":
				cmdPong();
				break;
			
			case "353": // список юзеров. может быть несколько таких команд
				planet.JoinUsers(c.Postfix);
				planet.SetName(c.Parameters[2]);
				break;
			
			case "366":
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
				MessageBox.Show("Error "+c.Name, c.Postfix);
				Disconnect();
				break;
			
			case "471": // планета переполнена
				MessageBox.Show("Error "+c.Name, c.Postfix);
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
				
			default:
				break;
		}
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
	
	private void cmdUser(){
		// USER <user_id> <user_password> <user_nick> <session_code> [<auth_code>]
		const string commandFormat = "USER {0} {1} {2} {3} {4}";
		var loginCommand = string.Format(commandFormat,
											userId,
											userPass,
											userName,
											authCode,
											SessionCodeGenerator.RandomAbc(16));
		SendToServer(loginCommand);
	}

	private void cmdQuit(){
		SendToServer("QUIT :ds");
	}
	
	private void cmdPrivmsg(string message){
		SendToServer("PRIVMSG 0 0 :"+message);
	}
	
}


