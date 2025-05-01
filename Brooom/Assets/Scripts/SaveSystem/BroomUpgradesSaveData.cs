using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


/// <summary>
/// A class used when persistently storing data related to broom upgrade levels.
/// </summary>
[System.Serializable]
public class BroomUpgradesSaveData {

    /// <summary>
    /// The current level and the maximum level of each broom upgrade represented by its name.
    /// Getter and setter convert <c>string[]</c> array from <c>upgradeLevelsArray</c> to <c>Dictionary&lt;LevelRegionType, bool&gt;</c> and vice versa.
    /// </summary>
    public Dictionary<string, (int currentLevel, int maxLevel)> UpgradeLevels {
        get {
            return GetDictionaryOfUpgrades(upgradeLevelsArray);
        }
        set {
            upgradeLevelsArray = GetArrayOfUpgrades(value);
        }
    }
    /// <summary>Broom upgrades stored in array (which is serializable by <c>JsonUtility</c> as opposed to <c>Dictionary</c>).
    /// Each string contains upgrade name, current level and max level separated by |.</summary>
    public string[] upgradeLevelsArray = null;


    // Creates broom upgrades dictionary out of broom upgrades array
    private Dictionary<string, (int currentLevel, int maxLevel)> GetDictionaryOfUpgrades(string[] upgradesArray) {
        Dictionary<string, (int currentLevel, int maxLevel)> upgradesDictionary = new Dictionary<string, (int currentLevel, int maxLevel)>();
        foreach (var upgrade in upgradesArray) {
            string[] parts = upgrade.Split('|');
            if (parts.Length == 3 && int.TryParse(parts[1], out int currentLevel) && int.TryParse(parts[2], out int maxLevel)) {
                upgradesDictionary[parts[0]] = (currentLevel, maxLevel);
            }
        }
        return upgradesDictionary;
    }

    // Creates broom upgrades array out of broom upgrades dictionary
    private string[] GetArrayOfUpgrades(Dictionary<string, (int currentLevel, int maxLevel)> upgradesDictionary) {
        string[] upgradesArray = new string[upgradesDictionary.Count];
        int i = 0;
        foreach (var upgrade in upgradesDictionary) {
            upgradesArray[i] = $"{upgrade.Key}|{upgrade.Value.currentLevel}|{upgrade.Value.maxLevel}";
            i++;
        }
        return upgradesArray;
    }
}
