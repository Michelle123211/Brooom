using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A visual effect which controls several visual effects all at once (i.e. they are all sterted and stopped together).
/// The effect keeps playing while at least one of the associated effects is playing (they can have different durations).
/// </summary>
public class CompositeVisualEffect : CustomVisualEffect {

	[Tooltip("A list of visual effects to be controlled together.")]
	[SerializeField] List<CustomVisualEffect> visualEffects = new List<CustomVisualEffect>();

	/// <summary>
	/// Starts playing all associated visual effects.
	/// </summary>
	protected override void StartPlaying_Internal() {
		foreach (var visualEffect in visualEffects)
			visualEffect.StartPlaying();
	}

	/// <summary>
	/// Stops playing all associated visual effects.
	/// </summary>
	protected override void StopPlaying_Internal() {
		foreach (var visualEffect in visualEffects)
			visualEffect.StopPlaying();
	}

	/// <summary>
	/// Updates all associated visual effects to progress further. It is called from <c>Update()</c> method.
	/// </summary>
	/// <param name="deltaTime">Elapsed time (in seconds) from the last call.</param>
	/// <returns><c>true</c> if the visual effect is still playing, <c>false</c> otherwise.</returns>
	protected override bool UpdatePlaying_Internal(float deltaTime) {
		// The effect keeps playing while at least one of the effects contained is playing
		foreach (var visualEffect in visualEffects) {
			if (visualEffect.IsPlaying) return true;
		}
		return false;
	}

}
