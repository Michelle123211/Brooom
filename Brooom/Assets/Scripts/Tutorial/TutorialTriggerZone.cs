using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component representing a trigger zone used in a tutorial.
/// In some points during the tutorial, the player has to enter the zone to progress further.
/// Others may register callbacks on the player entering or exiting the trigger zone.
/// </summary>
public class TutorialTriggerZone : MonoBehaviour {

	/// <summary>Called when the player enters the zone.</summary>
	public event Action onPlayerEntered;
	/// <summary>Called when the player exits the zone.</summary>
	public event Action OnPlayerExited;


	// Checks if it is the player and invokes a callback if so
	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player") && !other.isTrigger) { // only actual collider, not trigger around player to detect potential spell targets
			onPlayerEntered?.Invoke();
		}
	}

	// Checks if it is the player and invokes a callback if so
	private void OnTriggerExit(Collider other) {
		if (other.CompareTag("player") && !other.isTrigger) { // only actual collider, not trigger around player to detect potential spell targets
			OnPlayerExited?.Invoke();
		}
	}

}
