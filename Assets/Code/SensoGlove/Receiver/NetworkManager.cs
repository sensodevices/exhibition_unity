using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
    get { return "192.168.15.195"; }
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

    BroadcastMessage("SetNetworkManager", this);

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
    if (tcpState.sock != null) {
      tcpState.sock.Close();
      tcpState.sock = null;
    }
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
        var rotationArray = new float[] { 
          VRCamera.localRotation.w, VRCamera.localRotation.x, VRCamera.localRotation.y, VRCamera.localRotation.z,
          VRCamera.localPosition.x, VRCamera.localPosition.z, VRCamera.localPosition.y 
        };
        int len = rotationArray.Length * 4;
        byte[] msg = new byte[2 + len];
        msg[0] = 0x02;
        msg[1] = (byte)len;
        Buffer.BlockCopy(rotationArray, 0, msg, 2, len);

        tcpState.is_sending = true;
        try {
          tcpState.stream.BeginWrite(msg, 0, 2 + len, m_SendCb, tcpState);
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
      lastSensoData = DateTime.Now;
      aState.inBufferOffset += read;
      bool containsFullPacket = true;

      do {
        int packetType = (int)aState.inBuffer[0];
        int packetLen = (int)aState.inBuffer[1];
        int msgEnd = (packetLen + 2);
        if (aState.inBufferOffset >= msgEnd) {
          containsFullPacket = true;
          if (packetType == 1) {
            handData.ParseRawData(ref aState.inBuffer, 2);
          }

          // Remove packet from buffer
          if (msgEnd < aState.inBufferOffset) {
            Buffer.BlockCopy(aState.inBuffer, msgEnd, aState.inBuffer, 0, aState.inBufferOffset - msgEnd);
            aState.inBufferOffset = aState.inBufferOffset - msgEnd;
          } else {
            aState.inBufferOffset = 0;
          }
        } else {
          containsFullPacket = false;
        }
      } while (containsFullPacket);
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
        byte[] msg = new byte[3];
        msg[0] = 0x3;
        msg[1] = 0x1;
        msg[2] = (byte)((fingerId << 5) | (duration & 0x1F));
        tcpState.is_sending = true;
        try {
          tcpState.stream.BeginWrite(msg, 0, 3, m_SendCb, tcpState);
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
