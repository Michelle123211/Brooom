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

	// Initial values which determine the agent's skill level
	private PlayerStats baseStatsValues;
	// Currently used values (derived from the initial values based on distance to the player - rubber banding)
	private PlayerStats currentStatsValues;


	public void Initialize(RelationToPlayer skillLevelRelativeToPlayer) {
		baseStatsValues = PlayerState.Instance.CurrentStats;
		// TODO: Modify the baseStatsValues based on the skillLevelRelativeToPlayer
		currentStatsValues = baseStatsValues;
	}

	// Returns probability that the agent makes a mistake relevant for the Speed stat (e.g. slow down, skip speed bonus)
	public float GetSpeedMistakeProbability() {
		// TODO: Implement
		return 0;
	}

	// Returns probability that the agent makes a mistake relevant for the Dexterity stat (e.g. not change direction, collide with obstacles, miss bonus/hoop)
	public float GetDexterityMistakeProbability() {
		// TODO: Implement
		return 0;
	}

	// Returns probability that the agent makes a mistake relevant for the Precision stat (e.g. skip bonus/hoop, collide with obstacles)
	public float GetPrecisionMistakeProbability() {
		// TODO: Implement
		return 0;
	}

	// Returns probability that the agent makes a mistake relevant for the Magic stat (e.g. skip mana bonus, prolonged dealy before cast, not cast diverse spells)
	public float GetMagicMistakeProbability() {
		// TODO: Implement
		return 0;
	}

	private void Update() {
		// TODO: Compute currentStatsValues from rubber banding
		float distance = Vector3.Distance(transform.position.WithY(0), RaceController.Instance.playerRacer.characterController.transform.position.WithY(0));
	}

}
