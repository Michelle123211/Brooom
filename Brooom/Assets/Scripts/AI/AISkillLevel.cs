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
		// TODO: Compute currentStatsValues from rubber banding
		float distance = Vector3.Distance(transform.position.WithY(0), RaceController.Instance.playerRacer.characterController.transform.position.WithY(0));
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
