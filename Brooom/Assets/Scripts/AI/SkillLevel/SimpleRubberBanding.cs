using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class determining agents' stats values to affect their performance during race.
/// Stats values are initialized based on predefined modifiers of current player stats for each skill level.
/// Then, during the race, they are further modified based on the agent's position relatively to the player.
/// </summary>
public class SimpleRubberBanding : SkillLevelImplementation {

	[Tooltip("All available skill levels (relatively to player) and their corresponding stats modifications.")]
	[SerializeField] List<SkillLevelModifiers> skillLevelParameters;

	[Tooltip("Curve describing stats values modification based on distance from the player. The value determines stats modifier in case of positive distance (e.g. stats multiplied by 0.9f), and mistakes modifier in case of negative distance (e.g. stats increased by the stats complement multiplied by 0.9f).")]
	[SerializeField] AnimationCurve statsModificationBasedOnDistance;

	private PlayerStats baseStatsValues;
	private PlayerStats currentStatsValues;

	/// <inheritdoc/>
	public override PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType) {
		baseStatsValues = PlayerState.Instance.CurrentStats;
		// Modify the baseStatsValues based on the skillLevelType
		if (skillLevelParameters != null) {
			foreach (var level in skillLevelParameters) {
				if (level.skillLevel == skillLevelType) {
					ApplySkillLevelStatsModifications(level);
					break;
				}
			}
		}
		currentStatsValues = baseStatsValues;
		// And return
		return baseStatsValues;
	}

	/// <inheritdoc/>
	public override PlayerStats GetCurrentStats() {
		// Compute current stats values from rubber banding
		float distanceDifference = GetDistanceBetweenAgentAndPlayer();
		AnimationCurve modifierCurve = statsModificationBasedOnDistance; // stats modifier is determined by a curve (describing multiplier based on distance)
		distanceDifference = Mathf.Clamp(distanceDifference, modifierCurve.keys[0].time, modifierCurve.keys[modifierCurve.length - 1].time);
		float modifier = modifierCurve.Evaluate(distanceDifference);
		if (distanceDifference < 0) { // increase stats (= decrease amount of mistakes)
			currentStatsValues = baseStatsValues + baseStatsValues.GetComplement() * modifier;
		} else { // decrease stats (= increase amount of mistakes)
			currentStatsValues = baseStatsValues - baseStatsValues * modifier;
		}
		return currentStatsValues;
	}

	private void ApplySkillLevelStatsModifications(SkillLevelModifiers parameters) {
		// Modify the stats according to the parameters
		// ... speed
		baseStatsValues.speed = GetModifiedStatValue(baseStatsValues.speed, parameters.speedChange);
		// ... dexterity
		baseStatsValues.dexterity = GetModifiedStatValue(baseStatsValues.dexterity, parameters.dexterityChange);
		// ... precision
		baseStatsValues.precision = GetModifiedStatValue(baseStatsValues.precision, parameters.precisionChange);
		// ... magic
		baseStatsValues.magic = GetModifiedStatValue(baseStatsValues.magic, parameters.magicChange);
	}

}
