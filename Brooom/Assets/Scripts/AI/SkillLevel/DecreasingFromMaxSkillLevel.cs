using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class determining agents' stats values to affect their performance during race.
/// It starts with maximum stats values, but then decreases them during the race if the agent is in front of the player.
/// </summary>
public class DecreasingFromMaxSkillLevel : SkillLevelImplementation {

	[Tooltip("Curve describing stats values modification (fraction of maximum values) based on distance from the player. Positive distance means the agent is in front of the player.")]
	[SerializeField] AnimationCurve statsModificationBasedOnDistance;

	private PlayerStats baseStatsValues;
	private PlayerStats currentStatsValues;

	/// <inheritdoc/>
	public override PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType) {
		// Start on maximum
		baseStatsValues = new PlayerStats {
			endurance = 100,
			speed = 100,
			dexterity = 100,
			precision = 100,
			magic = 100
		};
		currentStatsValues = baseStatsValues;
		return currentStatsValues;
	}

	/// <inheritdoc/>
	public override PlayerStats GetCurrentStats() {
		float distanceDifference = GetDistanceBetweenAgentAndPlayer();
		// Stats modifier is determined by a curve
		AnimationCurve modifierCurve = statsModificationBasedOnDistance;
		// If the agent is in front of the player, decrease stats accordingly, otherwise keep them at maximum
		distanceDifference = Mathf.Clamp(distanceDifference, modifierCurve.keys[0].time, modifierCurve.keys[modifierCurve.length - 1].time);
		float modifier = modifierCurve.Evaluate(distanceDifference);
		currentStatsValues = baseStatsValues * modifier;
		return currentStatsValues;
	}

}
