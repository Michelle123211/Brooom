using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(NavigationGoalPicker))]
[RequireComponent(typeof(NavigationGoalExecutor))]
public class GoalOrientedNavigation : CharacterInput {

	// Parameters - may be extracted outside (e.g. to ScriptableObject)
	[Tooltip("TIme in seconds after which the agent may try to choose another goal.")]
	[SerializeField] float deliberationInterval = 3;

	[Tooltip("Each goal is assigned rationality between 0 (not rational) and 1. Goals with rationality below this threshold will be automatically reconsidered.")]
	[SerializeField] float rationalityThreshold = 0.1f;

	[Tooltip("GameObject reference to the agent which is controlled by this AI. If null, .gameObject of this component is taken.")]
	[SerializeField] GameObject agentObject;

	[SerializeField] bool debugLogs = false;

	private NavigationGoalPicker goalPicker;
	private NavigationGoalExecutor goalExecutor;

	private NavigationGoal currentGoal;

	private float deliberationCountdown;

	public override CharacterMovementValues GetMovementInput() {
		return goalExecutor.GetCurrentMovementValue();
	}

	private NavigationGoal GetNewGoal() {
		NavigationGoal newGoal = goalPicker.GetGoal();
		bool shouldBeSkipped = false;
		if (shouldBeSkipped) newGoal = null;
		while (newGoal == null) {
			newGoal = goalPicker.GetAnotherGoal();
			// TODO: Decide whether the goal should be skipped
			shouldBeSkipped = false;
			if (shouldBeSkipped) newGoal = null;
		}
		return newGoal;
	}

	private void SetNewGoal(NavigationGoal goal) {
		currentGoal = goal;
		deliberationCountdown = deliberationInterval; // reset deliberation
	}

	private void Update() {
		if (RaceController.Instance.State != RaceState.RaceInProgress) return;

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
		if (!goalReached && goalValid && !shouldDeliberate && goalRational) return;

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
			NavigationGoal newGoal = GetNewGoal();
			SetNewGoal(newGoal);
			// Start working on the goal
			goalExecutor.SetGoal(currentGoal);
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
