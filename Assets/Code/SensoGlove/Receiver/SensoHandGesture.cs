using System.Collections.Generic;
using SimpleJSON;

public enum ESensoGestureType
{
    Unknown,
    PinchStart,
    PinchEnd,
    GesturesCount
};

public class SensoHandGesture
{
    public ESensoPositionType Hand { get; private set; }
    public ESensoFingerType[] Fingers { get; private set; }
    public ESensoGestureType Type { get; private set; }

    public SensoHandGesture (JSONNode data) {
        JSONArray arr = data["fingers"].AsArray;
        Fingers = new ESensoFingerType[arr.Count];
        for (int i = 0; i < arr.Count; ++i) {
            Fingers[i] = (ESensoFingerType)arr[i].AsInt;
        }

        string handStr = data["hand"].Value;
        if (handStr.Equals("rh")) Hand = ESensoPositionType.RightHand;
        else if (handStr.Equals("lh")) Hand = ESensoPositionType.LeftHand;
        else Hand = ESensoPositionType.Unknown;

        string typeStr = data["name"].Value;
        if (typeStr.Equals("pinch_start")) Type = ESensoGestureType.PinchStart;
        else if (typeStr.Equals("pinch_end")) Type = ESensoGestureType.PinchEnd;
        else Type = ESensoGestureType.Unknown;
    }
};