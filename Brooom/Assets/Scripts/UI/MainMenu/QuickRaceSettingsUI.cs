using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;



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
		// Get values from UI elements, store them into current state and save settings persistently
		SetStateFromCurrentlySelectedtValues();
		SaveQuickRaceSettings();
		LogQuickRaceStarted();
		// Load QuickRace scene
		SceneLoader.Instance.LoadScene(Scene.QuickRace);
	}

	private void LogQuickRaceStarted() {
		// Difficulty level
		int difficultyLevel = difficultyDropdown.value;
		string difficultyName;
		if (difficultyLevel < difficultyOptions.Count) difficultyName = difficultyOptions[difficultyLevel].difficultyNameLocalizationKey;
		else difficultyName = "QuickRaceDifficultyCustom";
		// Broom upgrades
		StringBuilder broomUpgrades = new StringBuilder();
		for (int i = 0; i < broomUpgradesUI.Count; i++) {
			broomUpgrades.Append($"{broomUpgradesUI[i].UpgradeName} {PlayerState.Instance.GetBroomUpgradeLevel(broomUpgradesUI[i].UpgradeName)}/{broomUpgradesUI[i].MaxLevel}");
			if (i < broomUpgradesUI.Count - 1) broomUpgrades.Append(" - ");
		}
		// Equipped spells
		StringBuilder slotsContent = new StringBuilder();
		for (int i = 0; i < PlayerState.Instance.equippedSpells.Length; i++) {
			Spell equippedSpell = PlayerState.Instance.equippedSpells[i];
			if (equippedSpell != null && !string.IsNullOrEmpty(equippedSpell.Identifier))
				slotsContent.Append(equippedSpell.SpellName);
			else slotsContent.Append("Empty");
			if (i < PlayerState.Instance.equippedSpells.Length - 1) slotsContent.Append(" - ");
		}

		Analytics.Instance.LogEvent(AnalyticsCategory.Race, $"Quick Race started with difficulty level {difficultyLevel} ({difficultyName}), stats {PlayerState.Instance.CurrentStats}, broom upgrades {broomUpgrades} and spells {slotsContent} (spells enabled is {enableSpellsToggle.isOn}).");
	}

	private void InitializeStateFromLoadedData(QuickRaceSaveData data) {
		// Broom upgrades
		if (data.broomUpgrades != null) { // broom upgrades were saved
			foreach (var upgrade in data.broomUpgrades.UpgradeLevels)
				PlayerState.Instance.SetBroomUpgradeLevel(upgrade.Key, upgrade.Value.currentLevel, upgrade.Value.maxLevel);
		}
		// Stats
		PlayerState.Instance.CurrentStats = new PlayerStats() {
			endurance = data.stats.endurance < 0 ? PlayerState.Instance.CurrentStats.endurance : data.stats.endurance, // if <0, it wasn't saved
			speed = data.stats.speed < 0 ? PlayerState.Instance.CurrentStats.speed : data.stats.speed, // if <0, it wasn't saved
			dexterity = data.stats.dexterity < 0 ? PlayerState.Instance.CurrentStats.dexterity : data.stats.dexterity, // if <0, it wasn't saved
			precision = data.stats.precision < 0 ? PlayerState.Instance.CurrentStats.precision : data.stats.precision, // if <0, it wasn't saved
			magic = data.stats.magic < 0 ? PlayerState.Instance.CurrentStats.magic : data.stats.magic // if <0, it wasn't saved
		};
		// Equipped spells
		if (data.spells != null && data.spells.EquippedSpells != null && data.spells.EquippedSpells.Length > 0) { // spells were saved
			PlayerState.Instance.equippedSpells = data.spells.EquippedSpells;
		}
	}

	private void SetStateFromCurrentlySelectedtValues() {
		int difficultyLevel = difficultyDropdown.value;
		// Stats and broom upgrades
		if (difficultyLevel < difficultyOptions.Count) { // Preset level selected
			float multiplier = difficultyOptions[difficultyLevel].difficultyValue;
			// Compute stats based on difficulty
			int statValue = Mathf.RoundToInt(multiplier * 100);
			PlayerState.Instance.CurrentStats = new PlayerStats { endurance = statValue, speed = statValue, dexterity = statValue, precision = statValue, magic = statValue };
			// Compute broom upgrades based on difficulty
			foreach (var upgrade in broomUpgradesUI) {
				PlayerState.Instance.SetBroomUpgradeLevel(upgrade.UpgradeName, Mathf.RoundToInt(multiplier * upgrade.MaxLevel), upgrade.MaxLevel);
			}
		} else { // Custom level selected
			// Use values directly from UI elements
			List<int> statsValues = new List<int>();
			foreach (var stat in statsUI) statsValues.Add(stat.CurrentValue);
			PlayerState.Instance.CurrentStats = PlayerStats.FromListOfValues(statsValues);
			foreach (var upgrade in broomUpgradesUI)
				PlayerState.Instance.SetBroomUpgradeLevel(upgrade.UpgradeName, upgrade.CurrentLevel, upgrade.MaxLevel);
		}
		// Spells
		if (!enableSpellsToggle.isOn) { // Spells disabled
			// Unequip everything and lock all spells (so opponents cannot use them either)
			foreach (var spell in PlayerState.Instance.equippedSpells) { 
				if (spell != null && !string.IsNullOrEmpty(spell.Identifier))
					PlayerState.Instance.UnequipSpell(spell.Identifier);
			}
			foreach (var spell in SpellManager.Instance.AllSpells)
				PlayerState.Instance.LockSpell(spell.Identifier);
		} else { 
			// Correct spells should be already equipped and all spells unlocked
		}
	}

	private void SaveQuickRaceSettings() {
		// Save current settings persistently
		QuickRaceSaveData data = new QuickRaceSaveData();
		// Basic settings are always saved
		data.difficultyLevel = difficultyDropdown.value;
		data.enableSpells = enableSpellsToggle.isOn ? 1 : 0;
		// Advanced settings (broom upgrades, stats, equipped spells) only if applicable
		if (data.difficultyLevel == difficultyOptions.Count) { // Custom level selected, save stats and broom upgrades
			data.stats = PlayerState.Instance.CurrentStats;
			data.broomUpgrades = new BroomUpgradesSaveData() { UpgradeLevels = PlayerState.Instance.BroomUpgradeLevels };
		}
		if (data.enableSpells == 1) { // Spells enabled, save equippedSpells
			data.spells = new SpellsSaveData {
				EquippedSpells = PlayerState.Instance.equippedSpells,
				SpellsAvailability = PlayerState.Instance.spellAvailability,
				SpellsUsage = PlayerState.Instance.spellCast
			};
		}

		SaveSystem.SaveQuickRaceData(data);
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
		string valueLocalizationKey = value < difficultyOptions.Count ? difficultyOptions[value].difficultyNameLocalizationKey : "QuickRaceDifficultyCustom";
		difficultyDropdown.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedString(valueLocalizationKey);
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
			statsUI.Add(statSettings);
		}
	}

	private void InitializeSpells() {
		// Unlock all spells
		foreach (var spell in SpellManager.Instance.AllSpells) {
			PlayerState.Instance.UnlockSpell(spell.Identifier);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="enableSpells">Could have 3 possible values: -1 if not initialized (spells are then enabled, if a spell is equipped), 0 if disabled, 1 if enabled.</param>
	private void InitializeEnableSpellsToggleValue(int enableSpells) {
		// If the value was not loaded from persistently stored state, then choose it based on equipped spells
		if (enableSpells < 0 || enableSpells > 1) { 
			enableSpells = PlayerState.Instance.HasEquippedSpells() ? 1 : 0;
		}
		enableSpellsToggle.isOn = (enableSpells == 1);
		OnSpellsEnabledChanged(enableSpellsToggle.isOn);
		// Equipped spells are initialized automatically in corresponding component
	}

	private void OnEnable() {
		// If there is no state backup, create it
		if (!SaveSystem.BackupExists()) SaveSystem.CreateBackup();

		// Create default value for difficulty level
		int difficultyLevel = -1;
		int enableSpells = -1;
		// If there exists persistently saved settings from previous race, load it
		QuickRaceSaveData loadedData = SaveSystem.LoadQuickRaceData();
		if (loadedData != null) {
			difficultyLevel = loadedData.difficultyLevel;
			enableSpells = loadedData.enableSpells;
			InitializeStateFromLoadedData(loadedData); // store it directly into game state
		} else { // Otherwise, load state from the main game so that initial values can be based on that
			PlayerState.Instance.LoadSavedState();
		}

		InitializeDifficultyDropdownOptions();
		InitializeBroomUpgrades();
		InitializeStats();
		InitializeSpells();

		InitializeDifficultyDropdownValue(difficultyLevel);
		InitializeEnableSpellsToggleValue(enableSpells); // based on whether there is a spell equipped, or not

		Analytics.Instance.LogEvent(AnalyticsCategory.Race, "Quick Race settings opened.");
	}

	private void OnDisable() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Race, "Quick Race settings closed.");
	}

}

[System.Serializable]
internal class QuickRaceDifficultyOption {
	[Tooltip("A localization key used to get localized name of this difficulty option.")]
	public string difficultyNameLocalizationKey;
	[Tooltip("A value between 0 and 1 determining percentage of the highest difficulty possible which corresponds to this difficulty level. It is then used e.g. to compute stats or broom upgrades.")]
	public float difficultyValue = 0f;
}
