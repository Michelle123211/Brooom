using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A level generator module responsible for instantiating randomized opponents on the start line.
/// Each opponent has a random skill level assigned (in such a way that no skill level is used twice),
/// and also other aspects are randomized (appearance, name, broom upgrades, equipped spells).
/// </summary>
public class OpponentsGeneration : LevelGeneratorModule {

	[Tooltip("Number of opponents to generate.")]
	public int opponentsCount = 5;
	[Tooltip("Skill levels of the opponents, each will be chosen at most once. If there are not enough skill levels for the number of opponents, AISkillLevel.SkillType.Average will be chosen by default.")]
	public List<AISkillLevel.SkillType> skillLevels;
	[Tooltip("Space between the racers on the start line.")]
	public float spacing = 2;
	[Tooltip("Prefab of the opponent.")]
	public OpponentRandomization opponentPrefab;
	[Tooltip("An object which will be parent of all opponent objects in the hierarchy.")]
	public Transform opponentsParent;
	[Tooltip("Colors assigned to the opponents which are then used in the minimap.")]
	public Color[] opponentColors;


	/// <summary>
	/// Instantiates opponents on the start line, each with randomized parameters such as skill level, appearance, name, broom upgrades and equipped spells.
	/// </summary>
	/// <param name="level"><inheritdoc/></param>
	public override void Generate(LevelRepresentation level) {
		// Remove any previously instantiated opponents
		UtilsMonoBehaviour.RemoveAllChildren(opponentsParent);
		// Prepare list of skill levels to choose from
		List<AISkillLevel.SkillType> remainingSkillLevels = new();
		foreach (var skillLevel in skillLevels) remainingSkillLevels.Add(skillLevel);
		// Instantiate and place the opponents
		for (int i = 0; i < opponentsCount; i++) {
			// Compute the position on the start line (given by the index) - if the opponent count is odd, there is one more to the left of the player
			int offset = i - ((opponentsCount + 1) / 2);
			if (offset >= 0) offset += 1;
			Vector3 position = level.playerStartPosition + offset * spacing * Vector3.right;
			// Instantiate under the common parent
			OpponentRandomization opponent = Instantiate<OpponentRandomization>(opponentPrefab, position, Quaternion.identity, opponentsParent);
			// Determine opponent's skill level
			if (remainingSkillLevels.Count > 0) {
				int skillLevelIndex = Random.Range(0, remainingSkillLevels.Count);
				opponent.GetComponentInChildren<AISkillLevel>().Initialize(remainingSkillLevels[skillLevelIndex]);
				remainingSkillLevels.RemoveAt(skillLevelIndex);
			} else opponent.GetComponentInChildren<AISkillLevel>().Initialize(AISkillLevel.SkillType.Average);
			// Randomize the opponent (appearance, name, broom upgrades, equipped spells, minimap icon color)
			opponent.Randomize(opponentColors[i % opponentColors.Length]);
		}
	}

}
