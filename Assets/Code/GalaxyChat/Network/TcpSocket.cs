using System;
using System.IO;
using System.Net.Sockets;

public enum TcpError{
	Read, Write, Connect
}

public class TcpErrorEventArgs : EventArgs {
	public TcpError ErrorType {get;private set;}
	public string Message {get;private set;}
	public TcpErrorEventArgs(TcpError type, string message){
		ErrorType = type;
		Message = message;
	}
}

public class TcpSocket {
	
	public delegate void MessageReceivedFunc(string message);
	public MessageReceivedFunc OnMessageReceived;
	
	public event EventHandler<TcpErrorEventArgs> OnError;
	
	bool socketReady;
	public TcpClient theClient;
	NetworkStream theStream;
	StreamWriter theWriter;
	Byte[] inStream;
	
	public void Update() {
		string serverSays = Read();
		if (serverSays != null) {
			if (OnMessageReceived != null)
				OnMessageReceived(serverSays);
		}
	}
	
	void InvokeOnError(TcpError type, string message){
		if (OnError != null)
			OnError(this, new TcpErrorEventArgs(type, message));
	}
	
	public void Connect(string host, int port) {
		try {
			theClient = new TcpClient();
			
			var result = theClient.BeginConnect(host, port, null, null);
			var success = result.AsyncWaitHandle.WaitOne(5000);// ждём 5 сек
			if (!success){
				InvokeOnError(TcpError.Connect, "Connection timeout");
				return;
			}
			theClient.EndConnect(result);

			theStream = theClient.GetStream();
			theWriter = new StreamWriter(theStream);
			inStream = new Byte[theClient.ReceiveBufferSize];
			theClient.SendTimeout = 5;
			socketReady = true;
		}
		catch (Exception e) {
			InvokeOnError(TcpError.Connect, e.ToString());
		}
		
	}
	
	public bool IsConnected{
		get { return theClient != null ? theClient.Connected : false; }
	}

	public void Write(string theLine) {
		if (!socketReady)
			return;
		try {
			String tmpString = theLine + "\r\n";
			theWriter.Write(tmpString);
			theWriter.Flush();
		} catch (Exception e){
			InvokeOnError(TcpError.Write, e.ToString());
		}
	}
	
	public string Read() {
		if (!socketReady)
			return null;
		try{
			if (theStream.DataAvailable) {
				var actual = theStream.Read(inStream, 0, inStream.Length);
				return System.Text.Encoding.UTF8.GetString(inStream, 0, actual);
			}
		} catch (Exception e){
			InvokeOnError(TcpError.Read, e.ToString());
		}
		return null;
	}
	
	// return actually read bytes count
	public int ReadBytes(Byte[] targetBuffer) {
		if (!socketReady)
			return 0;
		try {
			if (theStream.DataAvailable) {
				var count = theStream.Read(targetBuffer, 0, targetBuffer.Length);
				return count;
			}
		} catch (Exception e){
			InvokeOnError(TcpError.Read, e.ToString());
		}
		return 0;
	}
	
	public void Disconnect() {
		if (!socketReady)
			return;
		theWriter.Close();
		theClient.Close();
		socketReady = false;
	}
}
