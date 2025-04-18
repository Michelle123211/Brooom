using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class BroomUpgradesSaveData {
    public Dictionary<string, (int currentLevel, int maxLevel)> UpgradeLevels {
        get {
            return GetDictionaryOfUpgrades(upgradeLevelsArray);
        }
        set {
            upgradeLevelsArray = GetArrayOfUpgrades(value);
        }
    }
    // Broom upgrades stored in array (which is serializable by JsonUtility as opposed to Dictionary)
    public string[] upgradeLevelsArray = null; // each string contains upgrade name, current level and max level separated by |

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
