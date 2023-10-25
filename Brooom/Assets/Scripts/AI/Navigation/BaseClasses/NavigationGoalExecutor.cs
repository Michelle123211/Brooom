using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(NavigationSteering))]
public abstract class NavigationGoalExecutor : MonoBehaviour {

	protected GameObject agent;

	protected NavigationSteering steering;

	public void Initialize(GameObject agent) {
		this.agent = agent;
		steering.Initialize(agent);
	}
	public CharacterMovementValues GetCurrentMovementValue() {
		return steering.GetCurrentMovementValue();
	}

	public void SetGoal(NavigationGoal goal) {
		steering.SetTargetPosition(DetermineTargetPositionFromGoal(goal));
	}

	protected abstract Vector3 DetermineTargetPositionFromGoal(NavigationGoal goal);

}
