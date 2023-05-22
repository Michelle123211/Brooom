using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem
{
    // Names of the save files
    private static string characterFileName = "character_customization";
    private static string settingsFileName = "settings";
    private static string playerStateFileName = "player_state";
    private static string rebindingFileName = "key_bindings";

    // Other parts of the path
    private static string fileExtension = ".json";
    private static string path = Application.persistentDataPath + "/";


    public static void SaveCharacterCustomization(CharacterCustomizationSaveData characterData) {
        string json = JsonUtility.ToJson(characterData);
        File.WriteAllText(path + characterFileName + fileExtension, json);
    }

    public static CharacterCustomizationSaveData LoadCharacterCustomization() {
        string filename = path + characterFileName + fileExtension;
        if (File.Exists(filename)) { // If there is a save file, load the data from there
            string json = File.ReadAllText(filename);
            CharacterCustomizationSaveData characterData = JsonUtility.FromJson<CharacterCustomizationSaveData>(json);
            return characterData;
        } else { // Otherwise return null
            return null;
        }
    }

    public static void SaveCurrentLanguage(string language) {
        string filename = path + settingsFileName + fileExtension;
        SettingsSaveData settingsData;
        if (File.Exists(filename)) { // If there is a save file, load the data from there
            string jsonData = File.ReadAllText(filename);
            settingsData = JsonUtility.FromJson<SettingsSaveData>(jsonData);
        } else { // Otherwise use a new instance with default values
            settingsData = new SettingsSaveData();
        }
        // Override only the language field and save it
        settingsData.currentLanguage = language;
        string json = JsonUtility.ToJson(settingsData);
        File.WriteAllText(filename, json);
    }

    public static string LoadCurrentLanguage() {
        string filename = path + settingsFileName + fileExtension;
        if (File.Exists(filename)) { // If there is a save file, load the data from there
            string json = File.ReadAllText(filename);
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
        File.WriteAllText(path + rebindingFileName + fileExtension, bindingsAsJson);
    }

    public static string LoadKeyBindings() {
        string filename = path + rebindingFileName + fileExtension;
        if (File.Exists(filename)) { // If there is a save file, load the data from there
            return File.ReadAllText(filename);
        } else { // Otherwise return null
            return null;
        }
    }
}
