using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatSettingsUI : MonoBehaviour {

	public int CurrentValue => (int)slider.value;

	[Tooltip("A label displaying the stat's name.")]
	[SerializeField] TextMeshProUGUI nameText;
	[Tooltip("A slider used for selecting current stat value.")]
	[SerializeField] Slider slider;
	[Tooltip("The tooltip component displaying additional information about the stat.")]
	[SerializeField] SimpleTooltip tooltip;

	public void Initialize(string statName, int value) {
		nameText.text = LocalizationManager.Instance.GetLocalizedString($"PlayerStat{statName}");
		slider.value = value;
		GetComponentInChildren<SettingsSliderUI>().UpdateValueLabel(value);
		tooltip.text = $"PlayerStatTooltip{statName}";
	}

}
