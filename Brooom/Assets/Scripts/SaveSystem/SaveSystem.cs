using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;


/// <summary>
/// A static class providing methods for saving and loading particular parts of game state.
/// All data is serialized to JSON and stored in <c>Saves/</c> folder.
/// </summary>
public static class SaveSystem {

    // Names of the save files
    /// <summary>Name of the save file (without extension) for storing character customization data.</summary>
    private static readonly string characterFileName = "character_customization";
    /// <summary>Name of the save file (without extension) for storing settings data (excluding key bindings).</summary>
    private static readonly string settingsFileName = "settings";
    /// <summary>Name of the save file (without extension) for storing key bindings data.</summary>
    private static readonly string rebindingFileName = "key_bindings";
    /// <summary>Name of the save file (without extension) for storing player state data.</summary>
    private static readonly string playerStateFileName = "player_state";
    /// <summary>Name of the save file (without extension) for storing broom upgrades data.</summary>
    private static readonly string broomUpgradesFileName = "broom_upgrades";
    /// <summary>Name of the save file (without extension) for storing spells data.</summary>
    private static readonly string spellsFileName = "spells";
    /// <summary>Name of the save file (without extension) for storing regions data.</summary>
    private static readonly string regionsFileName = "regions";
    /// <summary>Name of the save file (without extension) for storing achievements data.</summary>
    private static readonly string achievementsFileName = "achievements";
    /// <summary>Name of the save file (without extension) for storing tutorial data.</summary>
    private static readonly string tutorialFileName = "tutorial";
    /// <summary>Name of the save file (without extension) for storing quick race data.</summary>
    private static readonly string quickRaceFileName = "quick_race";

    // Other parts of the path
    /// <summary>File extension (including dot in front of it) used for any save file.</summary>
    private static readonly string fileExtension = ".json";
    /// <summary>Path of the folder containing all save files.</summary>
    private static readonly string saveFolder = Application.persistentDataPath + "/Saves/";
    /// <summary>Path of the subfolder used for storing save backup.</summary>
    private static readonly string backupFolder = saveFolder + "Backup/";

    // Full paths
    /// <summary>Full path of the save file for storing character customization data.</summary>
    private static readonly string characterPath = saveFolder + characterFileName + fileExtension;
    /// <summary>Full path of the save file for storing settings data (excluding key bindings).</summary>
    private static readonly string settingsPath = saveFolder + settingsFileName + fileExtension;
    /// <summary>Full path of the save file for storing key bindings data.</summary>
    private static readonly string rebindingPath = saveFolder + rebindingFileName + fileExtension;
    /// <summary>Full path of the save file for storing player state data.</summary>
    private static readonly string playerStatePath = saveFolder + playerStateFileName + fileExtension;
    /// <summary>Full path of the save file for storing broom upgrades data.</summary>
    private static readonly string broomUpgradesPath = saveFolder + broomUpgradesFileName + fileExtension;
    /// <summary>Full path of the save file for storing spells data.</summary>
    private static readonly string spellsPath = saveFolder + spellsFileName + fileExtension;
    /// <summary>Full path of the save file for storing regions data.</summary>
    private static readonly string regionsPath = saveFolder + regionsFileName + fileExtension;
    /// <summary>Full path of the save file for storing achievements data.</summary>
    private static readonly string achievementsPartialPath = saveFolder + achievementsFileName;
    /// <summary>Full path of the save file for storing tutorial data.</summary>
    private static readonly string tutorialPath = saveFolder + tutorialFileName + fileExtension;
    /// <summary>Full path of the save file for storing quick race data.</summary>
    private static readonly string quickRacePath = saveFolder + quickRaceFileName + fileExtension;


    static SaveSystem() {
        // Make sure the saves folder exists
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
    }

    /// <summary>
    /// Checks whether a game save exists (i.e. the game has been started previously).
    /// </summary>
    /// <returns><c>true</c> if save exists, <c>false</c> otherwise.</returns>
    public static bool SaveExists() {
        // Saves folder exists and there is file for character customization (implying a game has been started previously)
        if (!Directory.Exists(saveFolder)) return false;
        return File.Exists(characterPath);
    }

    /// <summary>
    /// Checks whether a backup of current game state exists, which is indicated by existence of <c>Saves\Backup\</c> folder.
    /// </summary>
    /// <returns>True if backup exists, false otherwise.</returns>
    public static bool BackupExists() {
        // Saves folder exists and there is a Backup subfolder
        if (!Directory.Exists(saveFolder)) return false;
        return Directory.Exists(backupFolder);
    }

    /// <summary>
    /// Creates a backup of the currently saved game state, while deleting any previously existing backup.
    /// </summary>
    public static void CreateBackup() {
        // If there is backup already, delete it
        DeleteBackup();
        // Copy all files to a subfolder (except settings, key rebinding and quick race, because that is not specific for a game and can be changed outside of it)
        Directory.CreateDirectory(backupFolder);
        foreach (var file in Directory.EnumerateFiles(saveFolder)) {
            string fileName = file.Substring(saveFolder.Length);
            if (fileName.Contains(settingsFileName) || fileName.Contains(rebindingFileName) || fileName.Contains(quickRaceFileName)) continue;
            File.Copy(file, backupFolder + fileName, true);
        }
    }

    /// <summary>
    /// Restores the current game state to a state from backup. Then deletes all backup files completely.
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
    /// Completely deletes all backup files.
    /// </summary>
    public static void DeleteBackup() { 
        if (Directory.Exists(backupFolder))
            Directory.Delete(backupFolder, true);
    }

	#region Character customization
    /// <summary>
    /// Saves the given character customization data into a file.
    /// </summary>
    /// <param name="characterData">Character customization data to be saved.</param>
	public static void SaveCharacterCustomization(CharacterCustomizationSaveData characterData) {
        SaveData<CharacterCustomizationSaveData>(characterData, characterPath);
    }

    /// <summary>
    /// Loads character customization data from a save file.
    /// </summary>
    /// <returns>Saved character customization data.</returns>
    public static CharacterCustomizationSaveData LoadCharacterCustomization() {
        return LoadData<CharacterCustomizationSaveData>(characterPath);
    }
	#endregion

	#region Settings
    /// <summary>
    /// Saves the given language settings into a file.
    /// </summary>
    /// <param name="language">Language to be saved.</param>
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

    /// <summary>
    /// Loads language settings from a save file.
    /// </summary>
    /// <returns>Saved language.</returns>
    public static string LoadCurrentLanguage() {
        if (File.Exists(settingsPath)) { // If there is a save file, load the data from there
            string json = File.ReadAllText(settingsPath);
            SettingsSaveData settingsData = JsonUtility.FromJson<SettingsSaveData>(json);
            return settingsData.currentLanguage;
        } else { // Otherwise return null
            return null;
        }
    }

    /// <summary>
    /// Saves the given settings values into a file.
    /// </summary>
    /// <param name="settings">Settings values to be saved.</param>
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

    /// <summary>
    /// Loads settings values from a save file.
    /// </summary>
    /// <returns>Saved settings values.</returns>
    public static SettingsSaveData LoadSettings() {
        return LoadData<SettingsSaveData>(settingsPath);
    }

    /// <summary>
    /// Saves the given key binding data (as a JSON) into a file.
    /// </summary>
    /// <param name="bindingsAsJson">Key binding data in JSON format.</param>
	public static void SaveKeyBindings(string bindingsAsJson) {
        File.WriteAllText(rebindingPath, bindingsAsJson);
    }

    /// <summary>
    /// Loads key binding data from a save file.
    /// </summary>
    /// <returns>Saved key binding data in JSON format.</returns>
    public static string LoadKeyBindings() {
        if (File.Exists(rebindingPath)) { // If there is a save file, load the data from there
            return File.ReadAllText(rebindingPath);
        } else { // Otherwise return null
            return null;
        }
    }
    #endregion

    #region Player state
    /// <summary>
    /// Saves the given player state data into a file.
    /// </summary>
    /// <param name="playerState">Player state data to be saved.</param>
    public static void SavePlayerState(PlayerStateSaveData playerState) {
        SaveData<PlayerStateSaveData>(playerState, playerStatePath);
    }

    /// <summary>
    /// Loads player state data from a save file.
    /// </summary>
    /// <returns>Saved player state data.</returns>
    public static PlayerStateSaveData LoadPlayerState() {
        return LoadData<PlayerStateSaveData>(playerStatePath);
    }

    /// <summary>
    /// Saves the given player statistics into a file.
    /// </summary>
    /// <param name="previousStats">Previous stats values to be saved.</param>
    /// <param name="currentStats">Current stats values to be saved.</param>
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

    /// <summary>
    /// Loads player statistics data from a save file.
    /// </summary>
    /// <returns>Saved player statistics data.</returns>
    public static StatisticsSaveData LoadPlayerStatistics() {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) return null; // no saved state
        // Return only the statistics
        return playerState.stats;
    }

    /// <summary>
    /// Saves the given known opponents data into a file.
    /// </summary>
    /// <param name="knownOpponents">Data about known opponents to be saved.</param>
    public static void SaveKnownOpponents(Dictionary<int, string> knownOpponents) {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) playerState = new PlayerStateSaveData(); // if there is no save file, use default values
        // Override only the opponents
        playerState.KnownOpponents = knownOpponents;
        // Save it back
        SavePlayerState(playerState);
    }

    /// <summary>
    /// Loads data about known opponents from a save file.
    /// </summary>
    /// <returns>Saved known opponents data.</returns>
    public static Dictionary<int, string> LoadKnownOpponents() {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) return null; // no saved state
        // Return only the opponents
        return playerState.KnownOpponents;
    }

    /// <summary>
    /// Saves the given information, about whether the game has been completed, into a file.
    /// </summary>
    /// <param name="gameComplete">Game completion state to be saved.</param>
    public static void SaveGameComplete(bool gameComplete) {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) playerState = new PlayerStateSaveData(); // if there is no save file, use default values
        // Override only the bool
        playerState.gameComplete = gameComplete;
        // Save it back
        SavePlayerState(playerState);
    }

    /// <summary>
    /// Loads information, about whether the game has been completed, from a save file.
    /// </summary>
    /// <returns>Saved game completion state.</returns>
    public static bool LoadGameComplete() {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) return false; // no saved state
        // Return only the bool
        return playerState.gameComplete;
    }

    /// <summary>
    /// Saves the given coins amount into a file.
    /// </summary>
    /// <param name="coins">Coins amount to be saved.</param>
    public static void SaveCoins(int coins) {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) playerState = new PlayerStateSaveData(); // if there is no save file, use default values
        // Override only the coins
        playerState.coins = coins;
        // Save it back
        SavePlayerState(playerState);
    }

    /// <summary>
    /// Loads coins amount from a save file.
    /// </summary>
    /// <returns>Saved coins amount.</returns>
    public static int LoadCoins() {
        // Load the whole player state
        PlayerStateSaveData playerState = LoadPlayerState();
        if (playerState == null) return 0; // no saved state
        // Return only the coins
        return playerState.coins;
    }
    #endregion

    #region Broom upgrades
    /// <summary>
    /// Saves the given broom upgrades data into a file.
    /// </summary>
    /// <param name="broomUpgrades">Broom upgrades data to be saved.</param>
    public static void SaveBroomUpgrades(BroomUpgradesSaveData broomUpgrades) {
        SaveData<BroomUpgradesSaveData>(broomUpgrades, broomUpgradesPath);
    }

    /// <summary>
    /// Loads broom upgrades data from a save file.
    /// </summary>
    /// <returns>Saved broom upgrades data.</returns>
    public static BroomUpgradesSaveData LoadBroomUpgrades() {
        return LoadData<BroomUpgradesSaveData>(broomUpgradesPath);
    }
    #endregion

    #region Spells
    /// <summary>
    /// Saves the given spells data into a file.
    /// </summary>
    /// <param name="spellsData">Spells data to be saved.</param>
    public static void SaveSpells(SpellsSaveData spellsData) {
        SaveData<SpellsSaveData>(spellsData, spellsPath);
    }

    /// <summary>
    /// Loads spells data from a save file.
    /// </summary>
    /// <returns>Saved spells data.</returns>
    public static SpellsSaveData LoadSpells() {
        return LoadData<SpellsSaveData>(spellsPath);
    }

    /// <summary>
    /// Saves the given spells availability data into a file.
    /// </summary>
    /// <param name="spellsAvailability">Spells availability data to be saved.</param>
    public static void SavePurchasedSpells(Dictionary<string, bool> spellsAvailability) {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        if (spellsData == null) spellsData = new SpellsSaveData(); // if there is no save file, use default values
        // Override only the purchased spells
        spellsData.SpellsAvailability = spellsAvailability;
        // Save it back
        SaveSpells(spellsData);
    }

    /// <summary>
    /// Loads spells availability data from a save file.
    /// </summary>
    /// <returns>Saved spells availability data.</returns>
    public static Dictionary<string, bool> LoadPurchasedSpells() {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        if (spellsData == null) return null; // no saved state
        // Return only the purchased spells
        return spellsData.SpellsAvailability;
    }

    /// <summary>
    /// Saves the given equipped spells slots into a file.
    /// </summary>
    /// <param name="equippedSpellsData">Equipped spells slot to be saved.</param>
    public static void SaveEquippedSpells(Spell[] equippedSpellsData) {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        if (spellsData == null) spellsData = new SpellsSaveData(); // if there is no save file, use default values
        // Override only the equipped spells
        spellsData.EquippedSpells = equippedSpellsData;
        // Save it back
        SaveSpells(spellsData);
    }

    /// <summary>
    /// Loads equipped spells slots from a save file.
    /// </summary>
    /// <returns>Saved equipped spells slots.</returns>
    public static Spell[] LoadEquippedSpells() {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        if (spellsData == null) return null; // no saved state
        // Return only the equipped spells
        return spellsData.EquippedSpells;
    }

    /// <summary>
    /// Saves the given information, about whether spells have been used, into a file.
    /// </summary>
    /// <param name="spellsUsage">Spells usage data to be saved.</param>
    public static void SaveCastSpells(Dictionary<string, bool> spellsUsage) {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        if (spellsData == null) spellsData = new SpellsSaveData(); // if there is no save file, use default values
        // Override only the cast spells
        spellsData.SpellsUsage = spellsUsage;
        // Save it back
        SaveSpells(spellsData);
    }

    /// <summary>
    /// Loads information, about whether spells have been used, from a save file.
    /// </summary>
    /// <returns>Saved spells usage data.</returns>
    public static Dictionary<string, bool> LoadCastSpells() {
        // Load everything
        SpellsSaveData spellsData = LoadSpells();
        if (spellsData == null) return null; // no saved state
        // Return only the cast spells
        return spellsData.SpellsUsage;
    }
	#endregion

	#region Visited regions
    /// <summary>
    /// Saves the given regions data into a file.
    /// </summary>
    /// <param name="regionsData">Regions data to be saved.</param>
	public static void SaveRegions(RegionsSaveData regionsData) {
        SaveData<RegionsSaveData>(regionsData, regionsPath);
	}

    /// <summary>
    /// Loads regions data from a save file.
    /// </summary>
    /// <returns>Saved regions data.</returns>
	public static RegionsSaveData LoadRegions() {
        return LoadData<RegionsSaveData>(regionsPath);
	}

    /// <summary>
    /// Saves the given information, about whether regions have been visited, into a file.
    /// </summary>
    /// <param name="visitedRegions">Information about (un)visited regions to be saved.</param>
	public static void SaveVisitedRegions(Dictionary<LevelRegionType, bool> visitedRegions) {
		// Load everything
		RegionsSaveData regionsData = LoadRegions();
		if (regionsData == null) regionsData = new RegionsSaveData(); // if there is no save file, use default values
		 // Override only the visited regions
		regionsData.RegionsVisited = visitedRegions;
		// Save it back
		SaveRegions(regionsData);
	}

    /// <summary>
    /// Loads information, about whether regions have been visited, from a save file.
    /// </summary>
    /// <returns>Saved information about (un)visited regions.</returns>
    public static Dictionary<LevelRegionType, bool> LoadVisitedRegions() {
		// Load everything
		RegionsSaveData regionsData = LoadRegions();
		if (regionsData == null) return null; // no saved state
        // Return only the visited regions
        return regionsData.RegionsVisited;
	}
	#endregion

	#region Achievements
    /// <summary>
    /// Saves the given achievements data into a file.
    /// </summary>
    /// <typeparam name="T">Type of the achievements data (achievements are composed of several subcategories, each handled by a separate type).</typeparam>
    /// <param name="data">Achievements data to be saved.</param>
    /// <param name="dataIdentifier">Identifier of the achievements subcategory, used in a filename.</param>
	public static void SaveAchievementData<T>(T data, string dataIdentifier) {
        string path = $"{achievementsPartialPath}_{dataIdentifier}{fileExtension}";
        SaveData<T>(data, path);
    }

    /// <summary>
    /// Loads achievements data of the given type from a save file.
    /// </summary>
    /// <typeparam name="T">Type of the achievements data (achievements are composed of several subcategories, each handled by a separate type).</typeparam>
    /// <param name="dataIdentifier">Identifier of the achievements subcategory, used in a filename.</param>
    /// <returns>Saved achievements data.</returns>
    public static T LoadAchievementData<T>(string dataIdentifier) {
        string path = $"{achievementsPartialPath}_{dataIdentifier}{fileExtension}";
        return LoadData<T>(path);
    }
    #endregion

    #region Tutorial
    /// <summary>
    /// Saves the given tutorial data into a file.
    /// </summary>
    /// <param name="data">Tutorial data to be saved.</param>
    public static void SaveTutorialData(TutorialSaveData data) {
        SaveData<TutorialSaveData>(data, tutorialPath);
    }

    /// <summary>
    /// Loads tutorial data from a save file.
    /// </summary>
    /// <returns>Saved tutorial data.</returns>
    public static TutorialSaveData LoadTutorialData() {
        return LoadData<TutorialSaveData>(tutorialPath);
    }
    #endregion

    #region Quick Race
    /// <summary>
    /// Saves the given quick race settings into a file.
    /// </summary>
    /// <param name="data">Quick race settings to be saved.</param>
    public static void SaveQuickRaceData(QuickRaceSaveData data) {
        SaveData<QuickRaceSaveData>(data, quickRacePath);
    }

    /// <summary>
    /// Loads quick race settings from a save file.
    /// </summary>
    /// <returns>Saved quick race settings.</returns>
    public static QuickRaceSaveData LoadQuickRaceData() {
        return LoadData<QuickRaceSaveData>(quickRacePath);
    }
    #endregion

    /// <summary>
    /// Serializes the given data into JSON and saves it into the given file.
    /// </summary>
    /// <typeparam name="T">Type of the data to be saved.</typeparam>
    /// <param name="data">Data to be saved.</param>
    /// <param name="dataPath">Path of the save file.</param>
    private static void SaveData<T>(T data, string dataPath) {
        // Save the whole data
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(dataPath, json);
    }

    /// <summary>
    /// Loads content of the given file and deserializes it from JSON to the given type.
    /// If the given file doesn't exist, default values are used instead.
    /// </summary>
    /// <typeparam name="T">Type of the data to obtain.</typeparam>
    /// <param name="dataPath">Path of the save file.</param>
    /// <returns>Data loaded from the save file, or <c>default</c> (if the file doesn't exist).</returns>
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
