using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Contains all parameters and computations related to mistakes made by AI agents
public class AISkillLevel : MonoBehaviour {

	public enum SkillType {
		Best,
		Good,
		Average,
		Bad,
		Worst
	}

	[Tooltip("Parameters of different kinds of mistakes the agents may make.")]
	public AISkillBasedMistakesParameters mistakesParameters;

	[Tooltip("Implementation which is used to compute stats values.")]
	[SerializeField] SkillLevelImplementation skillLevelImplementation;

	[SerializeField] bool debugLogs = false;

	// Currently used values (derived from the initial values based on distance to the player - rubber banding)
	private PlayerStats currentStatsValues;


	public void Initialize(SkillType skillLevelRelativeToPlayer) {
		skillLevelImplementation.Initialize(transform.parent.GetComponent<CharacterRaceState>());
		currentStatsValues = skillLevelImplementation.GetInitialStats(skillLevelRelativeToPlayer);
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

	private void Update() {
		currentStatsValues = skillLevelImplementation.GetCurrentStats();
		if (debugLogs)
			Debug.Log($"Current values: {currentStatsValues}");
	}

}

[System.Serializable]
internal class SkillLevelModifiers {
	[Tooltip("Skill level for which the following changes are applied.")]
	public AISkillLevel.SkillType skillLevel;

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

[System.Serializable]
internal class SkillLevelPercentages {
	[Tooltip("Skill level for which the following changes are applied.")]
	public AISkillLevel.SkillType skillLevel;

	[Tooltip("Percentage of the max Speed value (between 0 and 1) which is used for the AI agent.")]
	[Range(0, 1)]
	public float speedChange;

	[Tooltip("Percentage of the max Dexterity value (between 0 and 1) which is used for the AI agent.")]
	[Range(0, 1)]
	public float dexterityChange;

	[Tooltip("Percentage of the max Precision value (between 0 and 1) which is used for the AI agent.")]
	[Range(0, 1)]
	public float precisionChange;

	[Tooltip("Percentage of the max Magic value (between 0 and 1) which is used for the AI agent.")]
	[Range(0, 1)]
	public float magicChange;
}
