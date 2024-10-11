using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for pushing an opponent away
public class FlanteSpellEffect : DurativeSpellEffect {

	[Tooltip("A velocity added to the target is changing according to this curve over time (values between 0 and 1).")]
	[SerializeField] AnimationCurve velocityTweenCurve;

	[Tooltip("The impact strength of the spell is equal to the magnitude of velocity added to the target.")]
	[SerializeField] float impactStrength = 20;

	CharacterMovementController targetMovementController;
	Vector3 velocity;

	protected override void ApplySpellEffect_OneIteration(float time) {
		// TODO: Move the racer a bit further in the corresponding direction
		targetMovementController.AddAdditionalVelocityForNextFrame(velocity * velocityTweenCurve.Evaluate(time)); // tweened
	}

	protected override void FinishApplyingSpellEffect() {

	}

	protected override void StartApplyingSpellEffect() {
		targetMovementController = castParameters.Target.TargetObject.GetComponentInChildren<CharacterMovementController>();
		velocity = impactStrength * castParameters.castDirection;
	}
}
