using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;

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
    private static readonly string quickRaceFileName = "quick_race";

    // Other parts of the path
    private static readonly string fileExtension = ".json";
    private static readonly string saveFolder = Application.persistentDataPath + "/Saves/";
    private static readonly string backupFolder = saveFolder + "Backup/";

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
    private static readonly string quickRacePath = saveFolder + quickRaceFileName + fileExtension;


    static SaveSystem() {
        // Make sure the save folder exists
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
    }

    public static bool SaveExists() {
        // Saves folder exists and there is file for character customization (implying a game has been started previously)
        if (!Directory.Exists(saveFolder)) return false;
        return File.Exists(characterPath);
    }

    /// <summary>
    /// Check whether a backup of current game state exists, which is indicated by existence of 'Saves\Backup\' folder.
    /// </summary>
    /// <returns>True if 'Saves\Backup\' folder exists, false otherwise.</returns>
    public static bool BackupExists() {
        // Saves folder exists and there is a Backup subfolder
        if (!Directory.Exists(saveFolder)) return false;
        return Directory.Exists(backupFolder);
    }

    /// <summary>
    /// Copies all files from 'Saves\' folder to 'Saves\Backup\' subfolder, except for 'settings.json', 'key_bindings.json' and 'quick_race.json' (they are global and can be not only in-game).
    /// Also deletes any backup from before.
    /// </summary>
    public static void CreateBackup() {
        // If there is backup already, delete it
        DeleteBackup();
        // Copy all files to a subfolder (except settings and key rebinding, because that is not specific for a game and can be changed outside of it)
        Directory.CreateDirectory(backupFolder);
        foreach (var file in Directory.EnumerateFiles(saveFolder)) {
            string fileName = file.Substring(saveFolder.Length);
            if (fileName.Contains(settingsFileName) || fileName.Contains(rebindingFileName) || fileName.Contains(quickRaceFileName)) continue;
            File.Copy(file, backupFolder + fileName, true);
        }
    }

    /// <summary>
    /// Copies all files from 'Saves\Backup\' subfolder to 'Saves\' folder while overwriting any already existing files of the same name.
    /// Then deletes the 'Saves\Backup\' subfolder completely.
    /// </summary>
    public static void RestoreFromBackup() {
        // Copy all files from backup subfolder to the Saves folder (with overwrite enabled) and then delete the subfolder
        if (!Directory.Exists(backupFolder)) return;
        foreach (var file in Directory.EnumerateFiles(backupFolder)) { 
            string fileName = file.Substring(backupFolder.Length);
            File.Copy(file, saveFolder + fileName, true);
        }
        DeleteBackup();
    }

    /// <summary>
    /// Completely deletes 'Saves\Backup\' subfolder.
    /// </summary>
    public static void DeleteBackup() { 
        if (Directory.Exists(backupFolder))
            Directory.Delete(backupFolder, true);
    }

	#region Character customization
	public static void SaveCharacterCustomization(CharacterCustomizationSaveData characterData) {
        SaveData<CharacterCustomizationSaveData>(characterData, characterPath);
    }

    public static CharacterCustomizationSaveData LoadCharacterCustomization() {
        return LoadData<CharacterCustomizationSaveData>(characterPath);
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
        SaveData<SettingsSaveData>(settingsData, settingsPath);
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
        if (File.Exists(settingsPath)) {// If there are any, copy the current language
            string jsonData = File.ReadAllText(settingsPath);
            SettingsSaveData oldSettingsData = JsonUtility.FromJson<SettingsSaveData>(jsonData);
            settings.currentLanguage = oldSettingsData.currentLanguage;
        }
        // Save the settings into a file
        SaveData<SettingsSaveData>(settings, settingsPath);
        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(settingsPath, json);
    }

    public static SettingsSaveData LoadSettings() {
        return LoadData<SettingsSaveData>(settingsPath);
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
        SaveData<PlayerStateSaveData>(playerState, playerStatePath);
    }

    public static PlayerStateSaveData LoadPlayerState() {
        return LoadData<PlayerStateSaveData>(playerStatePath);
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
        SaveData<BroomUpgradesSaveData>(broomUpgrades, broomUpgradesPath);
    }

    public static BroomUpgradesSaveData LoadBroomUpgrades() {
        return LoadData<BroomUpgradesSaveData>(broomUpgradesPath);
    }
    #endregion

    #region Spells
    public static void SaveSpells(SpellsSaveData spellsData) {
        SaveData<SpellsSaveData>(spellsData, spellsPath);
    }

    public static SpellsSaveData LoadSpells() {
        return LoadData<SpellsSaveData>(spellsPath);
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
        SaveData<RegionsSaveData>(regionsData, regionsPath);
	}

	public static RegionsSaveData LoadRegions() {
        return LoadData<RegionsSaveData>(regionsPath);
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
        string path = $"{achievementsPartialPath}_{dataIdentifier}{fileExtension}";
        SaveData<T>(data, path);
    }

    public static T LoadAchievementData<T>(string dataIdentifier) {
        string path = $"{achievementsPartialPath}_{dataIdentifier}{fileExtension}";
        return LoadData<T>(path);
    }
    #endregion

    #region Tutorial
    public static void SaveTutorialData(TutorialSaveData data) {
        SaveData<TutorialSaveData>(data, tutorialPath);
    }
    public static TutorialSaveData LoadTutorialData() {
        return LoadData<TutorialSaveData>(tutorialPath);
    }
    #endregion

    #region Quick Race
    public static void SaveQuickRaceData(QuickRaceSaveData data) {
        SaveData<QuickRaceSaveData>(data, quickRacePath);
    }
    public static QuickRaceSaveData LoadQuickRaceData() {
        return LoadData<QuickRaceSaveData>(quickRacePath);
    }
    #endregion

    private static void SaveData<T>(T data, string dataPath) {
        // Save the whole data
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(dataPath, json);
    }

    private static T LoadData<T>(string dataPath) {
        // Load the whole data
        if (File.Exists(dataPath)) {  // If there is a save file, load the data from there
            string json = File.ReadAllText(dataPath);
            T dataObject = JsonUtility.FromJson<T>(json);
            return dataObject;
        } else { // Otherwise return default value
            return default;
        }
    }
    
}
