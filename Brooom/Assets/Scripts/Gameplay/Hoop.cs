using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoop : MonoBehaviour {

    [HideInInspector] public bool playerDetected = false;

    private bool isActive = false; // when active the hoop detects the player flying through it
    private int numOfTriggersActive = 0; // how many triggers the player entered and have not exited

    // Player flying through the hoop is detected only if the hoop is active
    public void Activate() {
        isActive = true;
        // Reset
        playerDetected = false;
        numOfTriggersActive = 0;
    }

    public void StartHighlighting() { 
        // TODO: Highlight the hoop
    }

    public void StopHighlighting() { 
        // TODO: Stop highlighting the hoop
    }

    public void OnPlayerTriggerEntered() {
        if (!isActive) return;
        numOfTriggersActive++;
        if (numOfTriggersActive == 2) { // there are 2 triggers, both must be active simultaneously
            playerDetected = true;
        }
    }

    public void OnPlayerTriggerExited() {
        if (!isActive) return;
        numOfTriggersActive--;
        // Prevent eventual errors (when activating the hoop while the player is inside a trigger)
        if (numOfTriggersActive < 0) numOfTriggersActive = 0;
    }

}
