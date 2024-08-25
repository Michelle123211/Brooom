using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for freezing an opponent
public class CongelatioSpellEffect : RacerAffectingSpellEffect {

	CharacterMovementController targetRacer;

	protected override void StartSpellEffect_Internal() {
		this.targetRacer = null;
		if (castParameters.TargetObject != null) targetRacer = castParameters.TargetObject.GetComponent<CharacterMovementController>();
		if (castParameters.TargetObject == null || targetRacer == null)
			throw new System.NotSupportedException($"{nameof(CongelatioSpellEffect)} may be casted only at an object with {nameof(CharacterMovementController)} component.");
		// Disable the racer's controls and stop the racer
		targetRacer.DisableActions(CharacterMovementController.StopMethod.BrakeStop);
	}

	protected override void StopSpellEffect_Internal() {
		// Allow the racer to move again
		if (targetRacer != null) targetRacer.EnableActions();
	}

}
