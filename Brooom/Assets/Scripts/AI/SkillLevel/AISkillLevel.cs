using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Contains all parameters and computations related to mistakes made by AI agents
/// <summary>
/// A class containing all parameters and computations related to mistakes made by AI agents.
/// It uses <c>AISkillBasedMistakesParameters</c> to get amount of different kinds of mistakes the agents may make based on their skill level, 
/// and also <c>SkillLevelImplementation</c> implementation to compute stats values of the agent based on their skill level.
/// </summary>
public class AISkillLevel : MonoBehaviour {

	/// <summary>
	/// All possible agents' skill types (describing only their overall ability but not exact stats values).
	/// </summary>
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

	[Tooltip("Whether debug messages should be logged.")]
	[SerializeField] bool debugLogs = false;

	// Currently used stats values (derived from the initial values based on distance to the player - rubber banding)
	private PlayerStats currentStatsValues;

	/// <summary>
	/// Initialize everything based on the given skill type (i.e. skill level relative to player).
	/// </summary>
	/// <param name="skillLevelRelativeToPlayer">Skill type assigned to the agent.</param>
	public void Initialize(SkillType skillLevelRelativeToPlayer) {
		skillLevelImplementation.Initialize(transform.parent.GetComponent<CharacterRaceState>(), transform.parent.GetComponent<CharacterMovementController>());
		currentStatsValues = skillLevelImplementation.GetInitialStats(skillLevelRelativeToPlayer);
		if (debugLogs)
			Debug.Log($"AISkillLevel.Initialize(): Stats are initialized to {currentStatsValues}");
	}

	/// <summary>
	/// Returns probability that the agent makes a mistake relevant for the Speed stat (e.g., slow down, skip speed bonus.
	/// </summary>
	/// <returns>Probability that the agent makes speed-related mistake.</returns>
	public float GetSpeedMistakeProbability() {
		// Complement of the speed value mapped between 0 and 1
		return (100 - currentStatsValues.speed) / 100f;
	}

	/// <summary>
	/// Returns probability that the agent makes a mistake relevant for the Dexterity stat (e.g., not change direction, collide with obstacles, miss bonus/hoop).
	/// </summary>
	/// <returns>Probability that the agent makes dexterity-related mistake.</returns>
	public float GetDexterityMistakeProbability() {
		// Complement of the dexterity value mapped between 0 and 1
		return (100 - currentStatsValues.dexterity) / 100f;
	}

	/// <summary>
	/// Returns probability that the agent makes a mistake relevant for the Precision stat (e.g., skip bonus/hoop, collide with obstacles).
	/// </summary>
	/// <returns>Probability that the agent makes precision-related mistake.</returns>
	public float GetPrecisionMistakeProbability() {
		// Complement of the precision value mapped between 0 and 1
		return (100 - currentStatsValues.precision) / 100f;
	}

	/// <summary>
	/// Returns probability that the agent makes a mistake relevant for the Magic stat (e.g., skip mana bonus, prolonged delay before cast, not cast diverse spells).
	/// </summary>
	/// <returns>Probability that the agent makes magic-related mistake.</returns>
	public float GetMagicMistakeProbability() {
		// Complement of the magic value mapped between 0 and 1
		return (100 - currentStatsValues.magic) / 100f;
	}

	private void Update() {
		// Rubber banding to adjust to the player...
		// ... difficulty-based (skill-based)
		currentStatsValues = skillLevelImplementation.GetCurrentStats();
		if (debugLogs)
			Debug.Log($"AISkillLevel.Update(): Current stats are {currentStatsValues}");
		// ... power-based
		skillLevelImplementation.AdjustCurrentMaximumSpeed();
	}

}

/// <summary>
/// A class describing change of player's stats which are then used for AI agent of a corresponding skill level.
/// </summary>
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

/// <summary>
/// A class describing percentage of player's stats which are then used for AI agent of a corresponding skill level.
/// </summary>
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
