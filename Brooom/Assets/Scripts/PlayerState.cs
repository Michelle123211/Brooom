using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviourSingleton<PlayerState>, IInitializableSingleton
{
    private Dictionary<string, int> broomUpgradeLevels = new Dictionary<string, int>();

    public void InitializeSingleton() {

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
