using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for relocating further in the direction of flying
public class TranslatioSpellEffect : VelocityAddingSpellEffect {

	protected override Vector3 GetBaseVelocityNormalized() {
		return castParameters.SourceObject.transform.forward; // forward direction
	}

	protected override CharacterMovementController GetTargetMovementController() {
		return castParameters.SourceObject.GetComponentInChildren<CharacterMovementController>();
	}

}
