using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellShield : MonoBehaviour {

	[Tooltip("Visual effect which is played when the shield is appearing around the racer.")]
	[SerializeField] CustomVisualEffect shieldAppearingEffect;
	[Tooltip("Visual effect which is played when the shield is disappearing at the end of its duration.")]
	[SerializeField] CustomVisualEffect shieldDisappearingEffect;

	private enum ShieldState {
		NotVisible,
		Appearing,
		Visible,
		Disappearing,
		Finished
	}
	private ShieldState currentState = ShieldState.NotVisible;

	public void Appear() {
		currentState = ShieldState.Appearing;
		shieldAppearingEffect.StartPlaying();
	}

	public void Disappear() {
		currentState = ShieldState.Disappearing;
		shieldDisappearingEffect.StartPlaying();
	}

	private void Update() {
		// Handle transitions between states
		switch (currentState) {
			case ShieldState.Appearing:
				if (!shieldAppearingEffect.IsPlaying) currentState = ShieldState.Visible;
				break;
			case ShieldState.Disappearing:
				if (!shieldDisappearingEffect.IsPlaying) currentState = ShieldState.Finished;
				break;
			case ShieldState.Finished:
				Destroy(gameObject);
				break;
			default:
				break;
		}
	}

}
