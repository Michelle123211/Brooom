using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour {

	public event Action onPlayerEntered;
	public event Action OnPlayerExited;

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player") && !other.isTrigger) { // only actual collider, not trigger around player to detect potential spell targets
			onPlayerEntered?.Invoke();
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.CompareTag("player") && !other.isTrigger) { // only actual collider, not trigger around player to detect potential spell targets
			OnPlayerExited?.Invoke();
		}
	}

}
