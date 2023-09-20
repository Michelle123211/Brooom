using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentRandomization : MonoBehaviour
{
    public void Randomize() {
		// Choose a random appearance along with a random name
		GetComponentInChildren<CharacterAppearance>()?.RandomizeCharacterCustomization();
		// Choose random levels of broom upgrades
		GetComponentInChildren<Broom>()?.RandomizeStateCloseToPlayer();
		// TODO: Choose random equipped spells (only if player has unlocked at least one spell, similar in value and similar number)
		// TODO: Choose and set a minimap icon color
	}
}
