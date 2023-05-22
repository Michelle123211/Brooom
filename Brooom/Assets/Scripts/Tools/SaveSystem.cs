using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem
{
    // Names of the save files
    private static readonly string characterFileName = "character_customization";
    private static readonly string settingsFileName = "settings";
    private static readonly string playerStateFileName = "player_state";
    private static readonly string rebindingFileName = "key_bindings";

    // Other parts of the path
    private static readonly string fileExtension = ".json";
    private static readonly string saveFolder = Application.persistentDataPath + "/Saves/";

    // Full paths
    private static readonly string characterPath = saveFolder + characterFileName + fileExtension;
    private static readonly string settingsPath = saveFolder + settingsFileName + fileExtension;
    private static readonly string playerStatePath = saveFolder + playerStateFileName + fileExtension;
    private static readonly string rebindingPath = saveFolder + rebindingFileName + fileExtension;


    static SaveSystem() {
        // Make sure the save folder exists
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
    }

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
        // TODO: Implement
    }

    public static SettingsSaveData LoadSettings() {
        // TODO: Implement
        return null;
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
}
