using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviourSingleton<PlayerState>, ISingleton
{
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

    


    private Dictionary<string, int> broomUpgradeLevels = new Dictionary<string, int>();


    public void ResetState() { 
        // TODO: Reset the game state
    }


    public void InitializeSingleton() {
    }

    public void AwakeSingleton() {
    }


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

}
