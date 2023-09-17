using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentsGeneration : LevelGeneratorModule {

	[Tooltip("Prefab of the opponent.")]
	public OpponentRandomization opponentPrefab;
	[Tooltip("Number of opponents to generate.")]
	public int opponentsCount = 5;
	[Tooltip("An object which will be parent of all opponent objects in the hierarchy.")]
	public Transform opponentsParent;

	public override void Generate(LevelRepresentation level) {
		for (int i = 0; i < opponentsCount; i++) {
			// TODO: Instantiate under the common parent
			// TODO: Place on the start line (to a position given by the index)
			// TODO: Randomize the opponent (appearance, name, broom upgrades, equipped spells, minimap icon color)
			// opponent.Randomize();
		}
	}
}
