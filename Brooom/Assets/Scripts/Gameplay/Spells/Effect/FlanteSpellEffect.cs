using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for pushing an opponent away
public class FlanteSpellEffect : VelocityAddingSpellEffect {

	protected override Vector3 GetInitialVelocityNormalized() {
		return castParameters.castDirection; // direction of casting the spell ~ similar to view direction for player
	}

	protected override CharacterMovementController GetTargetMovementController() {
		return castParameters.Target.TargetObject.GetComponentInChildren<CharacterMovementController>();
	}

}

public abstract class VelocityAddingSpellEffect : DurativeSpellEffect {

	[Tooltip("A velocity added to the target is changing according to this curve over time (values between 0 and 1).")]
	[SerializeField] AnimationCurve velocityTweenCurve;

	[Tooltip("The impact strength of the spell is equal to the magnitude of velocity added to the target.")]
	[SerializeField] float impactStrength = 20;

	CharacterMovementController targetMovementController;
	Vector3 velocity;

	protected override void ApplySpellEffect_OneIteration(float time) {
		// Move the racer a bit further in the corresponding direction
		targetMovementController.AddAdditionalVelocityForNextFrame(velocity * velocityTweenCurve.Evaluate(time)); // tweened
	}

	protected override void FinishApplyingSpellEffect() {

	}

	protected override void StartApplyingSpellEffect() {
		targetMovementController = GetTargetMovementController();
		velocity = impactStrength * GetInitialVelocityNormalized();
	}

	protected abstract CharacterMovementController GetTargetMovementController();
	protected abstract Vector3 GetInitialVelocityNormalized();
}
