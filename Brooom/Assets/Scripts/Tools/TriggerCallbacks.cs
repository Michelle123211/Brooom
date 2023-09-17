using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider))]
public class TriggerCallbacks : MonoBehaviour {
	[Tooltip("To which layer should the object, activating the trigger, belong.")]
	[SerializeField] LayerMask otherObjectLayer;

	[Tooltip("What should happen when an object with the given tag enters the trigger.")]
    [SerializeField] UnityEvent onTriggerEnterCallback;
	[Tooltip("What should happen when an object with the given tag exits the trigger.")]
	[SerializeField] UnityEvent onTriggerExitCallback;

	private void OnTriggerEnter(Collider other) {
		if (((1 << other.gameObject.layer) & otherObjectLayer) != 0) {
			if (!Utils.IsNullEvent(onTriggerEnterCallback))
				onTriggerEnterCallback.Invoke();
		}	
	}

	private void OnTriggerExit(Collider other) {
		if (((1 << other.gameObject.layer) & otherObjectLayer) != 0) {
			if (!Utils.IsNullEvent(onTriggerExitCallback))
				onTriggerExitCallback.Invoke();
		}
	}
}
