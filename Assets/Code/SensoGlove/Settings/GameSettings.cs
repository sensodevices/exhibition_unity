using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class GameSettings {
  
  static public int GetInboundPort()
  {
    return PlayerPrefs.GetInt("data_port", 13456);
  }
  
  static public void SetInboundPort(int newPort)
  {
    if (newPort > 10000 && newPort < 65535)
    {
      PlayerPrefs.SetInt("data_port", newPort);
    }
    else
    {
      //TODO: throw an error
    }
  }
  
  static public int GetVibratePort()
  {
    return PlayerPrefs.GetInt("vibrate_port", 3002);
  }
  
  static public void SetVibratePort(int newPort)
  {
    if (newPort > 0 && newPort < 65535)
    {
      PlayerPrefs.SetInt("vibrate_port", newPort);  
    }
    else
    {
      //TODO: throw an error
    }
  }
  
  static public string GetVibrateHost()
  {
    return PlayerPrefs.GetString("vibrate_host", "127.0.0.1");
  }
  
  static public void SetVibrateHost(string newHost)
  {
    IPAddress vibrateAddr;
    if (IPAddress.TryParse(newHost, out vibrateAddr)) {
      PlayerPrefs.SetString("vibrate_host", newHost);  
    } else {
      //TODO: throw an error
    }
  }
  
  static public IPEndPoint GetVibrateEndpoint()
  {
    IPAddress vibrateAddr;
    if (IPAddress.TryParse(GetVibrateHost(), out vibrateAddr))
    {
      return new IPEndPoint(vibrateAddr, GetVibratePort());
    } else {
      return null;
    }
  }
  
  static public void Save()
  {
    PlayerPrefs.Save();
  }

}
