using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class determining agents' stats values to affect their performance during race.
/// Stats values are initialized based on predefined modifiers of current player stats for each skill level.
/// Then they stay the same during the whole race duration.
/// </summary>
public class FixedCloseToPlayerSkillLevel : SkillLevelImplementation {

	[Tooltip("All available skill levels (relatively to player) and their corresponding stats modifications.")]
	[SerializeField] List<SkillLevelModifiers> skillLevelParameters;

	private PlayerStats currentStatsValues;

	/// <inheritdoc/>
	public override PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType) {
		currentStatsValues = PlayerState.Instance.CurrentStats;
		// Modify the currentStatsValues based on the skillLevelType
		if (skillLevelParameters != null) {
			foreach (var level in skillLevelParameters) {
				if (level.skillLevel == skillLevelType) {
					ApplySkillLevelStatsModifications(level);
					break;
				}
			}
		}
		// And return
		return currentStatsValues;
	}

	/// <inheritdoc/>
	public override PlayerStats GetCurrentStats() {
		return currentStatsValues;
	}

	private void ApplySkillLevelStatsModifications(SkillLevelModifiers parameters) {
		// Modify the stats according to the parameters
		// ... speed
		currentStatsValues.speed = GetModifiedStatValue(currentStatsValues.speed, parameters.speedChange);
		// ... dexterity
		currentStatsValues.dexterity = GetModifiedStatValue(currentStatsValues.dexterity, parameters.dexterityChange);
		// ... precision
		currentStatsValues.precision = GetModifiedStatValue(currentStatsValues.precision, parameters.precisionChange);
		// ... magic
		currentStatsValues.magic = GetModifiedStatValue(currentStatsValues.magic, parameters.magicChange);
	}

}
