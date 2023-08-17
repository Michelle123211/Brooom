using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// A class representing state throughout the whole gameplay
public class PlayerState : MonoBehaviourSingleton<PlayerState>, ISingleton {

    #region Progress
    private bool gameComplete = false;
    public bool GameComplete {
        get => gameComplete;
        set {
            gameComplete = value;
            SaveSystem.SaveGameComplete(gameComplete);
        }
    }
	#endregion

	#region Statistics
	public PlayerStats PreviousStats { get; set; } = new PlayerStats();

    private PlayerStats currentStats = new PlayerStats();
    public PlayerStats CurrentStats {
        get => currentStats;
        set { // Automatically store the previous stats when assigning new ones
            PreviousStats = currentStats;
            currentStats = value;
            // Save the values into a file
            SaveSystem.SavePlayerStatistics(PreviousStats, currentStats);
            // Notify anyone interested that the current stats are different
            Messaging.SendMessage("StatsChanged");
        }
    }
    #endregion

    #region Character Customization
    public CharacterCustomizationOptions customizationOptions;

    private CharacterCustomizationData characterCustomization = null;
    public CharacterCustomizationData CharacterCustomization {
        get {
            if (characterCustomization != null)
                return characterCustomization;
            else {
                // Load it from a file and cache the result
                characterCustomization = new CharacterCustomizationData();
                CharacterCustomizationSaveData characterSaveData = SaveSystem.LoadCharacterCustomization();
                if (characterSaveData != null)
                    characterCustomization.LoadFromSaveData(characterSaveData, customizationOptions);
                else
                    characterCustomization.InitializeToDefaultValues(customizationOptions);
                return characterCustomization;
            }
        }
        set {
            // Save the changes into a file
            characterCustomization = value;
            SaveSystem.SaveCharacterCustomization(characterCustomization.GetSaveData());
        }
    }
    #endregion

    #region Coins
    public int coins = 0; // TODO: Change to property when debugging is done (with private setter, public getter)
    public Action<int, int> onCoinsAmountChanged; // callback invoked whenever the amount of coins changes, parameters are old amount and new amount
    // Returns true if the transaction could be performed (there was enough coins)
    public bool ChangeCoinsAmount(int delta) {
        int newAmount = coins + delta;
        if (newAmount < 0) return false;
        if (newAmount > 999_999) newAmount = 999_999; // cannot go over 999 999
        int oldAmount = coins;
        coins = newAmount;
        // Save the value into a file
        SaveSystem.SaveCoins(coins);
        // Notify anyone interested that the coins amount changed
        onCoinsAmountChanged?.Invoke(oldAmount, newAmount);
        Messaging.SendMessage("CoinsChanged", newAmount - oldAmount);
        return true;
    }
    #endregion

    #region Spells
    [HideInInspector] public Spell[] equippedSpells; // spells assigned to slots
    #endregion

    #region Broom Upgrades
    public float maxAltitude = 15f; // Maximum Y coordinate the player can fly up to
    private Dictionary<string, Tuple<int, int>> broomUpgradeLevels = new Dictionary<string, Tuple<int, int>>(); // current and maximum level for each upgrade

    // Returns the highest purchased level of the given broom upgrade
    // Or -1 if the given broom upgrade is not known
    public int GetBroomUpgradeLevel(string upgradeName) {
        if (broomUpgradeLevels.ContainsKey(upgradeName))
            return broomUpgradeLevels[upgradeName].Item1;
        else {
            return -1;
        }
    }

    // Saves the given level as the highest purchased one for the given broom upgrade
    public void SetBroomUpgradeLevel(string upgradeName, int level, int maxLevel) {
        broomUpgradeLevels[upgradeName] = new Tuple<int, int>(level, maxLevel);
        // Check if all upgrades are purchased
        bool allMax = true;
        foreach (var upgrade in broomUpgradeLevels) {
            if (upgrade.Value.Item1 != upgrade.Value.Item2) {
                allMax = false;
                break;
            }
        }
        // Notify anyone interested that the broom has been upgraded maximally
        if (allMax) Messaging.SendMessage("AllBroomUpgrades");
    }
    #endregion

    #region Opponents
    public Dictionary<int, string> knownOpponents; // stored names of opponents already visible in the leaderboard (according to their place)
	#endregion

	#region Race State
	[Tooltip("The maximum amount of mana the player can have at once.")]
    public int maxManaAmount = 100;
    [HideInInspector]
    public RaceState raceState;
    #endregion


    public void SaveCurrentState() {
        // Use SaveSystem to save all the player's state
        // ...character customization is saved automatically in its setter
        SaveSystem.SavePlayerState(new PlayerStateSaveData {
            gameComplete = this.GameComplete,
            stats = new StatisticsSaveData { 
                previousStats = this.PreviousStats,
                currentStats = this.currentStats
            },
            coins = this.coins,
            KnownOpponents = this.knownOpponents
        });
        SaveSystem.SaveBroomUpgrades();
        SaveSystem.SaveSpells();
        // Save AchievementManager data
        AchievementManager.Instance.SaveAchievementsProgress();
    }

    public void LoadSavedState() {
        // Use SaveSystem to load all the player's state
        // ...character customization is loaded automatically in its getter
        PlayerStateSaveData playerState = SaveSystem.LoadPlayerState();
        LoadFromSavedPlayerState(playerState);
        SaveSystem.LoadBroomUpgrades();
        LoadFromSavedBroomUpgrades(); // TODO: Add parameter
        SaveSystem.LoadSpells();
        LoadFromSavedSpells(); // TODO: Add parameter
        // Load AchievementManager data
        AchievementManager.Instance.LoadAchievementsProgress();
    }

	public void ResetState() {
        // Initialize everything in player's state to default values
        InitializeSingleton();
        // Reset AchievementManager data
        AchievementManager.Instance.ResetAchievementsProgress();
        // Save the default state
        SaveCurrentState();
    }

    private void LoadFromSavedPlayerState(PlayerStateSaveData savedState) {
        GameComplete = savedState.gameComplete;

        CurrentStats = savedState.stats.previousStats;
        CurrentStats = savedState.stats.currentStats;

        coins = savedState.coins;

        knownOpponents = savedState.KnownOpponents;
    }

    private void LoadFromSavedBroomUpgrades() {
        // TODO: Add parameter and implement
    }

    private void LoadFromSavedSpells() {
        // TODO: Add parameter and implement
    }


    #region Singleton
    public void InitializeSingleton() {
        // Initialize everything to default values
        // ...progress
        GameComplete = false;
        // ...statistics
        PreviousStats = new PlayerStats();
        currentStats = new PlayerStats();
        // ...character customization
        characterCustomization = null;
        // ...coins
        coins = 0;
        // ...spells
        equippedSpells = new Spell[4];
        // TODO: Initialize purchased spells
        // ...broom upgrades
        maxAltitude = 15f;
        broomUpgradeLevels = new Dictionary<string, Tuple<int, int>>();
        // ...opponents
        knownOpponents = new Dictionary<int, string>();
        // ...race state
        raceState = new RaceState();

        // TODO: DEBUG only, remove
        equippedSpells[0] = new Spell();
        equippedSpells[0].identifier = "Test";
        equippedSpells[1] = new Spell();
        equippedSpells[1].identifier = "Test";
        equippedSpells[3] = new Spell();
        equippedSpells[3].identifier = "Test";
    }

    public void AwakeSingleton() {
    }
	#endregion

}

[System.Serializable]
public struct PlayerStats {
    [Range(0, 100)]
    public int endurance;
    [Range(0, 100)]
    public int speed;
    [Range(0, 100)]
    public int dexterity;
    [Range(0, 100)]
    public int precision;
    [Range(0, 100)]
    public int magic;

    // Computes weighted average of the stats
    // weight 3: precision, speed (more important and objective)
    // weight 2: endurance (artificially lowering the score at the beginning)
    // weight 1: magic (artificially lowering the score at the beginning), dexterity (not precise)
    public float GetWeightedAverage() {
        return (3 * (precision + speed) + 2 * (endurance) + 1 * (magic + dexterity)) / 10f;
    }

    // Returns the values in a specific order
    public List<float> GetListOfValues() {
        return new List<float> { endurance, speed, dexterity, precision, magic };
    }

    // Returns stats names in a specific order
    public static List<string> GetListOfStatNames() {
        return new List<string> { "Endurance", "Speed", "Dexterity", "Precision", "Magic" };
    }
}