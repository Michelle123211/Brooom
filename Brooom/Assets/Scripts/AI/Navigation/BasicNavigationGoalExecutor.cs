using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Opponents' navigation component responsible for executing actions leading to fulfilling the current navigation goal.
/// Also considers mistakes made based on agent's skill level, i.e. not changing direction for a while after reaching a goal (to mimick not being able to decide quickly on the next goal).
/// </summary>
public class BasicNavigationGoalExecutor : NavigationGoalExecutor {

	private AISkillLevel agentSkillLevel;

	/// <inheritdoc/>
	protected override Vector3 DetermineTargetPositionFromGoal(NavigationGoal goal) {
		// Determine if any mistakes occur
		goal.DetermineIfShouldFail(); // If the goal should be failed, its target position is adjusted accordingly
		if (agentSkillLevel == null)
			agentSkillLevel = agent.GetComponentInChildren<AISkillLevel>();
		// Make sure to set the correct target position after keeping the same direction for a short while
		StartCoroutine(SetTargetPositionAfterKeepingDirection(goal));
		// Return target position far in front of the agent for now (it will be then corrected)
		return (agent.transform.position + agent.transform.forward * 1000f);
	}

	private IEnumerator SetTargetPositionAfterKeepingDirection(NavigationGoal goal) {
		// Wait for a while (how long the agent should keep going in the original direction)
		float keepDirectionDuration = agentSkillLevel.mistakesParameters.KeepDirectionDurationCurve.Evaluate(agentSkillLevel.GetDexterityMistakeProbability());
		yield return new WaitForSeconds(keepDirectionDuration);
		// Set the correct target position
		if (goal.IsSameAs(currentGoal)) // the goal has not been changed in the meantime
			steering.SetTargetPosition(goal.TargetPosition);
	}
}
