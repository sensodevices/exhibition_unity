using UnityEngine;
using System;

public abstract class SensoFingerTip : MonoBehaviour 
{
	private ushort vibrateMS = 1000;
	public float RelativePitch { get; private set; }

    public ESensoFingerType FingerType;
    public ESensoPositionType HandType { 
		get { 
			if (Hand == null) return ESensoPositionType.Unknown; 
			else return Hand.HandType; 
		} 
	}
	public SensoManager Senso {
		get {
			return Hand.Senso;
		}
	}

	public bool IsVibrating { get; protected set; }
	private bool m_newVibrating;
	private DateTime vibratingUntil;
	private byte vibratingStrength;

	private WeakReference m_hand;
	public SensoHand Hand { 
		get {
			if (m_hand.IsAlive) {
				return m_hand.Target as SensoHand;
			} else {
				return null;
			}
		}
	}
    
	protected virtual void FixedUpdate () {
		if (m_newVibrating != IsVibrating) {
			IsVibrating = m_newVibrating;
			if (!IsVibrating) Hand.VibrateFinger(FingerType, 0, 0);
		}

		if (IsVibrating) {
			if (DateTime.Now >= vibratingUntil) {
				Hand.VibrateFinger(FingerType, vibrateMS, vibratingStrength);
				vibratingUntil = DateTime.Now.AddMilliseconds((double)vibrateMS);
			}
		}
	}

    ///
    /// @brief Sets the type of the hand: right or left
    ///
	public void SetHand(SensoHand aHand) {
		m_hand = new WeakReference(aHand, false);
	}

	///
	/// @brief Function that starts finger vibration
	///
	/// Function that starts finger vibration for specified duration and strength.<br>
	/// Duration is specified in milliseconds.<br>
	/// Valid strength values are 1 - 10 inclusive<br>  
	/// To force stop vibration before *duration* ms has passed call this function with duration or strength = 0
	public void Vibrate(byte strength = 5)
	{
		m_newVibrating = (strength != 0);
		if (strength != vibratingStrength) vibratingStrength = strength;
	}

	public void StopVibrate()
	{
		Vibrate(0);
	}

	public void SetRelativePitch(float newPitch) {
		RelativePitch = newPitch;
	}

};