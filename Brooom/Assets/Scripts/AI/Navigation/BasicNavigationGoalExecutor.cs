using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNavigationGoalExecutor : NavigationGoalExecutor {

	[Header("Skill-based mistakes")]
	[Tooltip("If the agent should make a mistake of not changing the direction to a new goal, the original direction will be kept for this amount of seconds.")]
	[SerializeField] float keepDirectionDuration = 0.7f;

	private AISkillLevel agentSkillLevel;

	protected override Vector3 DetermineTargetPositionFromGoal(NavigationGoal goal) {
		// TODO: Take into consideration errors depending on the goal type

		// Determine if any mistakes occur
		goal.DetermineIfShouldFail(); // If the goal should be failed, its target position is adjusted accordingly
		if (agentSkillLevel == null)
			agentSkillLevel = agent.GetComponentInChildren<AISkillLevel>();
		if (Random.value < agentSkillLevel.GetDexterityMistakeProbability()) { // agent should continue in the same direction (according to the dexterity mistake)
			 // Set target position far in front of the agent
			StartCoroutine(SetTargetPositionAfterKeepingDirection(goal));
			return (agent.transform.position + agent.transform.forward * 1000f);
		}
		return goal.TargetPosition;
	}

	private IEnumerator SetTargetPositionAfterKeepingDirection(NavigationGoal goal) {
		// Wait for a while
		yield return new WaitForSeconds(keepDirectionDuration);
		// Set the correct target position
		if (goal.IsSameAs(currentGoal)) // the goal has not been changed in the meantime
			steering.SetTargetPosition(DetermineTargetPositionFromGoal(goal));
	}
}
