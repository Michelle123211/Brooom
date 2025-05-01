using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


/// <summary>
/// The smallest class describing the current player state, which is used when storing the data persistently.
/// </summary>
[System.Serializable]
public class PlayerStateSaveData {

    // Initialize everything to default values

    /// <summary>Whether the player has completed the game.</summary>
    public bool gameComplete = false;
    /// <summary>Previous and current stats values.</summary>
    public StatisticsSaveData stats = new StatisticsSaveData();
    /// <summary>Current amount of coins</summary>
    public int coins = 0;

    /// <summary>
    /// Known opponents, i.e. name assigned to a particular place in leaderboard.
    /// Getter and setter convert <c>string[]</c> array from <c>knownOpponentsArray</c> to <c>Dictionary&lt;int, string&gt;</c> and vice versa.
    /// </summary>
    public Dictionary<int, string> KnownOpponents {
        get {
            return GetDictionaryOfOpponents(knownOpponentsArray);
        }
        set {
            knownOpponentsArray = GetArrayOfOpponents(value);
        }
    }

    /// <summary>Known opponents from leaderboard stored in array (which is serializable by <c>JsonUtility</c> as opposed to <c>Dictionary</c>).
    /// Each string contains place and name separated by |.</summary>
    public string[] knownOpponentsArray;

    // Creates opponents dictionary out of opponents array
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

    // Creates opponents array out of opponents dictionary
    private string[] GetArrayOfOpponents(Dictionary<int, string> opponentsDictionary) {
        string[] opponentsArray = new string[opponentsDictionary.Count];
        int i = 0;
        foreach (var opponent in opponentsDictionary) {
            opponentsArray[i] = $"{opponent.Key}|{opponent.Value}";
            i++;
        }
        return opponentsArray;
    }

}

/// <summary>
/// Previous stats values and current stats values in a representation used for persistently storing the data.
/// </summary>
[System.Serializable]
public class StatisticsSaveData {
    public PlayerStats previousStats;
    public PlayerStats currentStats;
}
