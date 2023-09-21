using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Broom : MonoBehaviour
{
	private BroomUpgrade[] upgradesAvailable;

	private bool isPlayer = false;

	// Set everything according to the the player's highest purchased levels of each broom upgrade
	public void UpdateFromPlayerState() {
		// Make sure all upgrades are known in the PlayerState
		foreach (var upgrade in upgradesAvailable) {
			if (PlayerState.Instance.GetBroomUpgradeLevel(upgrade.UpgradeName) < 0)
				PlayerState.Instance.SetBroomUpgradeLevel(upgrade.UpgradeName, 0, upgrade.MaxLevel);
		}
		// Level up according to the PlayerState
		foreach (var upgrade in upgradesAvailable) {
			int targetLevel = PlayerState.Instance.GetBroomUpgradeLevel(upgrade.UpgradeName);
			while (upgrade.CurrentLevel < targetLevel)
				upgrade.LevelUp();
		}
	}

	public BroomUpgrade[] GetAvailableUpgrades() {
		return upgradesAvailable;
	}

	public void RandomizeStateCloseToPlayer() {
		// Choose random level of broom upgrades (at least the same Elevation as the player, the same total number of upgrades +/- 1)
		int totalPlayerLevels = 0;
		foreach (var upgrade in upgradesAvailable) {
			int upgradeLevel = PlayerState.Instance.GetBroomUpgradeLevel(upgrade.UpgradeName);
			if (upgradeLevel > 0) totalPlayerLevels += upgradeLevel;
		}
		int levelsToDistribute = totalPlayerLevels;
		// Make sure the Elevation is at least the one of the player
		BroomUpgrade elevationUpgrade = null;
		foreach (var upgrade in upgradesAvailable)
			if (upgrade.UpgradeName == "Elevation") {
				elevationUpgrade = upgrade;
				break;
			}
		int elevationLevel = PlayerState.Instance.GetBroomUpgradeLevel("Elevation");
		if (elevationUpgrade != null && elevationLevel > -1) {
			while (elevationUpgrade.CurrentLevel < elevationLevel) {
				elevationUpgrade.LevelUp();
			}
			levelsToDistribute -= elevationUpgrade.CurrentLevel;
		}
		// Distribute the rest randomly
		levelsToDistribute += Random.Range(-1, 2); // just a small offset
		if (levelsToDistribute > 0) RandomizeState(levelsToDistribute);
	}

	public void RandomizeState() {
		// // Choose random level of broom upgrades
		int levelsTotal = 0;
		foreach (var upgrade in upgradesAvailable)
			levelsTotal += (upgrade.MaxLevel - upgrade.CurrentLevel);
		RandomizeState(Random.Range(0, levelsTotal + 1));
	}

	private void RandomizeState(int levelsToDistribute) {
		// Distribute the levels randomly
		while (levelsToDistribute > 0) {
			// Randomly select the upgrade to level up
			int upgradeIndex = Random.Range(0, upgradesAvailable.Length);
			if (!TryToLevelUpgradeUp(upgradesAvailable[upgradeIndex])) {
				// If it is already at the maximum, try the next upgrade
				int initialUpgradeIndex = upgradeIndex;
				upgradeIndex = (upgradeIndex + 1) % upgradesAvailable.Length;
				while (upgradeIndex != initialUpgradeIndex) {
					if (TryToLevelUpgradeUp(upgradesAvailable[upgradeIndex])) {
						break;
					} else {
						upgradeIndex = (upgradeIndex + 1) % upgradesAvailable.Length;
					}
				}
				if (upgradeIndex == initialUpgradeIndex) {
					// A full cycle was done, all upgrades are at their max levels
					break;
				}
			}
			levelsToDistribute--;
		}
	}

	private bool TryToLevelUpgradeUp(BroomUpgrade upgrade) {
		if (upgrade.CurrentLevel < upgrade.MaxLevel) {
			upgrade.LevelUp();
			return true;
		} else {
			return false;
		}
	}

	private void Awake() {
		isPlayer = CompareTag("Player");
		// Get all the upgrades available
		upgradesAvailable = GetComponentsInChildren<BroomUpgrade>();
	}

	void Start() {
		if (isPlayer) {
			// Initialize individual upgrades from the PlayerState
			UpdateFromPlayerState();
		}
    }
}
