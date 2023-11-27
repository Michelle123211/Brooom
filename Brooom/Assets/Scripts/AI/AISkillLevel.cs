using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Contains all parameters and computations related to mistakes made by AI agents
public class AISkillLevel : MonoBehaviour {

	public enum RelationToPlayer {
		Better,
		SlightlyBetter,
		RoughlyTheSame,
		SlightlyWorse,
		Worse
	}

	[Tooltip("Parameters of different kinds of mistakes the agents may make.")]
	public AISkillBasedMistakesParameters mistakesParameters;

	[Tooltip("All available skill levels (relatively to player) and their corresponding stats modifications.")]
	[SerializeField] List<SkillLevelParameters> skillLevelParameters;

	// Initial values which determine the agent's skill level
	private PlayerStats baseStatsValues;
	// Currently used values (derived from the initial values based on distance to the player - rubber banding)
	private PlayerStats currentStatsValues;

	private float[] trackPointDistanceSum;


	public void Initialize(RelationToPlayer skillLevelRelativeToPlayer) {
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
	}

	// Returns probability that the agent makes a mistake relevant for the Speed stat (e.g. slow down, skip speed bonus)
	public float GetSpeedMistakeProbability() {
		// Complement of the speed value mapped between 0 and 1
		return (100 - currentStatsValues.speed) / 100f;
	}

	// Returns probability that the agent makes a mistake relevant for the Dexterity stat (e.g. not change direction, collide with obstacles, miss bonus/hoop)
	public float GetDexterityMistakeProbability() {
		// Complement of the dexterity value mapped between 0 and 1
		return (100 - currentStatsValues.dexterity) / 100f;
	}

	// Returns probability that the agent makes a mistake relevant for the Precision stat (e.g. skip bonus/hoop, collide with obstacles)
	public float GetPrecisionMistakeProbability() {
		// Complement of the precision value mapped between 0 and 1
		return (100 - currentStatsValues.precision) / 100f;
	}

	// Returns probability that the agent makes a mistake relevant for the Magic stat (e.g. skip mana bonus, prolonged dealy before cast, not cast diverse spells)
	public float GetMagicMistakeProbability() {
		// Complement of the magic value mapped between 0 and 1
		return (100 - currentStatsValues.magic) / 100f;
	}

	private void ApplySkillLevelStatsModifications(SkillLevelParameters parameters) {
		// Modify the baseStatsValues according to the parameters
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

	private void Update() {
		// Precompute sums of track point distances
		if (trackPointDistanceSum == null) PrecomputeTrackPointDistanceSums();
		// Compute currentStatsValues from rubber banding
		float distanceDifference = 
			GetNormalizedDistanceRaced(transform.parent.GetComponent<CharacterRaceState>()) // difference between agent's...
			- GetNormalizedDistanceRaced(RaceController.Instance.playerRacer.state); // ... and player's distance raced
		Debug.Log($"Difference: {distanceDifference}");
		AnimationCurve modifierCurve = mistakesParameters.skillLevelBasedOnDistance; // stats modifier is determined by a curve
		distanceDifference = Mathf.Clamp(distanceDifference, modifierCurve.keys[0].time, modifierCurve.keys[modifierCurve.length - 1].time);
		float modifier = modifierCurve.Evaluate(distanceDifference);
		Debug.Log($"Modifier: {modifier}");
		Debug.Log($"Base values: {baseStatsValues}");
		if (distanceDifference < 0) { // increase stats (= decrease amount of mistakes)
			currentStatsValues = baseStatsValues + baseStatsValues.GetComplement() * modifier;
		} else { // decrease stats (= increase amount of mistakes)
			currentStatsValues = baseStatsValues - baseStatsValues * modifier;
		}
		Debug.Log($"Current values: {currentStatsValues}");
		Debug.Log("-----------------------");
	}

	private void PrecomputeTrackPointDistanceSums() {
		trackPointDistanceSum = new float[RaceController.Instance.level.track.Count + 1];
		for (int i = 0; i < trackPointDistanceSum.Length; i++) {
			if (i == 0) // from start to the first hoop
				trackPointDistanceSum[i] = Vector3.Distance(RaceController.Instance.level.playerStartPosition, RaceController.Instance.level.track[i].position);
			else {
				if (i == RaceController.Instance.level.track.Count) // from the last hoop to finish
					trackPointDistanceSum[i] = Vector3.Distance(RaceController.Instance.level.finish.transform.position, RaceController.Instance.level.track[i - 1].position);
				else // from a hoop to another hoop
					trackPointDistanceSum[i] = Vector3.Distance(RaceController.Instance.level.track[i - 1].position, RaceController.Instance.level.track[i].position);
				trackPointDistanceSum[i] += trackPointDistanceSum[i - 1];
			}
		}
	}

	private float GetNormalizedDistanceRaced(CharacterRaceState raceState) {
		float distanceRaced;
		// Sum up distances between all track points reached (+ the following one)
		distanceRaced = trackPointDistanceSum[raceState.followingTrackPoint];
		// Subtract distance between the agent and the following track point
		if (raceState.followingTrackPoint == RaceController.Instance.level.track.Count) {
			distanceRaced -= Vector3.Distance(raceState.transform.position, RaceController.Instance.level.finish.transform.position);
		} else {
			distanceRaced -= Vector3.Distance(raceState.transform.position, RaceController.Instance.level.track[raceState.followingTrackPoint].position);
		}
		return distanceRaced;
	}

}

[System.Serializable]
internal class SkillLevelParameters {
	[Tooltip("Skill level in relation to the player for which the following changes are applied.")]
	public AISkillLevel.RelationToPlayer skillLevel;

	[Tooltip("Change of the player's Speed parameter in percents (from -1 to 1). The new value is then used for the AI agent.")]
	[Range(-1, 1)]
	public float speedChange;

	[Tooltip("Change of the player's Dexterity parameter in percents (from -1 to 1). The new value is then used for the AI agent.")]
	[Range(-1, 1)]
	public float dexterityChange;

	[Tooltip("Change of the player's Precision parameter in percents (from -1 to 1). The new value is then used for the AI agent.")]
	[Range(-1, 1)]
	public float precisionChange;

	[Tooltip("Change of the player's Magic parameter in percents (from -1 to 1). The new value is then used for the AI agent.")]
	[Range(-1, 1)]
	public float magicChange;
}
