using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentUpgradeButtons : MonoBehaviour
{
	public Broom broom;
	BroomUpgrade[] upgrades;

	private int width = 200;
	private int height = 70;
	private int offsetX = 10;
	private int offsetY = 10;

	private void Start() {
		upgrades = broom.GetAvailableUpgrades();
	}

	private void OnGUI() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		GUI.skin.button.fontSize = 24;

		int column = 0;
		foreach (var upgrade in upgrades) { // speed, control, elevation
			if (GUI.Button(new Rect(offsetX + column * width, offsetY, width, height), $"{upgrade.Name} [{upgrade.CurrentLevel}] +")) {
				upgrade.LevelUp();
				PlayerState.Instance.SetBroomUpgradeLevel(upgrade.Name, upgrade.CurrentLevel);
				broom.UpdateState();
			}
			column++;
		}
#endif
	}
}
