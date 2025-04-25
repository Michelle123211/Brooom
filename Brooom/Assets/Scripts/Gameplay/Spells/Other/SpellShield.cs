using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component handling behaviour of a shield around a racer created by Defensio spell.
/// It uses visual effects for appearing and disappearing, and at the end of its duration it is destroyed.
/// </summary>
public class SpellShield : MonoBehaviour {

	[Tooltip("Visual effect which is played when the shield is appearing around the racer.")]
	[SerializeField] CustomVisualEffect shieldAppearingEffect;
	[Tooltip("Visual effect which is played when the shield is disappearing at the end of its duration.")]
	[SerializeField] CustomVisualEffect shieldDisappearingEffect;

	private enum ShieldState { // the shield is going through several states during its lifetime
		NotVisible,
		Appearing,
		Visible,
		Disappearing,
		Finished
	}
	private ShieldState currentState = ShieldState.NotVisible;

	/// <summary>
	/// Starts playing a visual effect which makes the shield appear around the racer.
	/// </summary>
	public void Appear() {
		currentState = ShieldState.Appearing;
		shieldAppearingEffect.StartPlaying();
	}

	/// <summary>
	/// Starts playing a visual effect which makes the shield disappear at the end of its duration.
	/// </summary>
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
