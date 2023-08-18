using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BroomUpgradesSaveData {
    public Dictionary<string, Tuple<int, int>> UpgradeLevels {
        get {
            return GetDictionaryOfUpgrades(upgradeLevelsArray);
        }
        set {
            upgradeLevelsArray = GetArrayOfUpgrades(value);
        }
    }
    // Known opponents stored in array (which is serializable by JsonUtility as opposed to Dictionary)
    public string[] upgradeLevelsArray = null; // each string contains upgrade name, current level and max level separated by |

    private Dictionary<string, Tuple<int, int>> GetDictionaryOfUpgrades(string[] upgradesArray) {
        Dictionary<string, Tuple<int, int>> upgradesDictionary = new Dictionary<string, Tuple<int, int>>();
        foreach (var upgrade in upgradesArray) {
            string[] parts = upgrade.Split('|');
            if (parts.Length == 3 && int.TryParse(parts[1], out int currentLevel) && int.TryParse(parts[2], out int maxLevel)) {
                upgradesDictionary[parts[0]] = new Tuple<int, int>(currentLevel, maxLevel);
            }
        }
        return upgradesDictionary;
    }

    private string[] GetArrayOfUpgrades(Dictionary<string, Tuple<int, int>> upgradesDictionary) {
        string[] upgradesArray = new string[upgradesDictionary.Count];
        int i = 0;
        foreach (var upgrade in upgradesDictionary) {
            upgradesArray[i] = $"{upgrade.Key}|{upgrade.Value.Item1}|{upgrade.Value.Item2}";
            i++;
        }
        return upgradesArray;
    }
}
