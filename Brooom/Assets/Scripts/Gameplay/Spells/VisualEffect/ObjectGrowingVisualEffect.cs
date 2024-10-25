using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrowingVisualEffect : DurativeVisualEffect {

	[Tooltip("An object whose scale will be changed.")]
	[SerializeField] Transform objectToGrow;
	[Tooltip("Initial scale of the object.")]
	[SerializeField] Vector3 initialScale;
	[Tooltip("Target scale of the object which should be reached.")]
	[SerializeField] Vector3 targetScale;
	[Tooltip("A curve determining how to tween between the initial scale and the target scale over the duration. All values should be normalized between 0 and 1.")]
	[SerializeField] AnimationCurve tweeningCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[Tooltip("Whether the object, whose scale is tweened, should be activated at the start.")]
	[SerializeField] bool activateAtStart = true;
	[Tooltip("Whether the object, whose scale is tweened, should be deactivated at the end.")]
	[SerializeField] bool deactivateAtEnd = true;

	protected override void StartPlaying_AfterDurativeInit() {
		objectToGrow.localScale = initialScale;
		if (activateAtStart)
			objectToGrow.gameObject.SetActive(true);
	}

	protected override void StopPlaying_AfterDurativeFinish() {
		if (deactivateAtEnd)
			objectToGrow.gameObject.SetActive(false);
	}

	protected override void UpdatePlaying_WithNormalizedTime(float currentTimeNormalized) {
		// Set scale
		objectToGrow.localScale = initialScale + tweeningCurve.Evaluate(currentTimeNormalized) * (targetScale - initialScale);
	}
}
