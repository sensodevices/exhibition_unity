using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleJSON;

public class NetworkManager : MonoBehaviour 
{
  public Transform VRCamera;

  private class TcpState 
  {
      public TcpClient sock;
      public NetworkStream stream;
      public Byte[] inBuffer;

      public int inBufferOffset;
      
      public bool ready;
      public bool is_sending;
      public bool waitConnect;
      public DateTime lastConnectRetry;
  }
   
  private string sensoHost {
    get { return "192.168.15.79"; }
  } 
  private int sensoPort {
    get { return 13456; }
  }
  public Canvas GameSettingsMenu; // prefab
    
  private TcpState tcpState;
  private HandNetworkData handData; // Storage for hands data
  private AsyncCallback m_ConnectCallback, m_SendCb, m_ReadCb;

  private DateTime lastVRCameraSent = DateTime.Now;
  private DateTime lastSensoData = DateTime.Now;
  
	void Start ()
  {
  	handData = new HandNetworkData();

    m_ConnectCallback = new AsyncCallback(ConnectCallback);
    m_SendCb = new AsyncCallback(SendCallback);
    m_ReadCb = new AsyncCallback(ReceiveCallback);

    tcpState = new TcpState();    
    tcpState.inBufferOffset = 0;
    tcpState.inBuffer = new Byte[1024];
    tcpState.ready = false;
    tcpState.is_sending = false;
    do_connect();
  }

  private void do_connect()
  {
    tcpState.lastConnectRetry = DateTime.Now;
    IPAddress anIP;

    if (IPAddress.TryParse(sensoHost, out anIP)) {
      tcpState.waitConnect = true;
      if (tcpState.sock == null) {
        tcpState.sock = new TcpClient(AddressFamily.InterNetwork);
      }
      tcpState.sock.BeginConnect(anIP, sensoPort, m_ConnectCallback, tcpState);
    } else {
      Debug.LogError("Unable to parse senso server IP address");
    }
  }
  
  public void Restart()
  {
    //TODO: Restart network manager
    Debug.Log("Restart network manager");
    Stop();
    do_connect();
  }

  public void Stop()
  {
    if (tcpState.stream != null) {
      tcpState.stream.Close();
    }
    tcpState.sock.Close();
    tcpState.sock = null;
    tcpState.ready = false;
    tcpState.is_sending = false;

    tcpState.inBufferOffset = 0;
  }

  void OnDestroy() 
  {
    Stop();
	}
  
  void FixedUpdate()
  {
    if (tcpState.ready) {
      if (tcpState.sock.Client.Connected) {
        /*if (tcpState.sock.Client.Poll(1, SelectMode.SelectRead) && tcpState.sock.Client.Available == 0) {
          HandleDisconnect(1);
          return;
        }*/
      } else {
        HandleDisconnect(2);
        return;
      }
      var now = DateTime.Now;
      var dt = now - lastVRCameraSent;
      var dataDT = now - lastSensoData;
      if (dataDT.TotalMilliseconds > 1000) {
        do_read(tcpState);
      }
      if (!tcpState.is_sending && dt.TotalMilliseconds >= 100) {
        var rotationArray = new float[] { VRCamera.rotation.w, VRCamera.rotation.x, VRCamera.rotation.y, VRCamera.rotation.z };
        byte[] msg = new byte[19];
        msg[0] = 0x45;
        msg[1] = 0x67;
        msg[2] = 0x02;
        Buffer.BlockCopy(rotationArray, 0, msg, 3, rotationArray.Length * 4);

        tcpState.is_sending = true;
        try {
          tcpState.stream.BeginWrite(msg, 0, 19, m_SendCb, tcpState);
        } catch (Exception err) {
          HandleDisconnect(3);
        } finally {
          lastVRCameraSent = now;
        }
      }
    } else if (!tcpState.waitConnect) {
      var diff = DateTime.Now - tcpState.lastConnectRetry;
      if (diff.TotalSeconds > 5) {
        do_connect();
      }
    }
  }
  
  /// <summary>Handler for connect event</summary>  
  private void ConnectCallback(IAsyncResult ar) 
  {
    var aState = (TcpState)ar.AsyncState;
    aState.waitConnect = false;
    var tcpClient = (TcpClient)aState.sock;
    if (tcpClient.Connected) {
      tcpState.stream = tcpClient.GetStream();
      tcpState.ready = true;
      do_read(tcpState);
    }
  }

  private void SendCallback(IAsyncResult ar)
  {
    ((TcpState)(ar.AsyncState)).is_sending = false;
  }

  private void ReceiveCallback(IAsyncResult ar)
  {
    var aState = (TcpState)(ar.AsyncState);
    int read = aState.stream.EndRead(ar);
    if (read == 0) { HandleDisconnect(4); return; }
    //
    aState.inBufferOffset += read;

    int msgEnd = aState.inBufferOffset - 1, msgStart = 0;
    for ( ; msgEnd >= 0; --msgEnd)
    {
      if (aState.inBuffer[msgEnd] == '\0') {
        for (msgStart = msgEnd - 1; msgStart >= 0; --msgStart) {
          if (aState.inBuffer[msgStart] == '\0') {
            break;
          }
        }
        break;
      }
    }
    if (msgStart == -1) msgStart = 0;

    if (msgEnd != msgStart) {
      String text = Encoding.UTF8.GetString(aState.inBuffer, msgStart, msgEnd - msgStart);
          
      JSONNode parsedPacket = null;
      try {
        parsedPacket = JSON.Parse(text);
      } catch (Exception je) {
        Debug.Log("Error parsing: " + je.Message);
      }
      if (parsedPacket != null) {
        lastSensoData = DateTime.Now;
        var leftHand = parsedPacket["lh"];
        if (leftHand != null )
          handData.DeserializeHand(HandNetworkData.DataType.LeftHand, leftHand.AsObject);

        var rightHand = parsedPacket["rh"];
        if (rightHand != null )
          handData.DeserializeHand(HandNetworkData.DataType.RightHand, rightHand.AsObject);
      }

      if (msgEnd < aState.inBufferOffset) {
        Buffer.BlockCopy(aState.inBuffer, msgEnd + 1, aState.inBuffer, 0, aState.inBufferOffset - msgEnd - 1);
        aState.inBufferOffset = aState.inBufferOffset - msgEnd - 1;
      } else {
        aState.inBufferOffset = 0;
      }
    }
    do_read(aState);
  }

  private void do_read(TcpState aState)
  {
    try {
      aState.stream.BeginRead(aState.inBuffer, aState.inBufferOffset, aState.inBuffer.Length - aState.inBufferOffset, m_ReadCb, aState);
    } catch (Exception err) {
      HandleDisconnect(5);
    }
  }
  
  public HandNetworkData.SingleHandData GetHandData(HandNetworkData.DataType handType)
  {
    if (handData == null) return null;
    if (handType == HandNetworkData.DataType.LeftHand) {
      return handData.leftHand;
    } else {
      return handData.rightHand;
    }
  }

  public void VibrateFinger(HandNetworkData.DataType handType, byte fingerId, byte duration)
  {
    if (tcpState.ready && !tcpState.is_sending) {
      byte[] msg = new byte[4];
      msg[0] = 0x45;
      msg[1] = 0x67;
      msg[2] = 0x01;
      msg[3] = (byte)((fingerId << 5) | (duration & 0x1F));
      tcpState.is_sending = true;
      try {
        tcpState.stream.BeginWrite(msg, 0, 4, m_SendCb, tcpState);
      } catch (Exception err) {
        HandleDisconnect(6);
      }
    }
  }

  /// <summary>Opens a menu to set up the Senso runtime</summary>
	public void OpenSettings() 
	{
		var gameMenu = (Canvas)Instantiate(GameSettingsMenu, new Vector3(0, 0, 0), Quaternion.identity);
		var setMan = gameMenu.GetComponent<SettingsManager>();
		setMan.netManager = this.GetComponent<NetworkManager>();
	}


  private void HandleDisconnect(int reason)
  {
    if (tcpState.ready) {
      Stop();
    }
  }
}
