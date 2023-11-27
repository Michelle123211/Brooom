using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRubberBanding : SkillLevelImplementation {

	[Tooltip("All available skill levels (relatively to player) and their corresponding stats modifications.")]
	[SerializeField] List<SkillLevelParameters> skillLevelParameters;

	[Tooltip("Curve describing stats values modification based on distance from the player. The value determines stats modifier in case of positive distance (e.g. stats multiplied by 0.9f), and mistakes modifier in case of negative distance (e.g. stats increased by the stats complement multiplied by 0.9f).")]
	[SerializeField] AnimationCurve statsModificationBasedOnDistance;

	private PlayerStats baseStatsValues;
	private PlayerStats currentStatsValues;

	public override PlayerStats GetInitialStats(AISkillLevel.RelationToPlayer skillLevelRelativeToPlayer) {
		baseStatsValues = PlayerState.Instance.CurrentStats;
		// Modify the baseStatsValues based on the skillLevelRelativeToPlayer
		if (skillLevelParameters != null) {
			foreach (var level in skillLevelParameters) {
				if (level.skillLevel == skillLevelRelativeToPlayer) {
					ApplySkillLevelStatsModifications(level);
					break;
				}
			}
		}
		currentStatsValues = baseStatsValues;
		// And return
		return baseStatsValues;
	}

	public override PlayerStats GetCurrentStats() {
		// Compute current stats values from rubber banding
		float distanceDifference =
			GetNormalizedDistanceRaced(this.agentRaceState) // difference between agent's...
			- GetNormalizedDistanceRaced(RaceController.Instance.playerRacer.state); // ... and player's distance raced
		AnimationCurve modifierCurve = statsModificationBasedOnDistance; // stats modifier is determined by a curve
		distanceDifference = Mathf.Clamp(distanceDifference, modifierCurve.keys[0].time, modifierCurve.keys[modifierCurve.length - 1].time);
		float modifier = modifierCurve.Evaluate(distanceDifference);
		if (distanceDifference < 0) { // increase stats (= decrease amount of mistakes)
			currentStatsValues = baseStatsValues + baseStatsValues.GetComplement() * modifier;
		} else { // decrease stats (= increase amount of mistakes)
			currentStatsValues = baseStatsValues - baseStatsValues * modifier;
		}
		return currentStatsValues;
	}

	private void ApplySkillLevelStatsModifications(SkillLevelParameters parameters) {
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

	private int GetModifiedStatValue(int initialValue, float percentageChange) {
		if (percentageChange < 0) { // Decreasing the value - subtract percentage of the stat value
			return Mathf.RoundToInt(initialValue - initialValue * percentageChange);
		} else { // Increasing the value - add percentage of the mistakes (stat value complement)
			return Mathf.RoundToInt(initialValue + (100 - initialValue) * percentageChange);
		}
	}

}
