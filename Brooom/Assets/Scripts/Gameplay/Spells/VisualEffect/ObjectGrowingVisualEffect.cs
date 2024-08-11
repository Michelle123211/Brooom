using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrowingVisualEffect : CustomVisualEffect {

	[Tooltip("An object whose scale will be changed.")]
	[SerializeField] Transform objectToGrow;
	[Tooltip("Initial scale of the object.")]
	[SerializeField] Vector3 initialScale;
	[Tooltip("Target scale of the object which should be reached.")]
	[SerializeField] Vector3 targetScale;
	[Tooltip("A curve determining how to tween between the initial scale and the target scale over the duration. All values should be normalized between 0 and 1.")]
	[SerializeField] AnimationCurve tweeningCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[Tooltip("How long it takes to reach the target scale.")]
	[SerializeField] float duration;

	float currentTimeNormalized;

	protected override void StartPlaying_Internal() {
		currentTimeNormalized = 0;
		objectToGrow.localScale = initialScale;
		objectToGrow.gameObject.SetActive(true);
	}

	protected override void StopPlaying_Internal() {
		objectToGrow.gameObject.SetActive(false);
	}

	protected override bool UpdatePlaying_Internal(float deltaTime) {
		// Update time
		currentTimeNormalized += (deltaTime / duration);
		bool shouldStop = currentTimeNormalized >= 1f;
		currentTimeNormalized = Mathf.Clamp(currentTimeNormalized, 0f, 1f);
		// Update effect
		objectToGrow.localScale = initialScale + tweeningCurve.Evaluate(currentTimeNormalized) * (targetScale - initialScale); // set scale
		// TODO: pass normalized time to shader to affect it if needed
		return !shouldStop;
	}
}
