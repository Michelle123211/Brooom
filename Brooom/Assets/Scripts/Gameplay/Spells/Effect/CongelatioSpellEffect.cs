using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MovementAffectingSpellEffect : RacerAffectingSpellEffect {

	private CharacterMovementController targetRacer;

	protected override void StartSpellEffect_Internal() {
		this.targetRacer = null;
		if (castParameters.Target.TargetObject != null) targetRacer = castParameters.Target.TargetObject.GetComponent<CharacterMovementController>();
		if (castParameters.Target.TargetObject == null || targetRacer == null)
			throw new System.NotSupportedException($"{nameof(CongelatioSpellEffect)} may be casted only at an object with {nameof(CharacterMovementController)} component.");
		// Disable the racer's controls
		DisableRacerMovement(targetRacer);
	}

	protected override void StopSpellEffect_Internal() {
		// Allow the racer to move again
		if (targetRacer != null) targetRacer.EnableActions();
	}

	protected abstract void DisableRacerMovement(CharacterMovementController targetRacer);
}

// Spell for freezing an opponent
public class CongelatioSpellEffect : MovementAffectingSpellEffect {

	protected override void DisableRacerMovement(CharacterMovementController targetRacer) {
		// Stop movement completely and immediately
		targetRacer.DisableActions(CharacterMovementController.StopMethod.ImmediateStop);
	}

}
