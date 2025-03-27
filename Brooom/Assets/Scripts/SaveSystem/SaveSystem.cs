using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class SaveSystem
{
    // Names of the save files
    private static readonly string characterFileName = "character_customization";
    private static readonly string settingsFileName = "settings";
    private static readonly string rebindingFileName = "key_bindings";
    private static readonly string playerStateFileName = "player_state";
    private static readonly string broomUpgradesFileName = "broom_upgrades";
    private static readonly string spellsFileName = "spells";
    private static readonly string regionsFileName = "regions";
    private static readonly string achievementsFileName = "achievements";
    private static readonly string tutorialFileName = "tutorial";

    // Other parts of the path
    private static readonly string fileExtension = ".json";
    private static readonly string saveFolder = Application.persistentDataPath + "/Saves/";

    // Full paths
    private static readonly string characterPath = saveFolder + characterFileName + fileExtension;
    private static readonly string settingsPath = saveFolder + settingsFileName + fileExtension;
    private static readonly string rebindingPath = saveFolder + rebindingFileName + fileExtension;
    private static readonly string playerStatePath = saveFolder + playerStateFileName + fileExtension;
    private static readonly string broomUpgradesPath = saveFolder + broomUpgradesFileName + fileExtension;
    private static readonly string spellsPath = saveFolder + spellsFileName + fileExtension;
    private static readonly string regionsPath = saveFolder + regionsFileName + fileExtension;
    private static readonly string achievementsPartialPath = saveFolder + achievementsFileName;
    private static readonly string tutorialPath = saveFolder + tutorialFileName + fileExtension;


    static SaveSystem() {
        // Make sure the save folder exists
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
    }

	#region Character customization
	public static void SaveCharacterCustomization(CharacterCustomizationSaveData characterData) {
        string json = JsonUtility.ToJson(characterData);
        File.WriteAllText(characterPath, json);
    }

    public static CharacterCustomizationSaveData LoadCharacterCustomization() {
        if (File.Exists(characterPath)) { // If there is a save file, load the data from there
            string json = File.ReadAllText(characterPath);
            CharacterCustomizationSaveData characterData = JsonUtility.FromJson<CharacterCustomizationSaveData>(json);
            return characterData;
        } else { // Otherwise return null
            return null;
        }
    }
	#endregion

	#region Settings
	public static void SaveCurrentLanguage(string language) {
        SettingsSaveData settingsData;
        if (File.Exists(settingsPath)) { // If there is a save file, load the data from there
            string jsonData = File.ReadAllText(settingsPath);
            settingsData = JsonUtility.FromJson<SettingsSaveData>(jsonData);
        } else { // Otherwise use a new instance with default values
            settingsData = new SettingsSaveData();
        }
        // Override only the language field and save it
        settingsData.currentLanguage = language;
        string json = JsonUtility.ToJson(settingsData);
        File.WriteAllText(settingsPath, json);
    }

    public static string LoadCurrentLanguage() {
        if (File.Exists(settingsPath)) { // If there is a save file, load the data from there
            string json = File.ReadAllText(settingsPath);
            SettingsSaveData settingsData = JsonUtility.FromJson<SettingsSaveData>(json);
            return settingsData.currentLanguage;
        } else { // Otherwise return null
            return null;
        }
    }

    public static void SaveSettings(SettingsSaveData settings) {
        // Load currently saved settings
        SettingsSaveData settingsData;
        if (File.Exists(settingsPath)) {// If there are any, copy the current language
            string jsonData = File.ReadAllText(settingsPath);
            settingsData = JsonUtility.FromJson<SettingsSaveData>(jsonData);
            settings.currentLanguage = settingsData.currentLanguage;
        }
        // Save the settings into a file
        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(settingsPath, json);
    }

    public static SettingsSaveData LoadSettings() {
        if (File.Exists(settingsPath)) { // If there is a save file, load the data from there
            string json = File.ReadAllText(settingsPath);
            SettingsSaveData settingsData = JsonUtility.FromJson<SettingsSaveData>(json);
            return settingsData;
        } else { // Otherwise return null
            return null;
        }
    }

	public static void SaveKeyBindings(string bindingsAsJson) {
        File.WriteAllText(rebindingPath, bindingsAsJson);
    }

    public static string LoadKeyBindings() {
        if (File.Exists(rebindingPath)) { // If there is a save file, load the data from there
            return File.ReadAllText(rebindingPath);
        } else { // Otherwise return null
            return null;
        }
    }
    #endregion

    #region Player state
    public static void SavePlayerState(PlayerStateSaveData playerState) {
        // Save the whole state
        string json = JsonUtility.ToJson(playerState);
        File.WriteAllText(playerStatePath, json);
    }

    public static PlayerStateSaveData LoadPlayerState() {
        // Load the whole state
        if (File.Exists(playerStatePath)) { // If there is a save file, load the data from there
            string json = File.ReadAllText(playerStatePath);
            PlayerStateSaveData playerState = JsonUtility.FromJson<PlayerStateSaveData>(json);
            return playerState;
        } else { // Otherwise return null
            return null;
        }
    }

	public static void SavePlayerStatistics(PlayerStats previousStats, PlayerStats currentStats) {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) playerState = new PlayerStateSaveData(); // if there is no save file, use default values
        // Override only the statistics
        playerState.stats.previousStats = previousStats;
        playerState.stats.currentStats = currentStats;
        // Save it back
        SavePlayerState(playerState);
    }

    public static StatisticsSaveData LoadPlayerStatistics() {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        // If there is no saved state, return null
        if (playerState == null) return null;
        // Return only the statistics
        return playerState.stats;
    }

    public static void SaveKnownOpponents(Dictionary<int, string> knownOpponents) {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) playerState = new PlayerStateSaveData(); // if there is no save file, use default values
        // Override only the opponents
        playerState.KnownOpponents = knownOpponents;
        // Save it back
        SavePlayerState(playerState);
    }

    public static Dictionary<int, string> LoadKnownOpponents() {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        // If there is no saved state, return null
        if (playerState == null) return null;
        // Return only the opponents
        return playerState.KnownOpponents;
    }

    public static void SaveGameComplete(bool gameComplete) {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) playerState = new PlayerStateSaveData(); // if there is no save file, use default values
        // Override only the bool
        playerState.gameComplete = gameComplete;
        // Save it back
        SavePlayerState(playerState);
    }

    public static bool LoadGameComplete() {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        // If there is no saved state, return false
        if (playerState == null) return false;
        // Return only the bool
        return playerState.gameComplete;
    }

    public static void SaveCoins(int coins) {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) playerState = new PlayerStateSaveData(); // if there is no save file, use default values
        // Override only the coins
        playerState.coins = coins;
        // Save it back
        SavePlayerState(playerState);
    }

    public static int LoadCoins() {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        // If there is no saved state, return 0
        if (playerState == null) return 0;
        // Return only the coins
        return playerState.coins;
    }
    #endregion

    #region Broom upgrades
    public static void SaveBroomUpgrades(BroomUpgradesSaveData broomUpgrades) {
        string json = JsonUtility.ToJson(broomUpgrades);
        File.WriteAllText(broomUpgradesPath, json);
    }

    public static BroomUpgradesSaveData LoadBroomUpgrades() {
        if (File.Exists(broomUpgradesPath)) { // If there is a save file, load the data from there
            // Load broom upgrades from file
            string json = File.ReadAllText(broomUpgradesPath);
            BroomUpgradesSaveData upgrades = JsonUtility.FromJson<BroomUpgradesSaveData>(json);
            return upgrades;
        } else { // Otherwise return null
            return null;
        }
    }
    #endregion

    #region Spells
    public static void SaveSpells(SpellsSaveData spellsData) {
        // Save everything
        string json = JsonUtility.ToJson(spellsData);
        File.WriteAllText(spellsPath, json);
    }

    public static SpellsSaveData LoadSpells() {
        // Load everything
        if (File.Exists(spellsPath)) { // If there is a save file, load the data from there
            string json = File.ReadAllText(spellsPath);
            SpellsSaveData spellsData = JsonUtility.FromJson<SpellsSaveData>(json);
            return spellsData;
        } else { // Otherwise return null
            return null;
        }
    }

    public static void SavePurchasedSpells(Dictionary<string, bool> spellsAvailability) {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        if (spellsData == null) spellsData = new SpellsSaveData(); // if there is no save file, use default values
        // Override only the purchased spells
        spellsData.SpellsAvailability = spellsAvailability;
        // Save it back
        SaveSpells(spellsData);
    }

    public static Dictionary<string, bool> LoadPurchasedSpells() {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        // If there is no saved state, return null
        if (spellsData == null) return null;
        // Return only the purchased spells
        return spellsData.SpellsAvailability;
    }

    public static void SaveEquippedSpells(Spell[] equippedSpellsData) {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        if (spellsData == null) spellsData = new SpellsSaveData(); // if there is no save file, use default values
        // Override only the equipped spells
        spellsData.EquippedSpells = equippedSpellsData;
        // Save it back
        SaveSpells(spellsData);
    }

    public static Spell[] LoadEquippedSpells() {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        // If there is no saved state, return null
        if (spellsData == null) return null;
        // Return only the equipped spells
        return spellsData.EquippedSpells;
    }

    public static void SaveCastSpells(Dictionary<string, bool> spellsUsage) {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        if (spellsData == null) spellsData = new SpellsSaveData(); // if there is no save file, use default values
        // Override only the cast spells
        spellsData.SpellsUsage = spellsUsage;
        // Save it back
        SaveSpells(spellsData);
    }

    public static Dictionary<string, bool> LoadCastSpells() {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        // If there is no saved state, return null
        if (spellsData == null) return null;
        // Return only the cast spells
        return spellsData.SpellsUsage;
    }
	#endregion

	#region Visited regions
	public static void SaveRegions(RegionsSaveData regionsData) {
		// Save everything
		string json = JsonUtility.ToJson(regionsData);
		File.WriteAllText(regionsPath, json);
	}

	public static RegionsSaveData LoadRegions() {
		// Load everything
		if (File.Exists(regionsPath)) { // If there is a save file, load the data from there
			string json = File.ReadAllText(regionsPath);
			RegionsSaveData regionsData = JsonUtility.FromJson<RegionsSaveData>(json);
			return regionsData;
		} else { // Otherwise return null
			return null;
		}
	}

	public static void SaveVisitedRegions(Dictionary<LevelRegionType, bool> visitedRegions) {
		// Load everything
		RegionsSaveData regionsData = LoadRegions();
		if (regionsData == null) regionsData = new RegionsSaveData(); // if there is no save file, use default values
																	  // Override only the visited regions
		regionsData.RegionsVisited = visitedRegions;
		// Save it back
		SaveRegions(regionsData);
	}

    public static Dictionary<LevelRegionType, bool> LoadVisitedRegions() {
		// Load everything
		RegionsSaveData regionsData = LoadRegions();
		// If there is no saved state, return null
		if (regionsData == null) return null;
        // Return only the visited regions
        return regionsData.RegionsVisited;
	}
	#endregion

	#region Achievements
	public static void SaveAchievementData<T>(T data, string dataIdentifier) {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText($"{achievementsPartialPath}_{dataIdentifier}{fileExtension}", json);
    }

    public static T LoadAchievementData<T>(string dataIdentifier) {
        string path = $"{achievementsPartialPath}_{dataIdentifier}{fileExtension}";
        if (File.Exists(path)) {  // If there is a save file, load the data from there
            // Load values from a file
            string json = File.ReadAllText(path);
            T result = JsonUtility.FromJson<T>(json);
            return result;
        } else {  // Otherwise return default
            return default;
        }
    }
    #endregion

    #region Tutorial
    public static void SaveTutorialData(TutorialSaveData data) {
        // Save the whole data
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(tutorialPath, json);
    }
    public static TutorialSaveData LoadTutorialData() {
        // Load the whole data
        if (File.Exists(tutorialPath)) { // If there is a save file, load the data from there
            string json = File.ReadAllText(tutorialPath);
            TutorialSaveData tutorialState = JsonUtility.FromJson<TutorialSaveData>(json);
            return tutorialState;
        } else { // Otherwise return null
            return null;
        }
    }
	#endregion
}
