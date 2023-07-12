using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    #region Money
    public int money = 0;
    #endregion

    #region Spells
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
        raceState = new RaceState(100, new EquippedSpell[4]);
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