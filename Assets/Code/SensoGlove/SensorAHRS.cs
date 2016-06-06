using UnityEngine;
using System;

/// <summary>Struct for storing single AHRS sensor</summary>
public class SensorAHRS 
{
  private Quaternion m_quat;
  public Quaternion quaternion { get { return m_quat; } }
  
  public float yaw { get { return m_quat.eulerAngles.y; } }
	public float pitch { get { return m_quat.eulerAngles.x; } }
	public float roll { get { return m_quat.eulerAngles.z; } }

  /// <summary>Parses byte array into SensorAHRS struct</summary>
  public int Parse(ref byte[] buffer, int offset) 
  {
    int o = 0;
    m_quat.w = BitConverter.ToSingle(buffer, offset + o); o += 4;
    m_quat.x = BitConverter.ToSingle(buffer, offset + o); o += 4;
    m_quat.z = BitConverter.ToSingle(buffer, offset + o); o += 4;
    m_quat.y = -BitConverter.ToSingle(buffer, offset + o); o += 4;
    return o;
  }

  public void UnityDebug()
  {
	  Debug.Log("yaw: " + (yaw * 180.0 / Math.PI) + " pitch: " + (pitch * 180.0 / Math.PI) + " roll: " + (roll * 180.0 / Math.PI));
  }
}