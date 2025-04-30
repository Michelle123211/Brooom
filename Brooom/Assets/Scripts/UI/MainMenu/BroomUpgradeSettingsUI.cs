using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component representing a slider for selecting current level of a particular broom upgrade in Quick Race settings.
/// </summary>
public class BroomUpgradeSettingsUI : MonoBehaviour {

	/// <summary>Name of the broom upgrade controlled by this settings.</summary>
	public string UpgradeName => assignedUpgrade.UpgradeName;
	/// <summary>Current level of the associated broom upgrade (obtained from the corresponding slider).</summary>
	public int CurrentLevel => (int)slider.value;
	/// <summary>Maximum possible level of the associated broom upgrade.</summary>
	public int MaxLevel => assignedUpgrade.MaxLevel;

	[Tooltip("A label displaying the broom upgrade's name.")]
	[SerializeField] TextMeshProUGUI nameText;
	[Tooltip("A slider used for selecting current broom upgrade level.")]
	[SerializeField] Slider slider;
	[Tooltip("A tooltip component displaying additional information about the broom upgrade.")]
	[SerializeField] SimpleTooltip tooltip;

	BroomUpgrade assignedUpgrade;

	/// <summary>
	/// Initializes the slider for selecting current value of the given broom upgrade.
	/// </summary>
	/// <param name="broomUpgrade">Broom upgrade to be associated with the slider.</param>
	public void Initialize(BroomUpgrade broomUpgrade) {
		this.assignedUpgrade = broomUpgrade;
		// Upgrade UI
		nameText.text = LocalizationManager.Instance.GetLocalizedString("BroomUpgrade" + broomUpgrade.UpgradeName);
		tooltip.text = $"BroomUpgradeTooltip{assignedUpgrade.UpgradeName}";
		slider.minValue = 0;
		slider.maxValue = broomUpgrade.MaxLevel;
		slider.value = broomUpgrade.CurrentLevel;
		GetComponentInChildren<SettingsSliderUI>().UpdateValueLabel(broomUpgrade.CurrentLevel);
	}

}
