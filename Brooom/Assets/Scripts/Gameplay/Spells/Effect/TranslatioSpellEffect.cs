using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell which quickly relocates the racer who cast it farther in the direction of flying.
/// </summary>
public class TranslatioSpellEffect : VelocityAddingSpellEffect {

	/// <inheritdoc/>
	protected override Vector3 GetBaseVelocityNormalized() {
		return castParameters.SourceObject.transform.forward; // forward direction
	}

	/// <inheritdoc/>
	protected override CharacterMovementController GetTargetMovementController() {
		// Self
		return castParameters.SourceObject.GetComponentInChildren<CharacterMovementController>();
	}

}
