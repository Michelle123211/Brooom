using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectSlotUI : MonoBehaviour
{
	[Tooltip("An image displaying icon of the effect.")]
	[SerializeField] Image effectIconImage;
	[Tooltip("A label which displays the time left.")]
	[SerializeField] TextMeshProUGUI effectDurationText;

	private CharacterEffect assignedEffect;

	// Initialization with icon and duration
	public void Initialize(CharacterEffect effect) {
		assignedEffect = effect;
		effectIconImage.sprite = assignedEffect.Icon;
		effectDurationText.text = $"{(int)assignedEffect.TimeLeft} s";
		effect.onEffectEnd += DestroySelf; // register callback - destroy on timeout
	}

	private void Update() {
		// Update the time label
		if (assignedEffect.TimeLeft >= 0) {
			effectDurationText.text = $"{(int)assignedEffect.TimeLeft} s";
		}
	}

	private void DestroySelf() {
		Destroy(gameObject);
	}
}
