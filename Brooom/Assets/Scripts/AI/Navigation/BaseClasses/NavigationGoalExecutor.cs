using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(NavigationSteering))]
public abstract class NavigationGoalExecutor : MonoBehaviour {

	protected GameObject agent;

	protected NavigationSteering steering;

	public void Initialize(GameObject agent) {
		this.agent = agent;
	}

	public abstract void SetGoal(NavigationGoal goal);
    
}
