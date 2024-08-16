using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DurativeVisualEffect : CustomVisualEffect {

	[Tooltip("How long it takes to reach the end of the effect.")]
	[SerializeField] float duration;

	float currentTimeNormalized;

	protected override void StartPlaying_Internal() {
		currentTimeNormalized = 0;
		StartPlaying_AfterDurativeInit();
	}

	protected override void StopPlaying_Internal() {
		StopPlaying_AfterDurativeFinish();
	}

	protected override bool UpdatePlaying_Internal(float deltaTime) {
		// Update time
		currentTimeNormalized += (deltaTime / duration);
		bool shouldStop = currentTimeNormalized >= 1f;
		currentTimeNormalized = Mathf.Clamp(currentTimeNormalized, 0f, 1f);
		// Update effect
		UpdatePlaying_WithNormalizedTime(currentTimeNormalized);
		return !shouldStop;
	}

	protected abstract void StartPlaying_AfterDurativeInit();
	protected abstract void StopPlaying_AfterDurativeFinish();
	protected abstract void UpdatePlaying_WithNormalizedTime(float currentTimeNormalized);

}
