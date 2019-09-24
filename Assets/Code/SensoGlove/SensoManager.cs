using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class SensoManager : MonoBehaviour 
{
    private bool m_rightEnabled = false;
    private bool m_leftEnabled = false;
    public Transform[] Hands;

    private LinkedList<SensoHand> sensoHands;

    private string GetSensoHost () {
        return searchCLIArgument("sensoHost", SensoHost);
    }
    private Int32 GetSensoPort () {
        var portStr = searchCLIArgument("sensoPort", SensoPort.ToString());
        Int32 port;
        if (!Int32.TryParse(portStr, out port)) {
            port = SensoPort;
        }
        return port;
    }
    public string SensoHost = "127.0.0.1"; //!< IP address of the Senso Server instane
    public Int32 SensoPort = 53450; //!< Port of the Senso Server instance

    private SensoNetworkThread sensoThread;

    public Transform orientationSource;
    private DateTime orientationNextSend;
    private double orientationSendEveryMS = 100.0f;

    public Transform VRCameraHolder;

    void Start () {
        if (Hands != null && Hands.Length > 0) {
            sensoHands = new LinkedList<SensoHand>();
            for (int i = 0; i < Hands.Length; ++i) {
                Component[] components = Hands[i].GetComponents(typeof(SensoHand));
                for (int j = 0; j < components.Length; ++j) {
                    var hand = components[j] as SensoHand;
                    sensoHands.AddLast(hand);
                    if (!m_rightEnabled && hand.HandType == ESensoPositionType.RightHand) m_rightEnabled = true;
                    else if (!m_leftEnabled && hand.HandType == ESensoPositionType.LeftHand) m_leftEnabled = true;
                }
            }
        }
        sensoThread = new SensoNetworkThread(GetSensoHost(), GetSensoPort());
        sensoThread.StartThread();
        BroadcastMessage("SetSensoManager", this);
        /*if (orientationSource != null)
        {
            var aRig = orientationSource.GetComponent<OVRCameraRig>();
            if (aRig != null)
            {
                aRig.UpdatedAnchors += OrientationUpdated;
            }
        }*/
    }

    void OnDisable () {
        sensoThread.StopThread();
    }

    void Update () {
        sensoThread.UpdateData();
        SensoHandData leftSample = null, rightSample = null;

        if (m_rightEnabled) {
            rightSample = sensoThread.GetSample(ESensoPositionType.RightHand);
        }
        if (m_leftEnabled) {
            leftSample = sensoThread.GetSample(ESensoPositionType.LeftHand);
        }
        if (sensoHands != null) {
            foreach (var hand in sensoHands) {
                if (hand.HandType == ESensoPositionType.RightHand) {
                    hand.SensoPoseChanged(rightSample);
                } else if (hand.HandType == ESensoPositionType.LeftHand) {
                    hand.SensoPoseChanged(leftSample);
                }
            }
        }
        
        // Gestures
        var gestures = sensoThread.GetGestures();
        if (gestures != null) {
            for (int i = 0; i < gestures.Length; ++i) {
                if (gestures[i].Type == ESensoGestureType.PinchStart || gestures[i].Type == ESensoGestureType.PinchEnd) {
                    fingerPinch(gestures[i].Hand, gestures[i].Fingers[0], gestures[i].Fingers[1], gestures[i].Type == ESensoGestureType.PinchEnd);
                }
            }
        }
    }

    /*void OrientationUpdated(OVRCameraRig rig)
    {
        if (VRCameraHolder != null)
        {
            VRCameraHolder.localRotation = Quaternion.Inverse(rig.centerEyeAnchor.localRotation);
        }
        if (DateTime.Now >= orientationNextSend)
        {
            sensoThread.SendHMDOrientation(rig.centerEyeAnchor.localEulerAngles);
            orientationNextSend = DateTime.Now.AddMilliseconds(orientationSendEveryMS);
        }
    }*/

	///
	/// @brief Send vibration command to the server
	///
    public void SendVibro(ESensoPositionType hand, ESensoFingerType finger, ushort duration, byte strength)
    {
        sensoThread.VibrateFinger(hand, finger, duration, strength);
    }

    ///
    /// @brief Searches for the parameter in arguments list
    ///
    private static string searchCLIArgument (string param, string def = "")
    {
        if (Application.platform == RuntimePlatform.Android) {
            return def;
        }
        var args = System.Environment.GetCommandLineArgs();
        int i;
        string[] searchArgs = { param, "-" + param, "--" + param };

        for (i = 0; i < args.Length; ++i) {
            if (Array.Exists(searchArgs, elem => elem.Equals(args[i])) && args.Length > i + 1 ) {
                return args[i + 1];
            }
        }
        return def;
    }

    /// Events
    public void fingerPinch(ESensoPositionType handType, ESensoFingerType finger1Type, ESensoFingerType finger2Type, bool stop = false)
    {
        SensoHand aHand = null;
        foreach (var hand in sensoHands) 
            if (hand.HandType == handType) {
                aHand = hand;
                break;
            }
        
        if (aHand != null) {
            aHand.TriggerPinch(finger1Type, finger2Type, stop);
        }
    }

    /// 
    /// @brief Returns current battery value for the glove
    /// 
    public int GetBattery(ESensoPositionType pos)
    {
        return sensoThread.GetBatteryValue(pos);
    }

}