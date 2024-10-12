using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for pushing an opponent away
public class FlanteSpellEffect : VelocityAddingSpellEffect {

	protected override Vector3 GetBaseVelocityNormalized() {
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

	protected override void ApplySpellEffect_OneIteration(float time) {
	}

	protected override void FinishApplyingSpellEffect() {
	}

	protected override void StartApplyingSpellEffect() {
		// Add additional velocity to the target racer
		GetTargetMovementController().additionalVelocity.AddAdditionalVelocity(
			new AdditionalVelocityTweened(impactStrength * GetBaseVelocityNormalized(), effectDuration, velocityTweenCurve)
		);
	}

	protected abstract CharacterMovementController GetTargetMovementController();
	protected abstract Vector3 GetBaseVelocityNormalized();
}
