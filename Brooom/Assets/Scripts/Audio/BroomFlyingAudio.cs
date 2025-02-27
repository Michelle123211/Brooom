using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomFlyingAudio : MonoBehaviour {

	private CharacterMovementController movementController;
	private FMODUnity.StudioEventEmitter broomSound;

	void Update() {
		// Set Speed parameter
		float currentSpeedNormalized = movementController.GetCurrentSpeed() / CharacterMovementController.MAX_SPEED;
		broomSound.SetParameter("Speed", currentSpeedNormalized);
	}

	private void Start() {
		movementController = GetComponent<CharacterMovementController>();
		broomSound = GetComponent<FMODUnity.StudioEventEmitter>();
	}

}
