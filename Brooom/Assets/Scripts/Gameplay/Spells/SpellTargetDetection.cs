using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTargetDetection : MonoBehaviour {

    [Tooltip("The maximum distance in which potential targets are detected. Trigger zone's radius is overriden with this value.")]
    [SerializeField] float maxDetectionDistance = 15;

    [Tooltip("SpellController component assigned to this racer.")]
    [SerializeField] SpellController spellController;

    [Tooltip("Trigger zone for detecting potential targets.")]
    [SerializeField] SphereCollider triggerZone;

	private void OnTriggerEnter(Collider other) {

	}

	private void OnTriggerExit(Collider other) {
		
	}

	private void Awake() {
        triggerZone.radius = maxDetectionDistance;
	}

}
