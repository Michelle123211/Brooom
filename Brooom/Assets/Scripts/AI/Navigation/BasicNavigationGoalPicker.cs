using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNavigationGoalPicker : NavigationGoalPicker {

	public override NavigationGoal GetAnotherGoal() {
		// TODO - return different goal
		return GetGoal();
	}

	public override NavigationGoal GetGoal() {
		// TODO - take into consideration other goal types
		int nextHoopIndex = raceState.trackPointToPassNext;
		if (raceState.HasFinished) {
			return new EmptyGoal(this.agent);
		} else if (nextHoopIndex >= raceState.hoopsPassedArray.Length) {
			return new FinishNavigationGoal(this.agent);
		}  else if (RaceController.Instance.level.track[nextHoopIndex].isCheckpoint) {
			return new CheckpointGoal(this.agent, nextHoopIndex);
		} else {
			return new HoopGoal(this.agent, nextHoopIndex);
		}
	}
}
