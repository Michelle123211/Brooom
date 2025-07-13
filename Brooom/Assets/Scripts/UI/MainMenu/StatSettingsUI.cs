using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component representing a slider for selecting current value of a particular stat in Quick Race settings.
/// </summary>
public class StatSettingsUI : MonoBehaviour {

	/// <summary>Current value of the associated stat (obtained from the corresponding slider).</summary>
	public int CurrentValue => (int)slider.value;

	[Tooltip("A label displaying the stat's name.")]
	[SerializeField] TextMeshProUGUI nameText;
	[Tooltip("A slider used for selecting current stat value.")]
	[SerializeField] Slider slider;
	[Tooltip("A tooltip component displaying additional information about the stat.")]
	[SerializeField] SimpleTooltip tooltip;

	/// <summary>
	/// Initializes the slider for selecting current value of the given stat.
	/// </summary>
	/// <param name="statName">Stat to be associated with the slider.</param>
	/// <param name="value">Initial value to be used.</param>
	public void Initialize(string statName, int value) {
		nameText.text = LocalizationManager.Instance.GetLocalizedString($"PlayerStat{statName}");
		tooltip.text = $"PlayerStatTooltip{statName}";
		slider.value = value;
		GetComponentInChildren<SettingsSliderUI>().UpdateValueLabel(value);
	}

}
