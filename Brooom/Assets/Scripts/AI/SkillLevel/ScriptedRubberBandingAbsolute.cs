using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class determining agents' stats values to affect their performance during race in a way that they will try to stay at a certain distance from the player.
/// It implements rubber banding using race scripts with predefined initial stats values for each skill level (not derived from the player's stats values).
/// Then during the race, stats are modified based on the agent's position relatively to a position relative to the player.
/// </summary>
public class ScriptedRubberBandingAbsolute : ScriptedRubberBanding {

	[Tooltip("All available skill levels and their corresponding race scripts (mapping fraction of the race to distance from the player).")]
	[SerializeField] List<SkillLevelRaceScriptAbsolute> skillLevelParameters;

	/// <inheritdoc/>
	public override PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType) {
		baseStatsValues = maxStatsValues;
		// Modify the baseStatsValues based on the skillLevelType - a fraction of maximum stats values
		if (skillLevelParameters != null) {
			foreach (var level in skillLevelParameters) {
				if (level.skillLevel == skillLevelType) {
					baseStatsValues = baseStatsValues * level.statsModifier;
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

/// <summary>
/// A class containing parameters for a scripted rubber banding at a certain skill level (i.e., initial stats values, desired distance from the player throughout the race).
/// Stats values are initialized absolutely as a fraction of the maximum values.
/// </summary>
[System.Serializable]
internal class SkillLevelRaceScriptAbsolute {
    [Tooltip("Skill level associated to the following race script.")]
    public AISkillLevel.SkillType skillLevel;

    [Tooltip("Curve describing in what distance from player the agent tries to stay throughout the race (fraction of the track the player reached mapped to distance). Positive distance means staying in front of the player.")]
    public AnimationCurve distanceFromPlayer;

	[Tooltip("Initial values of the stats (between 0 and 1 as a fraction of the maximum values).")]
	[Range(0, 1)]
	public float statsModifier;
}