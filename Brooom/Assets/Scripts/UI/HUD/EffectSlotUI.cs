using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component displaying a single effect affecting the player in the HUD during race.
/// </summary>
public class EffectSlotUI : MonoBehaviour {

	[Tooltip("An image displaying icon of the effect.")]
	[SerializeField] Image effectIconImage;
	[Tooltip("A label which displays the time left.")]
	[SerializeField] TextMeshProUGUI effectDurationText;

	private CharacterEffect assignedEffect;

	/// <summary>
	/// Initializes the slot with the given effect, i.e. sets the icon and duration.
	/// </summary>
	/// <param name="effect">Effect to be asigned to the slot.</param>
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
