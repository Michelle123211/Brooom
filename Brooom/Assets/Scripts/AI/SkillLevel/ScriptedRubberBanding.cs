using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptedRubberBanding : SkillLevelImplementation {

	[Tooltip("Difference between the actual and desired distance from the player mapped to the stats modifier (fraction of the max stat value added to the initial stat value).")]
	[SerializeField] protected AnimationCurve statsChangeCurve;

	protected PlayerStats baseStatsValues;
	protected PlayerStats currentStatsValues;
	protected PlayerStats maxStatsValues = new PlayerStats {
		endurance = 100,
		speed = 100,
		dexterity = 100,
		precision = 100,
		magic = 100
	};
	protected AnimationCurve desiredDistanceCurve = AnimationCurve.Constant(0, 1, 0);

	private PlayerStats temporaryStatsValues; // stats values used as a reference for tweening the values in the last segment of race

	public override PlayerStats GetCurrentStats() {
		float raceFraction = RaceController.Instance.IsInitialized ? GetNormalizedDistanceRaced(agentRaceState) : 0f;
		// Maximum stats in the first 5 % of the race
		if (raceFraction < 0.05f) {
			currentStatsValues = maxStatsValues;
			temporaryStatsValues = currentStatsValues;
		}
		// Converge to the initial values in the last 10 % of the race
		else if (raceFraction > 0.9f) {
			float modifier = (raceFraction - 0.9f) * 10f;
			if (baseStatsValues.speed > temporaryStatsValues.speed) // stats are clamped between 0 and 100 so we need to multiply by a positive number and then either add or subtract
				currentStatsValues = temporaryStatsValues + (modifier * (baseStatsValues - temporaryStatsValues));
			else
				currentStatsValues = temporaryStatsValues - (modifier * (temporaryStatsValues - baseStatsValues));
		}
		// Otherwise adjust stats according to the distance from the player
		else {
			// Get desired distance from the player
			float desiredDistance = desiredDistanceCurve.Evaluate(raceFraction);
			// Compare it to the actual distance
			float actualDistance = GetDistanceBetweenAgentAndPlayer();
			float difference = actualDistance - desiredDistance;
			difference = Mathf.Clamp(difference, statsChangeCurve.keys[0].time, statsChangeCurve.keys[statsChangeCurve.length - 1].time);
			// Apply modifications to the stats values accordingly
			float modifier = statsChangeCurve.Evaluate(difference);
			if (modifier >= 0) currentStatsValues = baseStatsValues + maxStatsValues * modifier;
			else currentStatsValues = baseStatsValues - maxStatsValues * Mathf.Abs(modifier); // stats are clamped between 0 and 100 so we need to multiply by a positive number and then subtract
			temporaryStatsValues = currentStatsValues;
		}
		return currentStatsValues;
	}
}
