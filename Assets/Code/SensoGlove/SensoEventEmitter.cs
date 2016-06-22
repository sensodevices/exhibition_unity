using UnityEngine;
using System;

public class FingersCollidedEventArgs : EventArgs
{
  private FingerTarget[] m_fingers;
  public FingerTarget[] Fingers {
    get {
      return m_fingers;
    }
  }

  public FingersCollidedEventArgs(FingerTarget finger1, FingerTarget finger2)
  {
    m_fingers = new FingerTarget[2] { finger1, finger2 };
  }
}


/// <summary>Senso Glove event emitter</summary>
public class SensoEventEmitter : MonoBehaviour {

  public void Start()
  {
    BroadcastMessage("RegisterEventEmitter", this);
  }

  public event EventHandler<FingersCollidedEventArgs> onFingersCollided = delegate {}; // Is fired when two fingers' colliders entered each other
  public event EventHandler<FingersCollidedEventArgs> onFingersReleased = delegate {}; // Is fired when two fingers' colliders exit each other
  
  public void fingerCollided(FingerTarget finger1, FingerTarget finger2)
  {
    if (finger1.fingerId > finger2.fingerId) {
      FingerTarget _f = finger1;
      finger1 = finger2;
      finger2 = _f;
    }
    var args = new FingersCollidedEventArgs(finger1, finger2);
    onFingersCollided(this, args);
  }

  public void fingerReleased(FingerTarget finger1, FingerTarget finger2)
  {
    if (finger1.fingerId > finger2.fingerId) {
      FingerTarget _f = finger1;
      finger1 = finger2;
      finger2 = _f;
    }
    var args = new FingersCollidedEventArgs(finger1, finger2);
    onFingersReleased(this, args);
  }
}
