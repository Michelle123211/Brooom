using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NavigationGoalPicker : MonoBehaviour {

	protected GameObject agent;
	protected CharacterRaceState raceState;

	public void Initialize(GameObject agent) {
		this.agent = agent;
		this.raceState = agent.GetComponentInChildren<CharacterRaceState>();
	}

	// Decides which goal should be next
	public abstract NavigationGoal GetGoal();

    // May be called several times after GetGoal()
    // Should return a different goal which has not been returned since the last GetGoal() call
    public abstract NavigationGoal GetAnotherGoal();

	// Lets the NavigationGoalPicker know that the given goal was reached (so he can adapt to that if necessary)
	public abstract void OnGoalReached(NavigationGoal goal);

}