using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for pulling a bonus closer
public class AttractioSpellEffect : DurativeSpellEffect {

	[Tooltip("A curve describing how to tween between the start position (bonus position) and end position (racer position). Tweens between 0 and 1.")]
	[SerializeField] AnimationCurve movementTween;

	private Vector3 startPosition;
	private Vector3 endPosition;

	private Transform bonusTransform;

	protected override void ApplySpellEffect_OneIteration(float time) {
		// Update end position and move the bonus a bit closer to it (the racer)
		endPosition = castParameters.GetCastPosition();
		if (bonusTransform != null) // bonus hasn't been picked up yet
			bonusTransform.position = startPosition + movementTween.Evaluate(time) * (endPosition - startPosition);
	}

	protected override void FinishApplyingSpellEffect() {
	}

	protected override void StartApplyingSpellEffect() {
		bonusTransform = castParameters.Target.TargetObject.GetComponent<Transform>();
		// Initialize start and end position between which the bonus would be moved
		startPosition = castParameters.GetTargetPosition(); // where the bonus is
		endPosition = castParameters.GetCastPosition(); // where the racer casting the spell is
	}
}
