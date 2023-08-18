using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentUpgradeButtons : MonoBehaviour
{
	public Broom broom;
	BroomUpgrade[] upgrades;

	private int width = 100;
	private int height = 30;
	private int offsetX = 10;
	private int offsetY = 10;

	private void Start() {
		upgrades = broom.GetAvailableUpgrades();
	}

	private void Update() {
		KeyCode[] keys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };
		for (int i = 0; i < upgrades.Length; i++) {
			if (Input.GetKeyDown(keys[i])) {
				upgrades[i].LevelUp();
				broom.UpdateState();
			}
		}
	}

	private void OnGUI() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		GUI.skin.button.fontSize = 16;

		int column = 0;
		foreach (var upgrade in upgrades) { // speed, control, elevation
			GUI.Label(new Rect(offsetX + column * width, offsetY, width, height), $"{upgrade.UpgradeName} [{upgrade.CurrentLevel}]: {column + 1}");
			//if (GUI.Button(new Rect(offsetX + column * width, offsetY, width, height), $"{upgrade.Name} [{upgrade.CurrentLevel}] +")) {
			//	upgrade.LevelUp();
			//	PlayerState.Instance.SetBroomUpgradeLevel(upgrade.Name, upgrade.CurrentLevel);
			//	broom.UpdateState();
			//}
			column++;
		}
#endif
	}
}
