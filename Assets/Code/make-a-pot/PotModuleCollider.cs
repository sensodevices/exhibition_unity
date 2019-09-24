using UnityEngine;
using System;

public class PotModuleCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other) {
        var palm = other.GetComponent<SensoPalm>();
        if (palm != null) {
            var hand = palm.Hand;
            if (hand != null) {
                BroadcastMessage("SubscribeEvents", hand);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        var palm = other.GetComponent<SensoPalm>();
        if (palm != null) {
            var hand = palm.Hand;
            if (hand != null) {
                BroadcastMessage("UnsubscribeEvents", hand);
            }
        }
    }

}

