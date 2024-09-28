using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for confusing an opponent
public class ConfusioneSpellEffect : MovementAffectingSpellEffect {

	protected override void DisableRacerMovement(CharacterMovementController targetRacer) {
		// Disable actions but without stopping so the racer keeps moving forward
		targetRacer.DisableActions(CharacterMovementController.StopMethod.NoStop);
	}

}
