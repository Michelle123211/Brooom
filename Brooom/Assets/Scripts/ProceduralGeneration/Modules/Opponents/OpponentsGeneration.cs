using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentsGeneration : LevelGeneratorModule {

	[Tooltip("Number of opponents to generate.")]
	public int opponentsCount = 5;
	[Tooltip("Space between the racers on the start line.")]
	public float spacing = 2;
	[Tooltip("Prefab of the opponent.")]
	public OpponentRandomization opponentPrefab;
	[Tooltip("An object which will be parent of all opponent objects in the hierarchy.")]
	public Transform opponentsParent;

	public override void Generate(LevelRepresentation level) {
		for (int i = 0; i < opponentsCount; i++) {
			// Compute the position on the start line (given by the index) - if the opponent count is odd, there is one more to the left of the player
			int offset = i - ((opponentsCount + 1) / 2);
			if (offset >= 0) offset += 1;
			Vector3 position = level.playerStartPosition + offset * Vector3.right * spacing;
			// Instantiate under the common parent
			OpponentRandomization opponent = Instantiate<OpponentRandomization>(opponentPrefab, position, Quaternion.identity, opponentsParent);
			// Randomize the opponent (appearance, name, broom upgrades, equipped spells, minimap icon color)
			opponent.Randomize();
		}
	}
}
