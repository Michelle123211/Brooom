using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A visual effect for tweening an object's scale over a duration of time.
/// It can also activate the target object at the start and deactivate it at the end.
/// </summary>
public class ObjectGrowingVisualEffect : DurativeVisualEffect {

	[Tooltip("An object whose scale will be changed.")]
	[SerializeField] Transform objectToGrow;
	[Tooltip("Initial scale of the object.")]
	[SerializeField] Vector3 initialScale;
	[Tooltip("Target scale of the object which should be reached.")]
	[SerializeField] Vector3 targetScale;
	[Tooltip("A curve determining how to tween between the initial scale and the target scale over the duration. All values should be normalized between 0 (i.e. initial scale) and 1 (i.e. target scale).")]
	[SerializeField] AnimationCurve tweeningCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[Tooltip("Whether the object, whose scale is tweened, should be activated at the start.")]
	[SerializeField] bool activateAtStart = true;
	[Tooltip("Whether the object, whose scale is tweened, should be deactivated at the end.")]
	[SerializeField] bool deactivateAtEnd = true;

	/// <inheritdoc/>
	protected override void StartPlaying_AfterDurativeInit() {
		objectToGrow.localScale = initialScale;
		if (activateAtStart)
			objectToGrow.gameObject.SetActive(true);
	}

	/// <inheritdoc/>
	protected override void StopPlaying_AfterDurativeFinish() {
		if (deactivateAtEnd)
			objectToGrow.gameObject.SetActive(false);
	}

	/// <inheritdoc/>
	protected override void UpdatePlaying_WithNormalizedTime(float currentTimeNormalized) {
		// Set scale
		objectToGrow.localScale = initialScale + tweeningCurve.Evaluate(currentTimeNormalized) * (targetScale - initialScale);
	}
}
