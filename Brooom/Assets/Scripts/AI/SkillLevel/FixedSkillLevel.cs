using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedSkillLevel : SkillLevelImplementation {

	private PlayerStats currentStatsValues;

	[Tooltip("All available skill levels and their corresponding stats values (percentage of the max value, between 0 and 1).")]
	[SerializeField] List<SkillLevelPercentages> skillLevelParameters;

	public override PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType) {
		currentStatsValues = PlayerState.Instance.CurrentStats;
		// Set stats values based on the skillLevelType
		if (skillLevelParameters != null) {
			foreach (var level in skillLevelParameters) {
				if (level.skillLevel == skillLevelType) {
					ApplySkillLevelStatsValues(level);
					break;
				}
			}
		}
		// And return
		return currentStatsValues;
	}

	public override PlayerStats GetCurrentStats() {
		return currentStatsValues;
	}

	private void ApplySkillLevelStatsValues(SkillLevelPercentages parameters) {
		// Modify the stats according to the parametersS
		// ... speed
		currentStatsValues.speed = GetModifiedStatValue(100, parameters.speedChange);
		// ... dexterity
		currentStatsValues.dexterity = GetModifiedStatValue(100, parameters.dexterityChange);
		// ... precision
		currentStatsValues.precision = GetModifiedStatValue(100, parameters.precisionChange);
		// ... magic
		currentStatsValues.magic = GetModifiedStatValue(100, parameters.magicChange);
	}

}
