using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(NavigationSteering))]
public abstract class NavigationGoalExecutor : MonoBehaviour {

	protected GameObject agent;

	protected NavigationSteering steering;

	public void Initialize(GameObject agent) {
		this.agent = agent;
		this.steering = GetComponent<NavigationSteering>();
		this.steering.Initialize(agent);
	}
	public CharacterMovementValues GetCurrentMovementValue() {
		return steering.GetCurrentMovementValue();
	}

	public void SetGoal(NavigationGoal goal) {
		if (goal.Type == NavigationGoalType.None) {
			steering.StopSteering();
		} else {
			goal.DetermineIfShouldFail();
			steering.SetTargetPosition(DetermineTargetPositionFromGoal(goal));
		}
	}

	protected abstract Vector3 DetermineTargetPositionFromGoal(NavigationGoal goal);

}
