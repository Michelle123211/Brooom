using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentRandomization : MonoBehaviour
{

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
