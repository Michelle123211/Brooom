using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Rubber banding using race scripts with absolute initial stats values (not derived relatively from the player's stats values)
public class ScriptedRubberBandingAbsolute : ScriptedRubberBanding {

	[Tooltip("All available skill levels and their corresponding race scripts (mapping fraction of the race to distance from the player).")]
	[SerializeField] List<SkillLevelRaceScriptAbsolute> skillLevelParameters;

	public override PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType) {
		baseStatsValues = maxStatsValues;
		// Modify the baseStatsValues based on the skillLevelType
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