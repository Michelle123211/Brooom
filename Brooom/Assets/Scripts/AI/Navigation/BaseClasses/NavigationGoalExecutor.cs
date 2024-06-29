using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SkillBasedNavigationSteering))]
public abstract class NavigationGoalExecutor : MonoBehaviour {

	protected GameObject agent;

	protected NavigationSteering steering;

	protected NavigationGoal currentGoal;

	public void Initialize(GameObject agent) {
		this.agent = agent;
		this.steering = GetComponent<NavigationSteering>();
		this.steering.Initialize(agent);
	}
	public CharacterMovementValues GetCurrentMovementValue() {
		return steering.GetCurrentMovementValue();
	}

	public void SetGoal(NavigationGoal goal) {
		currentGoal = goal;
		if (goal.Type == NavigationGoalType.None) {
			steering.StopSteering();
		} else {
			steering.SetTargetPosition(DetermineTargetPositionFromGoal(goal));
		}
	}

	protected abstract Vector3 DetermineTargetPositionFromGoal(NavigationGoal goal);

}
