using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell immediately freezing a target opponent.
/// </summary>
public class CongelatioSpellEffect : MovementAffectingSpellEffect {

	/// <summary>
	/// Temporarily stops the target racer's movement completely and immediately by disabling its actions (with immediate stop) via <c>CharacterMovementController</c>.
	/// </summary>
	/// <param name="targetRacer">Racer whose movement should be stopped.</param>
	protected override void DisableRacerMovement(CharacterMovementController targetRacer) {
		// Stop movement completely and immediately
		targetRacer.DisableActions(CharacterMovementController.StopMethod.ImmediateStop);
	}

}


/// <summary>
/// A base class for a spell affecting movement of a target racer, e.g. limiting its actions or stopping it altogether.
/// </summary>
public abstract class MovementAffectingSpellEffect : RacerAffectingSpellEffect {

	private CharacterMovementController targetRacer;

	/// <inheritdoc/>
	protected override void StartSpellEffect_Internal() {
		this.targetRacer = null;
		if (castParameters.Target.TargetObject != null) targetRacer = castParameters.Target.TargetObject.GetComponent<CharacterMovementController>();
		if (castParameters.Target.TargetObject == null || targetRacer == null)
			throw new System.NotSupportedException($"{nameof(CongelatioSpellEffect)} may be cast only at an object with {nameof(CharacterMovementController)} component.");
		// Disable the racer's controls
		DisableRacerMovement(targetRacer);
	}

	/// <inheritdoc/>
	protected override void StopSpellEffect_Internal() {
		// Allow the racer to move again
		if (targetRacer != null) targetRacer.EnableActions();
	}

	/// <summary>
	/// Disables racer's movement in some way, using its <c>CharacterMovementController</c> component.
	/// </summary>
	/// <param name="targetRacer">Racer whose movement should be affected.</param>
	protected abstract void DisableRacerMovement(CharacterMovementController targetRacer);
}
