using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NavigationGoalPicker : MonoBehaviour {

	protected GameObject agent;

	public void Initialize(GameObject agent) {
		this.agent = agent;
	}

	// Decides which goal should be next
	public abstract NavigationGoal GetGoal();

    // May be called several times after GetGoal()
    // Should return a different goal which has not been returned since the last GetGoal() call
    public abstract NavigationGoal GetAnotherGoal();

}