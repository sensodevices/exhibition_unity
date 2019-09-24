using UnityEngine;
using System;

public abstract class SensoPalm : MonoBehaviour {
	private WeakReference m_hand;
	public SensoHand Hand {
		get {
			if (m_hand != null && m_hand.IsAlive) return (m_hand.Target as SensoHand);
			else return null;
		}
	}
	///
	/// @brief Returns hand's type to which this palm belongs to
	public ESensoPositionType HandType { 
		get {
			if (Hand != null) return Hand.HandType;
			else return ESensoPositionType.Unknown;
		} 
	}
	///
	/// @brief Returns SensoManager which is controlling this palm
	public SensoManager Senso {
		get {
			return Hand.Senso;
		}
	}
	private SensoFingerTip[] fingers;
	public bool Grasping { get; private set; }

	public void Start ()
	{
		fingers = this.GetComponentsInChildren<SensoFingerTip>();
	}

	public void FixedUpdate()
	{
		int grasp = 0;
		foreach (var fing in fingers) {
			if (fing.FingerType == ESensoFingerType.Thumb) continue;
			float diffZ = fing.RelativePitch;
			if (diffZ > 180) diffZ = (360.0f - diffZ);
			if (diffZ >= 65 && diffZ <= 150) ++grasp;
		}
		if (!Grasping) {
			if (grasp >= 3) {
				var h = Hand;
				if (h != null) {
					h.TriggerGrab(grasp, false);
				}
				Grasping = true;
			}
		} else {
			if (grasp < 1) {
				var h = Hand;
				if (h != null) {
					h.TriggerGrab(0, true);
				}
				Grasping = false;
			}
		}
	}

	public void SetHand(SensoHand aHand)
	{
		m_hand = new WeakReference(aHand);
	}
}
