using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour {

	public event Action onTutorialTriggerEnter;
	public event Action onTutorialTriggerExit;

	private void OnTriggerEnter(Collider other) {
		onTutorialTriggerEnter?.Invoke();
	}

	private void OnTriggerExit(Collider other) {
		onTutorialTriggerExit?.Invoke();
	}

}
