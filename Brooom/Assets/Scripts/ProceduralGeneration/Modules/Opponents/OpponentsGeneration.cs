using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentsGeneration : LevelGeneratorModule {

	[Tooltip("Prefab of the opponent.")]
	public GameObject opponentPrefab;
	[Tooltip("Number of opponents to generate.")]
	public int opponentsCount = 5;
	[Tooltip("An object which will be parent of all opponent objects in the hierarchy.")]
	public Transform opponentsParent;

	public override void Generate(LevelRepresentation level) {
		for (int i = 0; i < opponentsCount; i++) { 
			// TODO: Instantiate under the common parent
			// TODO: Place on the start line (to a position given by the index)
			// TODO: Choose a random appearance (randomization similar to the character customization scene, then set in the CharacterAppearance component)
			// ... along with a random name
			// TODO: Choose a random broom upgrades (the same altitude as the player, the same total number of upgrades +/- 1 as the player)
			// TODO: Choose random equipped spells (inly if player has unlocked at least one spell, similar in value and similar number)
			// TODO: Choose and set a minimap icon color
		}
	}
}
