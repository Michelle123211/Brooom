using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNavigationGoalExecutor : NavigationGoalExecutor {

	protected override Vector3 DetermineTargetPositionFromGoal(NavigationGoal goal) {
		// TODO: Take into consideration errors depending on the goal type
		return goal.TargetPosition;
	}
}
