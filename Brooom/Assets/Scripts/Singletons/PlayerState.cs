using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// A class representing state throughout the whole gameplay
public class PlayerState : MonoBehaviourSingleton<PlayerState>, ISingleton
{
    public PlayerStats stats = new PlayerStats();

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
        int oldAmount = coins;
        coins = newAmount;
        onCoinsAmountChanged(oldAmount, newAmount);
        return true;
    }
    #endregion

    #region Spells
    [HideInInspector] public Spell[] equippedSpells; // spells assigned to slots
    #endregion


    #region Broom Upgrades
    public float maxAltitude = 15f; // Maximum Y coordinate the player can fly up to
    private Dictionary<string, int> broomUpgradeLevels = new Dictionary<string, int>();

    // Returns the highest purchased level of the given broom upgrade
    public int GetBroomUpgradeLevel(string upgradeName) {
        if (broomUpgradeLevels.ContainsKey(upgradeName))
            return broomUpgradeLevels[upgradeName];
        else
            return 0;
    }

    // Saves the given level as the highest purchased one for the given broom upgrade
    public void SetBroomUpgradeLevel(string upgradeName, int level) {
        broomUpgradeLevels[upgradeName] = level;
    }
    #endregion

    #region Race State
    [Tooltip("The maximum amount of mana the player can have at once.")]
    public int maxManaAmount = 100;
    [HideInInspector]
    public RaceState raceState;
	#endregion


	public void ResetState() { 
        // TODO: Reset the game state
    }


    #region Singleton
    public void InitializeSingleton() {
        equippedSpells = new Spell[4];
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
}