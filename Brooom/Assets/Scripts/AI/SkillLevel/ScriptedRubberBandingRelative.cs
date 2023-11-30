using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Rubber banding using race scripts with initial stats values derived from the player's stats values
public class ScriptedRubberBandingRelative : ScriptedRubberBanding {

	[Tooltip("All available skill levels and their corresponding race scripts (mapping fraction of the race to distance from the player).")]
	[SerializeField] List<SkillLevelRaceScriptRelative> skillLevelParameters;

	public override PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType) {
		baseStatsValues = PlayerState.Instance.CurrentStats;
		// Modify the baseStatsValues based on the skillLevelType
		if (skillLevelParameters != null) {
			foreach (var level in skillLevelParameters) {
				if (level.skillLevel == skillLevelType) {
					baseStatsValues.speed = GetModifiedStatValue(baseStatsValues.speed, level.statsModifier);
					baseStatsValues.dexterity = GetModifiedStatValue(baseStatsValues.dexterity, level.statsModifier);
					baseStatsValues.precision = GetModifiedStatValue(baseStatsValues.precision, level.statsModifier);
					baseStatsValues.magic = GetModifiedStatValue(baseStatsValues.magic, level.statsModifier);
					desiredDistanceCurve = level.distanceFromPlayer;
					break;
				}
			}
		}
		currentStatsValues = baseStatsValues;
		// And return
		return baseStatsValues;
	}
}

[System.Serializable]
internal class SkillLevelRaceScriptRelative {
	[Tooltip("Skill level associated to the following race script.")]
	public AISkillLevel.SkillType skillLevel;

	[Tooltip("Curve describing in what distance from player the agent tries to stay throughout the race (fraction of the track the player reached mapped to distance). Positive distance means staying in front of the player.")]
	public AnimationCurve distanceFromPlayer;

	[Tooltip("Initial values of the stats (between -1 and 1 as a relative change of the player's stats values in case of ScriptedRubberBandingRelative).")]
	[Range(-1, 1)]
	public float statsModifier;
}