using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component representing a finish line which racers have to fly through to finish the race.
/// </summary>
public class FinishLine : MonoBehaviour {

    private bool isActive = false; // only active finish line detects racers flying through it

    /// <summary>
    /// Activates the finish line, only then it detects racers flying through.
    /// </summary>
    public void Activate() {
        isActive = true;
    }

    /// <summary>
    /// Detects any racer flying through and calls a callback <c>onFinishPassed</c> on their <c>CharacterRaceState</c> component.
    /// It is called whenever an object enters the trigger zone.
    /// </summary>
    /// <param name="otherObject">Object entering the trigger zone.</param>
    public void OnFinishLineTriggerEntered(Collider otherObject) {
        if (!isActive) return;
        if (otherObject.isTrigger) return;
        GameObject rootObject = otherObject.GetComponent<ColliderRootObject>().GetRootObject();
        rootObject.GetComponent<CharacterRaceState>()?.OnFinishPassed(); // root object has CharacterRaceState component
    }

}
