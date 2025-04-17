using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BroomUpgradeSettingsUI : MonoBehaviour {

	[Tooltip("A label displaying the broom upgrade's name.")]
	[SerializeField] TextMeshProUGUI nameText;
	[Tooltip("A slider used for selecting current broom upgrade level.")]
	[SerializeField] Slider slider;
	[Tooltip("The tooltip component displaying additional information about the broom upgrade.")]
	[SerializeField] SimpleTooltip tooltip;

	private BroomUpgrade assignedUpgrade;

	public void Initialize(BroomUpgrade broomUpgrade) {
		this.assignedUpgrade = broomUpgrade;
		// Upgrade UI
		nameText.text = LocalizationManager.Instance.GetLocalizedString("BroomUpgrade" + broomUpgrade.UpgradeName);
		tooltip.text = $"BroomUpgradeTooltip{assignedUpgrade.UpgradeName}";
		slider.minValue = 0;
		slider.maxValue = broomUpgrade.MaxLevel;
		slider.value = broomUpgrade.CurrentLevel;
	}

}
