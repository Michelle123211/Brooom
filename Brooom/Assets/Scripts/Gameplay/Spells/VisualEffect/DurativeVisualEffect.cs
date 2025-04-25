using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A visual effect related to casting a spell which has a fixed duration.
/// </summary>
public abstract class DurativeVisualEffect : CustomVisualEffect {

	[Tooltip("How long it takes to reach the end of the effect.")]
	[SerializeField] float duration;

	float currentTimeNormalized;

	/// <inheritdoc/>
	protected override void StartPlaying_Internal() {
		currentTimeNormalized = 0;
		StartPlaying_AfterDurativeInit();
	}

	/// <inheritdoc/>
	protected override void StopPlaying_Internal() {
		StopPlaying_AfterDurativeFinish();
	}

	/// <inheritdoc/>
	protected override bool UpdatePlaying_Internal(float deltaTime) {
		// Update time
		currentTimeNormalized += (deltaTime / duration);
		bool shouldStop = currentTimeNormalized >= 1f;
		currentTimeNormalized = Mathf.Clamp(currentTimeNormalized, 0f, 1f);
		// Update effect
		UpdatePlaying_WithNormalizedTime(currentTimeNormalized);
		return !shouldStop;
	}

	/// <summary>
	/// Initializes the visual effect to its initial values.
	/// </summary>
	protected abstract void StartPlaying_AfterDurativeInit();
	/// <summary>
	/// Finalized the visual effect to its final values.
	/// </summary>
	protected abstract void StopPlaying_AfterDurativeFinish();
	/// <summary>
	/// Updates the visual effect to progress further. It is called from <c>Update()</c> method.
	/// </summary>
	/// <param name="currentTimeNormalized">Number between 0 and 1 indicating how far we are in the effect's duration.</param>
	protected abstract void UpdatePlaying_WithNormalizedTime(float currentTimeNormalized);

}
