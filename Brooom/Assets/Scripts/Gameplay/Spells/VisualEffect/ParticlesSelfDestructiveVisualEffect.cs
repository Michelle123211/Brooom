using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A visual effect controlling <c>ParticleSystem</c> and destroying itself after it has finished.
/// It can either have a fixed duration (then stopping and destroying the effect is handled automatically),
/// or not (then the <c>StopPlaying()</c> method needs to be called explicitly from somewhere.
/// </summary>
public class ParticlesSelfDestructiveVisualEffect : SelfDestructiveVisualEffect {
	
	[Tooltip("Particle System which is handled as part of this visual effect.")]
	[SerializeField] ParticleSystem particles;

	[Tooltip("If true, the visual effect will be stopped after a time period corresponding to the ParticleSystem duration.")]
	[SerializeField] bool hasFixedDuration;

	float duration = 0;
	float currentTime;

	/// <inheritdoc/>
	protected override void StartPlaying_Internal() {
		if (hasFixedDuration) {
			duration = particles.main.duration;
		}
		currentTime = 0;
		particles.Play();
	}

	/// <inheritdoc/>
	protected override void StopPlayingBeforeDestruction_Internal() {
		particles.Stop();
	}

	/// <summary>
	/// <inheritdoc/>
	/// If the effect has a fixed duration and the time is out, the effect is stopped.
	/// </summary>
	/// <param name="deltaTime">Elapsed time (in seconds) from the last call.</param>
	/// <returns><c>true</c> if the visual effect is still playing, <c>false</c> otherwise.</returns>
	protected override bool UpdatePlaying_Internal(float deltaTime) {
		if (hasFixedDuration) { // The Stop method is handled automatically based on the duration
			currentTime += deltaTime;
			return currentTime < duration;
		} else // The Stop method is handled manually from somewhere else
			return true;
	}

}
