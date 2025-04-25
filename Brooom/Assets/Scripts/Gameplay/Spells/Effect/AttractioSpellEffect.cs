using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell pulling target bonus to the racer who cast it.
/// </summary>
public class AttractioSpellEffect : DurativeSpellEffect {

	[Tooltip("A curve describing how to tween between the start position (bonus position) and end position (racer position). Tweens between 0 and 1.")]
	[SerializeField] AnimationCurve movementTween;

	private Vector3 startPosition;
	private Vector3 endPosition;

	private Transform bonusTransform;

	/// <summary>
	/// Moves the target bonus a bit closer to the racer casting the spell.
	/// </summary>
	/// <param name="time">Number between 0 and 1 (indicating how far we are in the total duration).</param>
	protected override void ApplySpellEffect_OneIteration(float time) {
		// Update end position and move the bonus a bit closer to it (the racer)
		endPosition = castParameters.GetCastPosition();
		if (bonusTransform != null) // bonus hasn't been picked up yet
			bonusTransform.position = startPosition + movementTween.Evaluate(time) * (endPosition - startPosition);
	}

	/// <inheritdoc/>
	protected override void FinishApplyingSpellEffect() {
	}

	/// <inheritdoc/>
	protected override void StartApplyingSpellEffect() {
		// Make sure the target object is still valid - it could be a bonus spawned by Temere Commodum which was picked up while the spell was flying towards it
		// ... if not, there's nothing to be done anymore
		if (castParameters.Target.TargetObject == null) return;

		bonusTransform = castParameters.Target.TargetObject.GetComponent<Transform>();
		// Initialize start and end position between which the bonus would be moved
		startPosition = castParameters.GetTargetPosition(); // where the bonus is
		endPosition = castParameters.GetCastPosition(); // where the racer casting the spell is
	}
}
