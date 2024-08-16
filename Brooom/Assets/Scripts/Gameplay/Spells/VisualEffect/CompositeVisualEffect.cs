using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This Visual Effect operates several Visual Effects all at once
public class CompositeVisualEffect : CustomVisualEffect {

	[SerializeField] List<CustomVisualEffect> visualEffects = new List<CustomVisualEffect>();

	protected override void StartPlaying_Internal() {
		foreach (var visualEffect in visualEffects)
			visualEffect.StartPlaying();
	}

	protected override void StopPlaying_Internal() {
		foreach (var visualEffect in visualEffects)
			visualEffect.StopPlaying();
	}

	protected override bool UpdatePlaying_Internal(float deltaTime) {
		// The effect keeps playing while at least one of the effects contained is playing
		foreach (var visualEffect in visualEffects) {
			if (visualEffect.IsPlaying) return true;
		}
		return false;
	}

}
