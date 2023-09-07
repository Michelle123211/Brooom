using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider))]
public class TriggerCallbacks : MonoBehaviour {

	[Tooltip("Which tag should the other object, activating the trigger, have.")]
	[SerializeField] string otherObjectTag = string.Empty;

	[Tooltip("What should happen when an object with the given tag enters the trigger.")]
    [SerializeField] UnityEvent onTriggerEnterCallback;
	[Tooltip("What should happen when an object with the given tag exits the trigger.")]
	[SerializeField] UnityEvent onTriggerExitCallback;

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag(otherObjectTag)) {
			if (!Utils.IsNullEvent(onTriggerEnterCallback))
				onTriggerEnterCallback.Invoke();
		}	
	}

	private void OnTriggerExit(Collider other) {
		if (other.CompareTag(otherObjectTag)) {
			if (!Utils.IsNullEvent(onTriggerExitCallback))
				onTriggerExitCallback.Invoke();
		}
	}
}
