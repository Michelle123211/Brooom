using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



// Represents panel with settings for quick race
// Handles any UI interactions inside of this panel and sets current state when the race is started
public class QuickRaceSettingsUI : MonoBehaviour {

	[Tooltip("These will be all possible preset difficulty levels. They will affect stats and broom upgrades.")]
	[SerializeField] List<QuickRaceDifficultyOption> difficultyOptions;

	[Tooltip("Broom object in the scene. All available broom upgrades can be obtained from it.")]
	[SerializeField] Broom broom;

	[Header("UI elements")]
	[SerializeField] TMP_Dropdown difficultyDropdown;
	[SerializeField] Toggle enableSpellsToggle;
	[Tooltip("A parent object of everything related to equipped spells. Is set to inactive when spells are disabled.")]
	[SerializeField] EquippedSpellsUI equippedSpellsSection;

	[Tooltip("A parent object of everything related to broom upgrades. Is set to inactive, unless current difficulty level is Custom.")]
	[SerializeField] GameObject broomUpgradesSection;
	[Tooltip("A parent object of individual broom upgrades which are added by instantiating a prefab.")]
	[SerializeField] Transform broomUpgradesParent;
	[Tooltip("A prefab of a single broom upgrade settings.")]
	[SerializeField] BroomUpgradeSettingsUI broomUpgradePrefab;

	[Tooltip("A parent object of everything related to stats. Is set to inactive, unless current difficulty level is Custom.")]
	[SerializeField] GameObject statsSection;
	[Tooltip("A parent object of individual stats which are added by instantiating a prefab.")]
	[SerializeField] Transform statsParent;
	[Tooltip("A prefab of a single stat settings.")]
	[SerializeField] StatSettingsUI statPrefab;

	private List<BroomUpgradeSettingsUI> broomUpgradesUI = new List<BroomUpgradeSettingsUI>();
	private List<StatSettingsUI> statsUI = new List<StatSettingsUI>();


	public void OnDifficultyChanged(int optionValue) {
		// If it is Custom, show all advanced options (broom upgrades, stats), otherwise hide them
		bool isCustomDifficulty = !(optionValue < difficultyOptions.Count);
		broomUpgradesSection.SetActive(isCustomDifficulty);
		statsSection.SetActive(isCustomDifficulty);
	}

	public void OnSpellsEnabledChanged(bool spellsEnabled) {
		// If spells are enabled, show equipped spell slots, otherwise hide them
		equippedSpellsSection.gameObject.SetActive(spellsEnabled);
	}

	public void OnRaceStarted() {
		// Get values from UI elements and store them into current state
		// - Stats based on chosen difficulty level, or based on sliders (if Custom)
		// - Broom upgrades based on chosen difficulty level, or based on sliders (if Custom)
		// - Spells
		//		- if spells are not enabled, unequip everything and lock all spells (so opponents cannot use them either)
		//		- if spells are enabled, do nothing (correct spells should be already equipped and all spells should be unlocked)
		// Load QuickRace scene
	}

	private void InitializeDifficultyDropdownOptions() {
		// All difficulty options + Custom
		difficultyDropdown.ClearOptions();
		foreach (var difficultyOption in difficultyOptions)
			difficultyDropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Instance.GetLocalizedString(difficultyOption.difficultyNameLocalizationKey)));
		difficultyDropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Instance.GetLocalizedString("QuickRaceDifficultyCustom")));
	}

	private void InitializeDifficultyDropdownValue(int value) {
		// If the value was not loaded from persistently stored state, then choose it based on current stats values (compute average and then closest value is the chosen option)
		if (value < 0) {
			float statsAverage = PlayerState.Instance.CurrentStats.GetWeightedAverage();
			float minDifference = float.MaxValue;
			for (int i = 0; i < difficultyOptions.Count; i++) {
				float difference = Mathf.Abs(statsAverage - (difficultyOptions[i].difficultyValue * 100));
				if (difference < minDifference) {
					minDifference = difference;
					value = i;
				}
			}
		}
		// Set it and adjust other UI
		difficultyDropdown.value = value;
		OnDifficultyChanged(value);
		// Sometimes the label is not initialized correctly (it is empty), so set it manually
		difficultyDropdown.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedString(difficultyOptions[value].difficultyNameLocalizationKey);
	}

	private void InitializeBroomUpgrades() {
		// Instantiate a prefab representing a broom upgrade and initialize it (upgrade name, min and max value, current value)
		UtilsMonoBehaviour.RemoveAllChildren(broomUpgradesParent);
		broomUpgradesUI.Clear();
		BroomUpgrade[] upgrades = broom.GetAvailableUpgrades();
		foreach (var upgrade in upgrades) {
			BroomUpgradeSettingsUI upgradeSettings = Instantiate<BroomUpgradeSettingsUI>(broomUpgradePrefab, broomUpgradesParent);
			upgradeSettings.Initialize(upgrade);
			broomUpgradesUI.Add(upgradeSettings);
		}
	}

	private void InitializeStats() {
		// Instantiate a prefab representing a stat and initialize it (stat name, current value)
		UtilsMonoBehaviour.RemoveAllChildren(statsParent);
		statsUI.Clear();
		List<float> statsValues = PlayerState.Instance.CurrentStats.GetListOfValues();
		List<string> statsNames = PlayerStats.GetListOfStatNames();
		for (int i = 0; i < statsValues.Count; i++) {
			StatSettingsUI statSettings = Instantiate<StatSettingsUI>(statPrefab, statsParent);
			statSettings.Initialize(statsNames[i], (int)statsValues[i]);
		}
	}

	private void InitializeSpells() {
		// Unlock all spells
		foreach (var spell in SpellManager.Instance.AllSpells) {
			PlayerState.Instance.UnlockSpell(spell.Identifier);
		}
	}

	private void InitializeEnableSpellsToggleValue() {
		// Based on equipped spells - enabled if there is a spell equipped, disabled otherwise
		enableSpellsToggle.isOn = PlayerState.Instance.HasEquippedSpells();
		OnSpellsEnabledChanged(enableSpellsToggle.isOn);
		// Equipped spells are initialized automatically in corresponding component
	}

	private void OnEnable() {
		// If there is no state backup, create it
		if (!SaveSystem.BackupExists()) SaveSystem.CreateBackup();

		// Create default value for difficulty level
		int difficultyLevel = -1;
		// If there exists persistently saved settings from previous race, load it
		// --- Store it directly into player state
		// --- Or overwrite difficultyLevel

		InitializeDifficultyDropdownOptions();
		InitializeBroomUpgrades();
		InitializeStats();
		InitializeSpells();

		InitializeDifficultyDropdownValue(difficultyLevel);
		InitializeEnableSpellsToggleValue(); // based on whether there is a spell equipped, or not
	}

}

[System.Serializable]
internal class QuickRaceDifficultyOption {
	[Tooltip("A localization key used to get localized name of this difficulty option.")]
	public string difficultyNameLocalizationKey;
	[Tooltip("A value between 0 and 1 determining percentage of the highest difficulty possible which corresponds to this difficulty level. It is then used e.g. to compute stats or broom upgrades.")]
	public float difficultyValue = 0f;
}
