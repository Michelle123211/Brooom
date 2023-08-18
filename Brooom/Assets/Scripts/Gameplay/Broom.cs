using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Broom : MonoBehaviour
{
	BroomUpgrade[] upgradesAvailable;

	// Set everything according to the the highest purchased levels of each broom upgrade
	public void UpdateState() {
		foreach (var upgrade in upgradesAvailable) {
			int targetLevel = PlayerState.Instance.GetBroomUpgradeLevel(upgrade.UpgradeName);
			if (targetLevel == -1) { // this broom upgrade is not known, register it
				PlayerState.Instance.SetBroomUpgradeLevel(upgrade.UpgradeName, 0, upgrade.MaxLevel);
			}
			while (upgrade.CurrentLevel < targetLevel)
				upgrade.LevelUp();
		}
	}

	public BroomUpgrade[] GetAvailableUpgrades() {
		return upgradesAvailable;
	}

	private void Awake() {
		// Get all the upgrades available
		upgradesAvailable = GetComponentsInChildren<BroomUpgrade>();
	}


	// Start is called before the first frame update
	void Start()
    {
		// Initialize individual upgrades from the PlayerState
		UpdateState();
    }
}
