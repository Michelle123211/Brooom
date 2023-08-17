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
            return ParseKnownOpponentsFromString();
        }
        set {
            knownOpponentsString = ConvertKnownOpponentsToString(value);
        }
    }
    // Known opponents stored in string (which is serializable by JsonUtility as opposed to Dictionary)
    public string knownOpponentsString = string.Empty; // all parts divided by |

    private string ConvertKnownOpponentsToString(Dictionary<int, string> knownOpponents) {
        StringBuilder result = new StringBuilder();
        foreach (var opponent in knownOpponents) {
            result.Append(opponent.Key.ToString());
            result.Append('|');
            result.Append(opponent.Value.ToString());
            result.Append('|');
        }
        return result.ToString();
    }

    private Dictionary<int, string> ParseKnownOpponentsFromString() {
        Dictionary<int, string> opponents = new Dictionary<int, string>();
        string[] parts = knownOpponentsString.Split('|', System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i += 2) {
            if (int.TryParse(parts[i], out int index)) {
                opponents[index] = parts[i + 1];
            }
        }
        return opponents;
    }
}

[System.Serializable]
public class StatisticsSaveData {
    public PlayerStats previousStats;
    public PlayerStats currentStats;
}
