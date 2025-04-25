using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell pushing a target opponent away in the direction the spell was cast in.
/// </summary>
public class FlanteSpellEffect : VelocityAddingSpellEffect {

	/// <inheritdoc/>
	protected override Vector3 GetBaseVelocityNormalized() {
		// Direction of casting the spell (~ similar to view direction for player), already normalized
		return castParameters.castDirection;
	}

	/// <inheritdoc/>
	protected override CharacterMovementController GetTargetMovementController() {
		return castParameters.Target.TargetObject.GetComponentInChildren<CharacterMovementController>();
	}

}

/// <summary>
/// A base class for a spell adding velocity to a target racer, to move them in some direction.
/// </summary>
public abstract class VelocityAddingSpellEffect : DurativeSpellEffect {

	[Tooltip("A velocity added to the target is changing according to this curve over time (values between 0 and 1).")]
	[SerializeField] AnimationCurve velocityTweenCurve;

	[Tooltip("The impact strength of the spell is equal to the magnitude of velocity added to the target.")]
	[SerializeField] float impactStrength = 20;

	/// <inheritdoc/>
	protected override void ApplySpellEffect_OneIteration(float time) {
	}

	/// <inheritdoc/>
	protected override void FinishApplyingSpellEffect() {
	}

	/// <summary>
	/// Adds additional velocity to the target racer via their <c>CharacterMovementController</c>.
	/// </summary>
	protected override void StartApplyingSpellEffect() {
		// Add additional velocity to the target racer
		GetTargetMovementController().additionalVelocity.AddAdditionalVelocity(
			new AdditionalVelocityTweened(impactStrength * GetBaseVelocityNormalized(), effectDuration, velocityTweenCurve)
		);
	}

	/// <summary>
	/// Gets <c>CharacterMovementController</c> component of the racer who should be targeted by the spell.
	/// </summary>
	/// <returns><c>CharacterMovementController</c> component of the target racer.</returns>
	protected abstract CharacterMovementController GetTargetMovementController();

	/// <summary>
	/// Computes base velocity (normalized) which should be added to the target racer.
	/// </summary>
	/// <returns>Normalized velocity used as a base for the velocity to be added to the target racer.</returns>
	protected abstract Vector3 GetBaseVelocityNormalized();
}
