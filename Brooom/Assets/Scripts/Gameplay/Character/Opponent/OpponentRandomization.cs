using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// A class providing a method for opponent randomization, setting name, appearance, broom upgrades levels and equipped spells.
/// </summary>
public class OpponentRandomization : MonoBehaviour {

	/// <summary>
	/// Randomizes opponent before a race by selecting a random name, appearance, broom upgrade levels and equipped spells.
	/// Also sets this opponent's minimap icon color.
	/// </summary>
	/// <param name="assignedColor">This opponent's minimap icon color.</param>
	public void Randomize(Color assignedColor) {
		// Choose a random appearance along with a random name
		GetComponentInChildren<CharacterAppearance>().RandomizeCharacterCustomization();
		// Choose random levels of broom upgrades
		GetComponentInChildren<Broom>().RandomizeStateCloseToPlayer();
		// Choose random equipped spells (only from those unlocked for player + one still locked)
		GetComponent<SpellController>().RandomizeEquippedSpells();
		// Choose and set a minimap icon color
		GetComponent<CharacterRaceState>().assignedColor = assignedColor;
		GetComponentInChildren<SpriteRenderer>().color = assignedColor;
	}

}
