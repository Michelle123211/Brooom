using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// A component allowing to add listeners to <c>UnityEvent</c>s which are invoked when an arbitrary object, 
/// belonging to one of the considered layers, enters or exits an associated trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerCallbacks : MonoBehaviour {

	[Tooltip("To which layer should the object, activating the trigger, belong.")]
	[SerializeField] LayerMask otherObjectLayer;

	[Tooltip("What should happen when an object belonging to the required layer enters the trigger. Parameter is the collider of the object.")]
	[SerializeField] UnityEvent<Collider> onTriggerEnterCallback;
	[Tooltip("What should happen when an object belonging to the required layer exits the trigger. Parameter is the collider of the object.")]
	[SerializeField] UnityEvent<Collider> onTriggerExitCallback;

	private void OnTriggerEnter(Collider other) {
		// Check if the object belongs to one of the considered layers
		if (((1 << other.gameObject.layer) & otherObjectLayer) != 0) {
			// Invoke callbacks
			if (!Utils.IsNullEvent(onTriggerEnterCallback))
				onTriggerEnterCallback.Invoke(other);
		}	
	}

	private void OnTriggerExit(Collider other) {
		// Check if the object belongs to one of the considered layers
		if (((1 << other.gameObject.layer) & otherObjectLayer) != 0) {
			// Invoke callbacks
			if (!Utils.IsNullEvent(onTriggerExitCallback))
				onTriggerExitCallback.Invoke(other);
		}
	}

}
