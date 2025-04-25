using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell confusing a target opponent in a way that for a limited time they cannot change their movement direction.
/// </summary>
public class ConfusioneSpellEffect : MovementAffectingSpellEffect {

	/// <summary>
	/// Temporarily disables target racer's actions (via <c>CharacterMovementController</c>) so that they only keep moving forward and cannot change direction.
	/// </summary>
	/// <param name="targetRacer">Racer whose movement should be disabled.</param>
	protected override void DisableRacerMovement(CharacterMovementController targetRacer) {
		// Disable actions but without stopping so the racer keeps moving forward
		targetRacer.DisableActions(CharacterMovementController.StopMethod.NoStop);
	}

}
