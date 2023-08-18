using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// The smallest class describing the current player state
[System.Serializable]
public class PlayerStateSaveData {
    // Initialize everything to default values
    public bool gameComplete = false;
    public StatisticsSaveData stats = new StatisticsSaveData();
    public int coins = 0;
    public Dictionary<int, string> KnownOpponents {
        get {
            return GetDictionaryOfOpponents(knownOpponentsArray);
        }
        set {
            knownOpponentsArray = GetArrayOfOpponents(value);
        }
    }
    // Known opponents stored in array (which is serializable by JsonUtility as opposed to Dictionary)
    public string[] knownOpponentsArray; // each string contains index and name separated by |

    private Dictionary<int, string> GetDictionaryOfOpponents(string[] opponentsArray) {
        Dictionary<int, string> opponentsDictionary = new Dictionary<int, string>();
        foreach (var opponent in opponentsArray) {
            string[] parts = opponent.Split('|');
            if (parts.Length == 2 && int.TryParse(parts[0], out int index)) {
                opponentsDictionary[index] = parts[1];
            }
        }
        return opponentsDictionary;
    }

    private string[] GetArrayOfOpponents(Dictionary<int, string> opponentsDictionary) {
        string[] opponentsArray = new string[opponentsDictionary.Keys.Count];
        int i = 0;
        foreach (var opponent in opponentsDictionary) {
            opponentsArray[i] = $"{opponent.Key}|{opponent.Value}";
            i++;
        }
        return opponentsArray;
    }
}

[System.Serializable]
public class StatisticsSaveData {
    public PlayerStats previousStats;
    public PlayerStats currentStats;
}
