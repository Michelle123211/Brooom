using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITestOpponentGeneration : LevelGeneratorModule {

	[Tooltip("Space between the racers on the start line.")]
	public float spacing = 2;
	[Tooltip("Prefabs of the opponents.")]
	public List<OpponentRandomization> opponentPrefabs;
	[Tooltip("An object which will be parent of all opponent objects in the hierarchy.")]
	public Transform opponentsParent;
	[Tooltip("Colors assigned to the opponents which are then used in the minimap.")]
	public Color[] opponentColors;

	public override void Generate(LevelRepresentation level) {
		if (opponentPrefabs == null) return;
		int opponentsCount = opponentPrefabs.Count;
		for (int i = 0; i < opponentsCount; i++) {
			// Compute the position on the start line (given by the index) - if the opponent count is odd, there is one more to the left of the player
			int offset = i - ((opponentsCount + 1) / 2);
			if (offset >= 0) offset += 1;
			Vector3 position = level.playerStartPosition + offset * Vector3.right * spacing;
			// Instantiate under the common parent
			OpponentRandomization opponent = Instantiate<OpponentRandomization>(opponentPrefabs[i], position, Quaternion.identity, opponentsParent);
			// Randomize the opponent (appearance, name, broom upgrades, equipped spells, minimap icon color)
			opponent.Randomize(opponentColors[i % opponentColors.Length]);
		}
	}
}