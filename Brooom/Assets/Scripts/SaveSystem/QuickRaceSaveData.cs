using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// The smallest class describing the last settings used for Quick Race
[System.Serializable]
public class QuickRaceSaveData {
	// Initialize everything to default values
	public int difficultyLevel = -1;
	public int enableSpells = -1; // int, because nullable bool is not serialized and we need to be able to tell value is not initialized
	public PlayerStats stats = new PlayerStats { dexterity = -1, endurance = -1, magic = -1, precision = -1, speed = -1 };
	public BroomUpgradesSaveData broomUpgrades = null;
	public SpellsSaveData spells = null;
}
