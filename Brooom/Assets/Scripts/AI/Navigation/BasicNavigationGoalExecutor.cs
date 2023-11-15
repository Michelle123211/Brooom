using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNavigationGoalExecutor : NavigationGoalExecutor {

	[Header("Skill-based mistakes")]
	[Tooltip("The curve describes mapping from dexterity mistake probability to duration of not changing the direction to a new goal (the original direction is kept for this amount of seconds).")]
	[SerializeField] AnimationCurve keepDirectionDurationCurve;

	private AISkillLevel agentSkillLevel;

	protected override Vector3 DetermineTargetPositionFromGoal(NavigationGoal goal) {
		// Determine if any mistakes occur
		goal.DetermineIfShouldFail(); // If the goal should be failed, its target position is adjusted accordingly
		if (agentSkillLevel == null)
			agentSkillLevel = agent.GetComponentInChildren<AISkillLevel>();
		// Make sure to set the correct target position after keeping the same direction for a short while
		StartCoroutine(SetTargetPositionAfterKeepingDirection(goal));
		// Return target position far in front of the agent (it will be then corrected 
		return (agent.transform.position + agent.transform.forward * 1000f);
	}

	private IEnumerator SetTargetPositionAfterKeepingDirection(NavigationGoal goal) {
		// Wait for a while
		float keepDirectionDuration = keepDirectionDurationCurve.Evaluate(agentSkillLevel.GetDexterityMistakeProbability());
		yield return new WaitForSeconds(keepDirectionDuration);
		// Set the correct target position
		if (goal.IsSameAs(currentGoal)) // the goal has not been changed in the meantime
			steering.SetTargetPosition(goal.TargetPosition);
	}
}
