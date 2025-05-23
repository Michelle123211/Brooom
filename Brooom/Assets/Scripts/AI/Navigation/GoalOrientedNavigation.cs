using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class determining opponents' movement values based on goal-oriented navigation.
/// It uses <c>NavigationGoalPicker</c> implementation for selecting navigation goals and <c>NavigationGoalExecutor</c> implementation for getting movement values leading to fulfilling the current goal.
/// </summary>
[RequireComponent(typeof(NavigationGoalPicker))]
[RequireComponent(typeof(NavigationGoalExecutor))]
public class GoalOrientedNavigation : CharacterInput {

	// Parameters - could be extracted outside (e.g. to ScriptableObject)
	[Tooltip("Time in seconds after which the agent may try to choose another goal.")]
	[SerializeField] float deliberationInterval = 6;

	[Tooltip("Each goal is assigned rationality between 0 (not rational) and 1. Goals with rationality below this threshold will be automatically reconsidered.")]
	[SerializeField] float rationalityThreshold = 0.1f;

	[Tooltip("GameObject reference to the agent which is controlled by this AI. If null, .gameObject of this component is taken.")]
	[SerializeField] GameObject agentObject;

	[Tooltip("Whether debug messages should be logged.")]
	[SerializeField] bool debugLogs = false;

	// Units for solving partial tasks - selecting next goal and determining actions to fulfill it
	private NavigationGoalPicker goalPicker;
	private NavigationGoalExecutor goalExecutor;

	private NavigationGoal currentGoal;

	private float deliberationCountdown; // time left before deliberation takes place

	/// <inheritdoc/>
	public override CharacterMovementValues GetMovementInput() {
		return goalExecutor.GetCurrentMovementValue();
	}

	private void SetNewGoal(NavigationGoal goal) {
		if (debugLogs) Debug.Log($"Goal changed to {goal.Type}.");
		currentGoal = goal;
		deliberationCountdown = deliberationInterval; // reset deliberation
		// Start working on the goal
		goalExecutor.SetGoal(currentGoal);
	}

	private void Update() {
		// Do computation only if the race is in progress
		if (RaceControllerBase.Instance.State != RaceState.RaceInProgress) return;

		deliberationCountdown -= Time.deltaTime;
		// Wait until one of the conditions is true:
		// --- the current goal has been reached
		bool goalReached = currentGoal == null ? false : currentGoal.IsReached();
		// --- the current goal is no longer valid
		bool goalValid = currentGoal == null ? false : currentGoal.IsValid();
		// --- the current goal is not rational anymore (e.g. the assigned bonus has been missed)
		bool goalRational = currentGoal == null ? false : currentGoal.GetRationality() >= rationalityThreshold;
		// --- deliberation cooldown has been reached
		bool shouldDeliberate = deliberationCountdown <= 0;

		// If the current goal should be kept, update its position and return
		if (!goalReached && goalValid && !shouldDeliberate && goalRational) {
			goalExecutor.UpdateCurrentGoalPosition();
			return;
		}

		if (debugLogs) {
			Debug.Log("-------------------");
			Debug.Log($"Current goal: {(currentGoal == null ? "Null" : currentGoal.Type)}");
			Debug.Log($"Reached: {goalReached}, valid: {goalValid}");
			Debug.Log($"Rationality: {goalRational}, should deliberate: {shouldDeliberate}");
		}

		// React accordingly
		if (goalReached) goalPicker.OnGoalReached(currentGoal);
		if (shouldDeliberate) deliberationCountdown += deliberationInterval;

		// Get new possible goal and set it as the current one (under some circumstances)
		if (!goalValid || goalReached || !goalRational || shouldDeliberate) {
			NavigationGoal newGoal = goalPicker.GetGoal();
			if (!newGoal.IsSameAs(currentGoal))
				SetNewGoal(newGoal);
		}
	}

	private void Start() {
		// Initialize variables
		deliberationCountdown = deliberationInterval;
		if (agentObject == null) agentObject = gameObject;
		// Initialize related components
		goalPicker = GetComponent<NavigationGoalPicker>();
		goalPicker.Initialize(agentObject);
		goalExecutor = GetComponent<NavigationGoalExecutor>();
		goalExecutor.Initialize(agentObject);
	}
}
