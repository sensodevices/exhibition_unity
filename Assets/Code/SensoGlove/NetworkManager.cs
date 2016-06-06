using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleJSON;

public class NetworkManager : MonoBehaviour 
{
  private class UdpState 
  {
      public IPEndPoint e;
      public UdpClient u;
      public bool is_sending;
  }
    
  private int listenPort {
    get { return GameSettings.GetInboundPort(); }
  }
  private int vibratePort {
    get { return GameSettings.GetVibratePort(); }
  }
  public int sensorsCnt = 7;
  public Canvas GameSettingsMenu; // prefab
    
  private UdpClient udpClient = null;
  private UdpState udpState = null;
  private IPEndPoint vibrateEP; // EndPoint to send vibrate packets to
	
  private HandNetworkData handData; // Storage for hands data
      
  // Incoming buffer control
  private byte[] inBuffer;
  private int inBufferOffset;
  
  private AsyncCallback m_RcvCb;
  private AsyncCallback m_SendCb;
	
	void Start ()
  {
    inBuffer = new byte[1024];
    inBufferOffset = 0;
    
  	handData = new HandNetworkData();
		startUdpListener();
  }

  
  void OnDestroy() 
  {
    stopUdpListener();
	}
  
  private void startUdpListener()
  {
    IPEndPoint e = new IPEndPoint(IPAddress.Any, listenPort);
    udpClient = new UdpClient(e);
    udpState = new UdpState();
    udpState.e = e;
    udpState.u = udpClient;
    udpState.is_sending = false;
    m_RcvCb = new AsyncCallback(ReceiveCallback);
    udpClient.BeginReceive(m_RcvCb, udpState);
    
    m_SendCb = new AsyncCallback(SendCallback);
    vibrateEP = GameSettings.GetVibrateEndpoint();
  }
  
  private void stopUdpListener()
  {
    udpClient.Close();
    udpClient = null;
    udpState = null;
  }
  
  /// <summary>Restarts Udp socket with binding to a port stored in game settings</summary>
  public void Restart()
  {
    if (udpClient != null) {
      stopUdpListener();
    }
    startUdpListener();
  }
    
  private void ReceiveCallback(IAsyncResult ar) 
  {
    UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
    IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

    Byte[] receiveBytes = u.EndReceive(ar, ref e);
    Buffer.BlockCopy(receiveBytes, 0, inBuffer, inBufferOffset, receiveBytes.Length);
    inBufferOffset += receiveBytes.Length;
    
    for (int i = inBufferOffset - 1; i > 4; --i) {
      if (inBuffer[i - 3] == 0x7D && inBuffer[i - 2] == 0x5D && inBuffer[i - 1] == 0x7D && inBuffer[i] == 0x7D) {
        // found end of packet
        String text = Encoding.UTF8.GetString(inBuffer, 0, i + 1);
        JSONNode parsedPacket = null;
        try {
          parsedPacket = JSON.Parse(text);
        } catch (Exception je) {
          Debug.Log("Error parsing: " + je.Message);
        }
        if (parsedPacket != null) {
          var leftHand = parsedPacket["lh"];
          if (leftHand != null )
            handData.DeserializeHand(HandNetworkData.DataType.LeftHand, leftHand.AsObject);

          var rightHand = parsedPacket["rh"];
          if (rightHand != null )
            handData.DeserializeHand(HandNetworkData.DataType.RightHand, rightHand.AsObject);
        }
        
        if (i + 1 < inBufferOffset) {
          Buffer.BlockCopy(inBuffer, i + 1, inBuffer, 0, inBufferOffset - i - 1);
          inBufferOffset -= (i - 1);
          text = Encoding.UTF8.GetString(inBuffer, 0, inBufferOffset);
        } else {
          inBufferOffset = 0;
        }
        i = inBufferOffset - 1;
      }
    }
    udpClient.BeginReceive(m_RcvCb, udpState);
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
    if (vibrateEP != null && udpState.is_sending == false) {
      byte[] msg = new byte[3];
      msg[0] = 0x45;
      msg[1] = 0x67;
      msg[2] = (byte)((fingerId << 5) | (duration & 0x1F));
      
      udpState.is_sending = true;
      udpClient.BeginSend(msg, 3, vibrateEP, m_SendCb, udpState);
      // udpClient.Send(msg, 3, vibrateEP);
    }
  }
  
  private void SendCallback(IAsyncResult ar)
  {
    ((UdpState)(ar.AsyncState)).is_sending = false;
  }
  
  /// <summary>Opens a menu to set up the Senso runtime</summary>
	public void OpenSettings() 
	{
		var gameMenu = (Canvas)Instantiate(GameSettingsMenu, new Vector3(0, 0, 0), Quaternion.identity);
		var setMan = gameMenu.GetComponent<SettingsManager>();
		setMan.netManager = this.GetComponent<NetworkManager>();
	}

}
