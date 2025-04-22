using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A base class for opponents' navigation component which is responsible for executing actions leading to fulfilling the current navigation goal.
/// Different derived classes may adapt different strategies.
/// </summary>
[RequireComponent(typeof(SkillBasedNavigationSteering))]
public abstract class NavigationGoalExecutor : MonoBehaviour {

	/// <summary>Agent which is controlled by this component.</summary>
	protected GameObject agent;
	/// <summary>Component responsible for navigating towards a target position.</summary>
	protected NavigationSteering steering;
	/// <summary>Current navigation goal the agent is navigated towards.</summary>
	protected NavigationGoal currentGoal;

	/// <summary>
	/// Initializes everything necessary for executing actions leading to fulfilling navigation goals for the given agent, e.g. gets <c>NavigationSteering</c> component and initializes it.
	/// </summary>
	/// <param name="agent">Agent to which this component belongs.</param>
	public void Initialize(GameObject agent) {
		this.agent = agent;
		this.steering = GetComponent<NavigationSteering>();
		this.steering.Initialize(agent);
	}

	/// <summary>
	/// Computes current movement values which should be used right now to navigate towards the current navigation goal, with help of <c>NavigationSteering</c> component.
	/// </summary>
	/// <returns>Movement values as <c>CharacterMovementValues</c> for the agent to use.</returns>
	public CharacterMovementValues GetCurrentMovementValue() {
		return steering.GetCurrentMovementValue();
	}

	/// <summary>
	/// Sets current navigation goal for the agent to be navigated to and updates steering component's target position.
	/// </summary>
	/// <param name="goal">Current navigation goal. If its type is <c>NavigationGoalType.None</c>, steering will be deactivated.</param>
	public void SetGoal(NavigationGoal goal) {
		currentGoal = goal;
		if (goal.Type == NavigationGoalType.None) {
			steering.StopSteering();
		} else {
			steering.SetTargetPosition(DetermineTargetPositionFromGoal(goal));
		}
	}

	/// <summary>
	/// Updates target position computed from the current navigation goal (because the goal may be moving or the position may be dependent on agent's current position).
	/// </summary>
	public void UpdateCurrentGoalPosition() {
		// For now update target position only for finish which is dependent on current character position, others are clearly given
		if (currentGoal.Type == NavigationGoalType.Finish) {
			steering.SetTargetPosition(DetermineTargetPositionFromGoal(currentGoal));
		}
	}

	/// <summary>
	/// Computes target position from the given navigation goal.
	/// </summary>
	/// <param name="goal">Navigation goal from which to compute target position.</param>
	/// <returns>Target position based on the given navigation goal.</returns>
	protected abstract Vector3 DetermineTargetPositionFromGoal(NavigationGoal goal);

}
