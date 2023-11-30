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

	public override PlayerStats GetCurrentStats() {
		float raceFraction = GetNormalizedDistanceRaced(RaceController.Instance.playerRacer.state);
		// Maximum stats in the first 5 % of the race
		if (raceFraction < 0.05f) {
			currentStatsValues = maxStatsValues;
		}
		// Converge to the initial values in the last 20 % of the race
		else if (raceFraction > 0.8f) {
			currentStatsValues = ((currentStatsValues / 5) * 4) + (baseStatsValues / 5);
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
			currentStatsValues = baseStatsValues + maxStatsValues * statsChangeCurve.Evaluate(difference);
		}
		return currentStatsValues;
	}
}
