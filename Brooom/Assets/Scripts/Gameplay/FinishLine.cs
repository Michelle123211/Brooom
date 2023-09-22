using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour {

    private bool isActive = false; // only active finish line detects racers flying through it
    public void Activate() {
        isActive = true;
    }

    public void OnFinishLineTriggerEntered(Collider otherObject) {
        if (!isActive) return;
        otherObject.transform.parent.GetComponent<CharacterRaceState>()?.OnFinishPassed();
    }
}
