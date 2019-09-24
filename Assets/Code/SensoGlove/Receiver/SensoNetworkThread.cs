using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using SimpleJSON;

///
/// @brief States of the network thread
public enum SensoNetworkState {
	SENSO_DISCONNECTED, SENSO_CONNECTING, SENSO_CONNECTED, SENSO_FAILED_TO_CONNECT, SENSO_ERROR, SENSO_FINISHED, SENSO_STATE_NUM
};

///
/// @brief Class that connects to the Senso Server and provides pose samples
///
public class SensoNetworkThread
{
    public SensoNetworkState State { get; private set; }
    private TcpClient m_sock;
    private NetworkStream m_stream;
    private IAsyncResult m_connectRes;
    public bool IsReceiving { get; private set; }
    private bool m_isStarted = false;

    private IPAddress m_ip;
    private Int32 m_port;

    //private Thread m_thread = null; //!< Thread where socket networking happend
    //private volatile bool m_isRunning; //!< flag if thread should continue running
    private int RECV_BUFFER_SIZE = 4096; //!< Size of the buffer for read operations
    private int SEND_BUFFER_SIZE = 4096; //!< Size of the buffer to send
    private Byte[] m_buffer;
    private Byte[] outBuffer;
    private int outBufferOffset;

    private LinkedList<SensoHandGesture> gestures;
    public int GesturesCount { get; private set; }

    private SensoHandData[] handSamples;

    private int[] batteryValues = { 0, 100, 100 };

    ///
    /// @brief Structure to store vibration packet
    private struct VibroPacket
    {
        private ushort m_duration;
        private byte m_strength;
        public bool changed { get; private set; }

        public void Change(ushort duration, byte strength) {
            m_duration = duration;
            m_strength = strength;
            changed = true;
        }

        public int GetJSON(ESensoPositionType hand, ESensoFingerType finger, ref Byte[] buf, ref int bufOffset) {
            var str = String.Format("{{\"dst\":\"{0}\",\"type\":\"vibration\",\"data\":{{\"type\":{1},\"dur\":{2},\"str\":{3}}}}}\n", (hand == ESensoPositionType.RightHand ? "rh" : "lh"), (int)finger, m_duration, m_strength);
            changed = false;
            return Encoding.ASCII.GetBytes(str, 0, str.Length, buf, bufOffset);
        }
    };

    private struct OrientationPacket
    {
        public bool changed { get; private set; }
        private float pitch;
        private float roll;
        private float yaw;
        public void Change (Vector3 newRotation) {
            pitch = newRotation.x;
            yaw = newRotation.y;
            roll = newRotation.z;
            changed = true;
        }
        public int GetJSON(ref Byte[] buf, ref int bufOffset) {
            var str = String.Format("{{\"type\":\"orientation\",\"data\":{{\"type\":\"hmd\",\"pitch\":{0},\"roll\":{1},\"yaw\":{2}}}}}\n", pitch * Mathf.Deg2Rad, roll * Mathf.Deg2Rad, yaw * Mathf.Deg2Rad);
            changed = false;
            return Encoding.ASCII.GetBytes(str, 0, str.Length, buf, bufOffset);
        }
    };

  private VibroPacket[] vibroPackets; //!< Vibration packets to send to server
  private System.Object vibroLock = new System.Object(); //!< Lock to use when working with vibroPackets

  private OrientationPacket orientationPacket;
  private System.Object orientationLock = new System.Object();

    ///
    /// @brief Default constructor
    ///
    public SensoNetworkThread (string host, Int32 port)
    {
        m_port = port;
        State = SensoNetworkState.SENSO_DISCONNECTED;
        m_buffer = new Byte[RECV_BUFFER_SIZE];
        outBuffer = new Byte[SEND_BUFFER_SIZE];
        handSamples = new SensoHandData[(int)ESensoPositionType.PositionsCount];
        for (int i = 0; i < (int)ESensoPositionType.PositionsCount; ++i) handSamples[i] = new SensoHandData();
        
        if (!IPAddress.TryParse(host, out m_ip))
        {
            State = SensoNetworkState.SENSO_ERROR;
            Debug.LogError("SensoManager: can't parse senso driver host");
        }
        
        vibroPackets = new VibroPacket[10]; // 5 for each hand. 1-5 = right hand, 6-10 = left hand
        for (int i = 0; i < vibroPackets.Length; ++i)
        {
            vibroPackets[i] = new VibroPacket();
        }
        gestures = new LinkedList<SensoHandGesture>();
        GesturesCount = 0;
    }

    ~SensoNetworkThread()
    {
        StopThread();
    }

    public void StartThread()
    {
        if (!m_isStarted)
        {
            m_isStarted = true;
            connect();
        }
    }

    public void StopThread()
    {
        if (m_isStarted)
        {
            m_isStarted = false;
            disconnect();
        }
    }

    private void connect()
    {
        if (State == SensoNetworkState.SENSO_DISCONNECTED)
        {
            m_sock = new TcpClient(AddressFamily.InterNetwork);
            m_sock.NoDelay = true;
            var connectCB = new AsyncCallback(ProcessConnectResult);
            m_connectRes = m_sock.BeginConnect(m_ip, m_port, connectCB, null);
            State = SensoNetworkState.SENSO_CONNECTING;
        }
    }

    private void disconnect()
    {
        if (State == SensoNetworkState.SENSO_CONNECTING)
        {
            m_sock.EndConnect(m_connectRes);
        }
        else if (State == SensoNetworkState.SENSO_CONNECTED)
        {
            m_sock.Close();
        }
        m_stream = null;
        IsReceiving = false;
        State = SensoNetworkState.SENSO_DISCONNECTED;
    }

    // The following method is called when each asynchronous operation completes.
    void ProcessConnectResult(IAsyncResult result)
    {
        try
        {
            m_sock.EndConnect(result);
        } catch (Exception ex)
        {
            // Eat the exception
        }
        if (m_sock.Connected)
        {
            //Debug.Log("Connected to Senso Server");
            m_stream = m_sock.GetStream();
            State = SensoNetworkState.SENSO_CONNECTED;
        }
        else
        {
            //Debug.Log("Unable to connect to Senso Server");
            m_stream = null;
            State = SensoNetworkState.SENSO_DISCONNECTED;
        }
    }

    public void UpdateData()
    {
        if (State == SensoNetworkState.SENSO_DISCONNECTED && m_isStarted)
        {
            connect();
        }
        else if (State == SensoNetworkState.SENSO_CONNECTED)
        {
            ReadNetwork();
        }
    }

    private void ReadNetwork()
    {
        if (!m_sock.Connected) disconnect();
        if (IsReceiving || m_stream.DataAvailable)
        {
            // send out messages
            outBufferOffset = 0;
            netSendVibro(ref outBuffer, ref outBufferOffset);
            netSendOrientation(ref outBuffer, ref outBufferOffset);
            if (outBufferOffset > 0)
            {
                try
                {
                    m_stream.Write(outBuffer, 0, outBufferOffset);
                }
                catch (Exception ex)
                {
                    disconnect();
                    return;
                }
            }

            // Read incoming messages
            int readSz = -1;
            try {
                readSz = m_stream.Read(m_buffer, 0, RECV_BUFFER_SIZE);
            } catch (Exception ex)
            {
                Debug.LogError("Error reading socket: " + ex.Message);
            }
            int packetStart = 0;
            if (readSz > 0)
            {
                for (int i = 0; i < readSz; ++i)
                    if (m_buffer[i] == '\n')
                    {
                        if (ESensoPositionType.Unknown != processJsonStr(Encoding.ASCII.GetString(m_buffer, packetStart, i - packetStart - 1)))
                        {
                            if (!IsReceiving) IsReceiving = true;
                        }
                        packetStart = i + 1;
                    }
            }
            else
            {
                disconnect();
            }
        }
    }

    ///
    /// @brief Returns current sample of the specified hand
    ///
    public SensoHandData GetSample (ESensoPositionType handType) {
        return handSamples[(int)handType];
    }

    public int GetBatteryValue(ESensoPositionType handType)
    {
        return batteryValues[(int)handType];
    }
    ///
    /// @brief Parses JSON packet received from server
    ///
    private ESensoPositionType processJsonStr (string jsonPacket)
    {
        ESensoPositionType receivedType = ESensoPositionType.Unknown;
        JSONNode parsedData = null;
	        try {
		        parsedData = JSON.Parse(jsonPacket);
	        } catch (Exception ex) {
            Debug.LogError("packet " + jsonPacket + " parse error: " + ex.Message);
        }
        if (parsedData != null)
        {
            if (parsedData["type"].Value.Equals("position"))
            {
                var dataNode = parsedData["data"];
                receivedType = getPositionFromString(dataNode["type"].Value);
                try {
                    handSamples[(int)receivedType].parseJSONNode(dataNode);
                } catch (Exception ex) {
                    receivedType = ESensoPositionType.Unknown;
                    Debug.LogError(ex.Message);
                }
            } else if (parsedData["type"].Value.Equals("gesture")) {
                SensoHandGesture aGesture = new SensoHandGesture(parsedData["data"]);
                gestures.AddLast(aGesture);
                ++GesturesCount;
            }
            else if (parsedData["type"].Value.Equals("battery"))
            {
                var dataNode = parsedData["data"];
                int ind = (int)getPositionFromString(dataNode["type"].Value);
                batteryValues[ind] = dataNode["level"].AsInt;
            }
            else
            {
                Debug.Log("Received unknown type: " + parsedData["type"]);
            }
        }
        return receivedType;
    }

    ///
    /// @brief Send vibrating command to the server
    ///
    public void VibrateFinger (ESensoPositionType handType, ESensoFingerType fingerType, ushort duration, byte strength) {
        int handMult = handType == ESensoPositionType.RightHand ? 0 : 1;
        int ind = handMult * 5 + (int)fingerType;
        lock(vibroLock)
            vibroPackets[ind].Change(duration, strength);
    }

    private void netSendVibro (ref Byte[] buf, ref int offset)
    {
        ESensoPositionType hand = ESensoPositionType.Unknown;
        ESensoFingerType finger = ESensoFingerType.Thumb;
        lock(vibroLock) {
            for (int i = 0; i < 10; ++i) {
                if (i % 5 == 0)
                {
                    ++hand;
                    finger = ESensoFingerType.Thumb;
                }
                if (vibroPackets[i].changed) {
                    offset += vibroPackets[i].GetJSON(hand, finger, ref buf, ref offset);
                }
                ++finger;
            }
        }
    }

    private void netSendOrientation (ref Byte[] buf, ref int offset) {
        if (orientationPacket.changed) {
            lock(orientationLock)
            offset += orientationPacket.GetJSON(ref buf, ref offset);
        }
    }

    ///
    /// @brief Receive gestures from server
    ///
    public SensoHandGesture[] GetGestures () {
        if (GesturesCount > 0) {
            SensoHandGesture[] receivedGestures = new SensoHandGesture[GesturesCount];
            var enumerator = gestures.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); ++i) {
                receivedGestures[i] = enumerator.Current;
            }
            gestures.Clear();
            GesturesCount = 0;
            return receivedGestures;
        } else {
            return null;
        }
    }

    ///
    /// @brief Sends HMD orientation to Senso Server
    ///
    public void SendHMDOrientation (Vector3 newRotation) {
        lock(orientationLock)
            orientationPacket.Change(newRotation);
    }

    private ESensoPositionType getPositionFromString(String str)
    {
        if (str.Equals("rh")) return ESensoPositionType.RightHand;
        if (str.Equals("lh")) return ESensoPositionType.LeftHand;
        return ESensoPositionType.Unknown;
    }
}
