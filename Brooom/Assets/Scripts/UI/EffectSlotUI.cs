using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectSlotUI : MonoBehaviour
{
	[Tooltip("An image displaying icon of the effect.")]
	[SerializeField] Image effectImage;
	[Tooltip("A label which displays the time left.")]
	[SerializeField] TextMeshProUGUI effectDurationText;

	private PlayerEffect assignedEffect;

	// Initialization with icon and duration
	public void Initialize(PlayerEffect effect) {
		assignedEffect = effect;
		effectImage.sprite = assignedEffect.Icon;
		effectDurationText.text = $"{(int)assignedEffect.TimeLeft} s";
	}

	private void Update() {
		// Destroy on timeout
		if (assignedEffect.TimeLeft < 0) {
			Destroy(gameObject);
		}
		// Update the time
		else { 
			effectDurationText.text = $"{(int)assignedEffect.TimeLeft} s";
		}
	}
}
