using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

///
/// @brief States of the network thread
/*public enum SensoNetworkState
{
    SENSO_DISCONNECTED, SENSO_CONNECTING, SENSO_CONNECTED, SENSO_FAILED_TO_CONNECT, SENSO_ERROR, SENSO_FINISHED, SENSO_STATE_COUNT
};*/

public enum EyeIndex
{
    NO, LEFT, RIGHT, EYE_CNT
};

public class NetworkEyeConnection
{
    private TcpClient m_sock;
    private SensoNetworkState m_state;
    private IAsyncResult m_connectRes;
    private NetworkStream m_stream;
    public bool IsReceiving { get; private set; }

    private IPAddress m_ip;
    private Int32 m_port;

    static public int RECV_BUFFER_SIZE = 10 * 1024 * 1024; //!< Size of the buffer for read operations

    // Use this for initialization
    public NetworkEyeConnection(string host, Int32 port)
    {
        m_state = SensoNetworkState.SENSO_DISCONNECTED;
        m_port = port;
        if (IPAddress.TryParse(host, out m_ip))
        {   
            doConnect();
        }
    }

    ~NetworkEyeConnection()
    {
        Disconnect();
    }

    private void doConnect()
    {
        m_sock = new TcpClient(AddressFamily.InterNetwork);
        m_sock.NoDelay = true;
        if (m_state == SensoNetworkState.SENSO_DISCONNECTED)
        {
            var connectCB = new AsyncCallback(ProcessConnectResult);
            m_connectRes = m_sock.BeginConnect(m_ip, m_port, connectCB, null);
            m_state = SensoNetworkState.SENSO_CONNECTING;
        }
    }

    public void Disconnect()
    {
        try
        {
            if (m_state == SensoNetworkState.SENSO_CONNECTING)
            {
                m_sock.EndConnect(m_connectRes);
            }
            else if (m_state == SensoNetworkState.SENSO_CONNECTED)
            {
                m_sock.Close();
            }
        } catch (Exception ex)
        {
            Debug.Log("Disconnect error: " + ex.Message);
        }
        IsReceiving = false;
        m_stream = null;
        m_sock = null;
        m_state = SensoNetworkState.SENSO_DISCONNECTED;
    }

    // The following method is called when each asynchronous operation completes.
    void ProcessConnectResult(IAsyncResult result)
    {
        try
        {
            m_sock.EndConnect(result);
        } catch (Exception ex)
        {
            // eat the exception
        }
        if (m_sock.Connected)
        {
            Debug.Log("Connected to Eye Server");
            m_sock.SendTimeout = 100;
            m_stream = m_sock.GetStream();
            m_state = SensoNetworkState.SENSO_CONNECTED;
        } else
        {
            Debug.Log("Unable to connect to Eye Server");
            m_stream = null;
            m_state = SensoNetworkState.SENSO_DISCONNECTED;
        }
    }

    public UInt16[] GetFrame(EyeIndex eyeId, ref Byte[] frameBytes)
    {
        UInt16[] dim = { 0, 0 };
        int sz;
        if (m_state == SensoNetworkState.SENSO_DISCONNECTED)
        {
            doConnect();
            return dim;
        }
        if (m_state != SensoNetworkState.SENSO_CONNECTED) return dim;
        var outBuf = BitConverter.GetBytes((Int32)eyeId);
        
        try
        {
            m_stream.Write(outBuf, 0, 4);
            m_stream.Read(frameBytes, 0, 4);
            dim[0] = BitConverter.ToUInt16(frameBytes, 0);
            dim[1] = BitConverter.ToUInt16(frameBytes, 2);
            sz = dim[0] * dim[1] * 4;
            int readSz = 0;
            while (readSz < sz)
            {
                readSz += m_stream.Read(frameBytes, readSz, sz - readSz);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Read stream error: " + ex.Message);
            Disconnect();
            dim[0] = 0; dim[1] = 0;
            return dim;
        }
        return dim;
    }

    public IAsyncResult BeginGetFrame(EyeIndex eyeId, AsyncCallback callback)
    {
        if (m_state == SensoNetworkState.SENSO_DISCONNECTED) doConnect();
        if (m_state != SensoNetworkState.SENSO_CONNECTED || IsReceiving) return null;
        var m_frameResult = new FrameGetResult(callback);
        var outBuf = BitConverter.GetBytes((Int32)eyeId);
        //var writeCB = new AsyncCallback(OnWriteResult);
        IsReceiving = true;
        try {
            m_stream.Write(outBuf, 0, 4);
            DoRead(m_stream, m_frameResult);
        }
        catch (Exception ex)
        {
            Disconnect();
        }
        return m_frameResult;
    }

    private void DoRead(NetworkStream aStream, FrameGetResult cb)
    {
        var readCB = new AsyncCallback(OnReadResult);
        try
        {
            aStream.BeginRead(cb.frameBytes, cb.bufOffset, RECV_BUFFER_SIZE - cb.bufOffset, readCB, cb);
        } catch (IOException ex)
        {
            Debug.LogError("Socket read exception: " + ex.Message);
            Disconnect();
        }
    }

    private void OnReadResult(IAsyncResult ar)
    {
        var fRes = (FrameGetResult)ar.AsyncState;
        int readBytes;
        try
        {
            readBytes = m_stream.EndRead(ar);
        } catch (Exception ex)
        {
            readBytes = -1;
        }
        if (readBytes == -1)
        {
            Disconnect();
            return;
        }
        fRes.bufOffset += readBytes;
        // Debug.Log("Read bytes: " + readBytes + " offset: " + fRes.bufOffset + " sz: " + fRes.frameSize);
        if (!fRes.IsCompleted)
        {
            DoRead(m_stream, fRes);
        }
        else // Receive done. Process the frame
        {
            IsReceiving = false;
            if (fRes.resultCallback != null)
            {
                fRes.resultCallback(fRes);
            }
        }
    }
}

public class FrameGetResult : IAsyncResult
{
    public Byte[] frameBytes { get; private set; }
    public int bufOffset;

    public AsyncCallback resultCallback { get; private set; }

    public int frameSize { get { return BitConverter.ToInt32(frameBytes, 0); } }

    public FrameGetResult(AsyncCallback cb)
    {
        frameBytes = new Byte[NetworkEyeConnection.RECV_BUFFER_SIZE];
        frameBytes[0] = 0;
        frameBytes[1] = 0;
        frameBytes[2] = 0;
        frameBytes[3] = 0;
        resultCallback = cb;
    }

    public bool IsCompleted
    {
        get {
            int frameSz = BitConverter.ToInt32(frameBytes, 0);
            if (frameSz == 0) return false;
            else return !(bufOffset < frameSz + 4);
        }
    }

    public WaitHandle AsyncWaitHandle
    {
        get { throw new NotImplementedException(); }
    }

    public object AsyncState
    {
        get {
            var imgBuf = new Byte[frameSize - 1];
            Array.Copy(frameBytes, 4, imgBuf, 0, frameSize - 1);
            return imgBuf;
        }
    }

    public bool CompletedSynchronously
    {
        get { return false; }
    }
}