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
        if (File.Exists(filename)) { // If there is a load file, load the data from there
            string json = File.ReadAllText(filename);
            CharacterCustomizationSaveData characterData = JsonUtility.FromJson<CharacterCustomizationSaveData>(json);
            return characterData;
        } else { // Otherwise use a new instance which will be later initialized with default values
            return new CharacterCustomizationSaveData();
        }
    }
}
