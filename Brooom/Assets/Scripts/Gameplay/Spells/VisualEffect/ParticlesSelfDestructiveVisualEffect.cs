using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesSelfDestructiveVisualEffect : SelfDestructiveVisualEffect {
	
	[Tooltip("Particle System which is handleds as part of this visual effect.")]
	[SerializeField] ParticleSystem particles;

	[Tooltip("If true, the visual effect will be stopped after a certain time period.")]
	[SerializeField] bool hasFixedDuration;
	[Tooltip("If 'Has FIxed Duration' is true, the visual effect will be stopped after this amount of time in seconds.")]
	[SerializeField] float duration;

	float currentTime;

	protected override void StartPlaying_Internal() {
		currentTime = 0;
		particles.Play();
	}

	protected override void StopPlayingBeforeDestruction_Internal() {
		particles.Stop();
	}

	protected override bool UpdatePlaying_Internal(float deltaTime) {
		if (hasFixedDuration) { // The Stop method is handled automatically based on the duration
			currentTime += deltaTime;
			return currentTime < duration;
		} else // The Stop method is handled manually from somewhere else
			return true;
	}

}
