using UnityEngine;
using System;

public class PalmGraspedArgs : EventArgs
{
  private PalmTarget m_palm;
  public PalmTarget Palm { get { return m_palm; } }
  public PalmGraspedArgs(PalmTarget palm) {
    m_palm = palm;
  }
}

public class PalmTarget : MonoBehaviour {

	public GameObject[] fingers;
	private HandNetworkData.DataType m_handType;
	public HandNetworkData.DataType HandType { get { return m_handType; } }
	private bool m_grasping = false;
	public bool Grasping { get { return m_grasping; } }

	public event EventHandler<PalmGraspedArgs> onPalmGraspStart = delegate {}; // Is fired when two fingers' colliders exit each other
	public event EventHandler<PalmGraspedArgs> onPalmGraspEnd = delegate {}; // Is fired when two fingers' colliders exit each other


	public void Start()
	{
		BroadcastMessage("SetPalm", this);
	}
	public void FixedUpdate()
	{
		int grasp = 0;
		foreach (var fing in fingers) {
			float diffZ = Quaternion.Angle(transform.rotation, fing.transform.rotation);
			if (diffZ > 180) diffZ = (360.0f - diffZ);
			if (diffZ >= 90 && diffZ <= 150) ++grasp;
		}
		if (!m_grasping) {
			if (grasp >= 2) {
				var args = new PalmGraspedArgs(this);
    			onPalmGraspStart(this, args);
				m_grasping = true;
			}
		} else {
			if (grasp < 1) {
				var args = new PalmGraspedArgs(this);
    			onPalmGraspEnd(this, args);
				m_grasping = false;
			}
		}
	}

	/// <summary>Returns the position of the palm</summary>
	public Vector3 GetPosition () {
		return transform.position;
	}

	/// <summary>Returns the rotation of the palm</summary>
	public Quaternion GetRotation () {
		return transform.rotation;
	}

	
	public void SetHandType(HandNetworkData.DataType handType)
	{
		m_handType = handType;
	}

	public void ToggleVibrateAll(bool toggle)
	{
		foreach (var fing in fingers) {
			var t = fing.GetComponent<FingerTarget>();
			if (toggle) t.StartVibrate();
			else t.StopVibrate();
		}
	}
}
