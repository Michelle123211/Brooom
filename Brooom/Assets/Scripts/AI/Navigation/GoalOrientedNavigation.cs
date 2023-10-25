using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(NavigationGoalPicker))]
[RequireComponent(typeof(NavigationGoalExecutor))]
public class GoalOrientedNavigation : CharacterInput {

	// Parameters - may be extracted outside (e.g. to ScriptableObject)
	[Tooltip("TIme in seconds after which the agent may try to choose another goal.")]
	public float deliberationInterval = 3;
	[Tooltip("Probability of choosing a new goal after deliberation instead of keeping the current one.")]
	public float deliberationProbability = 0.3f;

	[Tooltip("GameObject reference to the agent which is controlled by this AI. If null, .gameObject of this component is taken.")]
	public GameObject agentObject;

	public NavigationGoalPicker goalPicker;
	public NavigationGoalExecutor goalExecutor;

	private NavigationGoal currentGoal;

	private float deliberationCountdown;

	public override CharacterMovementValues GetMovementInput() {
		return goalExecutor.GetCurrentMovementValue();
	}

	private NavigationGoal GetNewGoal() {
		NavigationGoal newGoal = null;
		while (newGoal == null) {
			newGoal = goalPicker.GetGoal();
			// TODO: Decide whether the goal should be skipped
			bool shouldBeSkipped = false;
			if (shouldBeSkipped) newGoal = null;
		}
		return newGoal;
	}

	private void SetNewGoal(NavigationGoal goal) {
		currentGoal = goal;
		// Reset deliberation
		deliberationCountdown = deliberationInterval;
	}

	private void Update() {
		// TODO: Maybe check goal validity and completion after longer time intervals instead of every frame

		deliberationCountdown -= Time.deltaTime;
		// Wait until on of the conditions is true:
		// --- the current goal is no longer valid
		bool goalValid = currentGoal.IsValid();
		// --- the current goal has been reached
		bool goalReached = currentGoal.IsReached();
		// --- deliberation cooldown has been reached
		bool shouldDeliberate = (deliberationCountdown <= 0);
		if (!goalReached && goalValid && !shouldDeliberate) return;

		// Get new possible goal and set it se the current one (under some circumstances)
		NavigationGoal newGoal = GetNewGoal();
		if (!goalValid || goalReached || (shouldDeliberate && Random.value < deliberationProbability))
			SetNewGoal(newGoal);

		// Start working on the goal
		goalExecutor.SetGoal(currentGoal);
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
