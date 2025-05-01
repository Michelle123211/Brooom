using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class used when persistently storing the last settings used for Quick Race.
/// </summary>
[System.Serializable]
public class QuickRaceSaveData {
	// Initialize everything to default values

	/// <summary>Difficulty level chosen during the last session (<c>-1</c> if not initialized).</summary>
	public int difficultyLevel = -1;
	/// <summary>Whether spells were enabled in the last session (<c>0</c> if disabled, <c>1</c> if enabled, <c>-1</c> if not initialized). Stored as <c>int</c>, because nullable bool is not serializable.</summary>
	public int enableSpells = -1;
	/// <summary>Player stats used in the last session (individual stats are <c>-1</c> if not initialized).</summary>
	public PlayerStats stats = new PlayerStats { dexterity = -1, endurance = -1, magic = -1, precision = -1, speed = -1 };
	/// <summary>Broom upgrades levels used in the last session (<c>null</c> if not initialized).</summary>
	public BroomUpgradesSaveData broomUpgrades = null;
	/// <summary>Spells equipped in the last session (<c>null</c> if not initialized).</summary>
	public SpellsSaveData spells = null;

}
