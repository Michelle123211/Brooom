using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNavigationGoalPicker : NavigationGoalPicker {

	[Tooltip("The agent will not pursue bonuses whose angle from the agent is not within this threshold.")]
	[SerializeField] float yawAngleThreshold = 60f;
	[Tooltip("The agent will not pursue bonuses whose angle from the agent is not within this threshold.")]
	[SerializeField] float pitchAngleThreshold = 30f;
	[Tooltip("The agent prefers bonus to next track point, even if going in the wrong direction, if it is at most this far.")]
	[SerializeField] float bonusReturnDistanceThreshold = 8f;

	private int lastBonusPickedUp = -1;

	public override NavigationGoal GetGoal() {
		if (raceState.HasFinished) return new EmptyGoal(this.agent);

		NavigationGoal nextGoal = null;
		// Get next bonus index and next hoop index
		int nextBonus = SelectNextBonusIndex();
		int nextHoop = raceState.trackPointToPassNext;
		// Get the first next goal which should not be skipped
		bool shouldBeSkipped = true;
		while (shouldBeSkipped) {
			NavigationGoal nextTrackGoal = CreateTrackGoalFromIndex(nextHoop);
			nextGoal = nextTrackGoal;
			if (nextBonus != -1) {
				// Check all bonuses in front of the next hoop
				while (nextBonus < RaceController.Instance.level.bonuses.Count 
					&& RaceController.Instance.level.bonuses[nextBonus].previousHoopIndex < nextHoop) {
					if (IsBonusSuitableAsGoal(nextBonus)) {
						// Compare the bonus goal with the hoop goal (which is better or not null)
						nextGoal = ChooseBetterGoal(nextTrackGoal, new BonusGoal(this.agent, nextBonus));
						// If bonus goal is picked, store it and break the cycle
						if (nextGoal.Type == NavigationGoalType.Bonus) break;
					}
					nextBonus++;
				}
			}
			// Increase the corresponding index
			if (nextGoal.Type == NavigationGoalType.Bonus) nextBonus++;
			else nextHoop++;
			// Determine whether the goal should be skipped
			shouldBeSkipped = nextGoal.ShouldBeSkipped();
		}
		// Return the new goal
		return nextGoal;
	}

	private NavigationGoal ChooseBetterGoal(NavigationGoal trackGoal, NavigationGoal bonusGoal) {
		// Decide whether to go for the track point or bonus
		Vector3 trackTarget = trackGoal.TargetPosition;
		Vector3 bonusTarget = bonusGoal.TargetPosition;
		Vector3 agentPosition = this.agent.transform.position;
		float bonusDistance = Vector3.Distance(agentPosition, bonusTarget);
		// If the track goal is closer, it has priority
		if (bonusDistance >= Vector3.Distance(agentPosition, trackTarget))
			return trackGoal;
		// If the agent is going in the correct direction (relative to the track orientation)
		if (Mathf.Abs(Vector3.SignedAngle(this.agent.transform.forward, trackTarget - agentPosition, Vector3.up)) < 90f) {
			// ... and the bonus is more or less on the way to the next track point, go for the bonus
			if (Mathf.Abs(Vector3.SignedAngle(trackTarget - agentPosition, bonusTarget - agentPosition, Vector3.up)) < yawAngleThreshold / 2f)
				return bonusGoal;
			// ... otherwise don't go necessarily far just for the bonus
			else return trackGoal;
		} 
		// Otherwise the agent is going in the wrong direction
		else {
			// ... go for the bonus only if it is VERY close
			if (Vector3.Distance(agentPosition, bonusTarget) < bonusReturnDistanceThreshold)
				return bonusGoal;
			else return trackGoal;
		}
	}

	// Creates goal for the next hoop/checkpoint or finish
	private NavigationGoal CreateTrackGoalFromIndex(int index) {
		if (index >= raceState.hoopsPassedArray.Length) {
			return new FinishNavigationGoal(this.agent);
		} else if (RaceController.Instance.level.track[index].isCheckpoint) {
			return new CheckpointGoal(this.agent, index);
		} else {
			return new HoopGoal(this.agent, index);
		}
	}

	// Selects bonus which should be picked up next (according to distance and agent's orientation)
	private int SelectNextBonusIndex() {
		float minDistance = float.MaxValue;
		int bonusIndex = -1;
		// Select the closest bonus from those which are in front of the agent
		for (int i = 0; i < RaceController.Instance.level.bonuses.Count; i++) {
			if (!IsBonusSuitableAsGoal(i)) continue;
			// Select the closest bonus
			float distance = Vector3.Distance(this.agent.transform.position, RaceController.Instance.level.bonuses[bonusIndex].position);
			if (distance < minDistance) {
				minDistance = distance;
				bonusIndex = i;
			}
		}
		// And return
		return bonusIndex;
	}

	// Determines whether the bonus with the given index is a suitable goal (regarding it's availability, position and the agent's orientation)
	private bool IsBonusSuitableAsGoal(int bonusIndex) {
		BonusSpot bonusSpot = RaceController.Instance.level.bonuses[bonusIndex];
		// The bonus must be available (have at least one instance active)
		if (!bonusSpot.IsBonusAvailable()) return false;
		// Ignore the bonus if it was picked up as the last one (we don't want to choose it again immediately)
		if (bonusIndex == lastBonusPickedUp) return false;
		// Check if the bonus is in front of the agent
		float angleYaw = Vector3.SignedAngle(this.agent.transform.forward, bonusSpot.position - this.agent.transform.position, Vector3.up);
		float anglePitch = Vector3.SignedAngle(this.agent.transform.forward, bonusSpot.position - this.agent.transform.position, this.agent.transform.right);
		if (Mathf.Abs(angleYaw) > yawAngleThreshold || Mathf.Abs(anglePitch) > pitchAngleThreshold)
			return false;
		// Otherwise it is suitable
		return true;
	}

	public override void OnGoalReached(NavigationGoal goal) {
		// Remember the last picked up bonus so that it is not chosen immediately again
		if (goal.Type == NavigationGoalType.Bonus) {
			BonusGoal bonusGoal = goal as BonusGoal;
			lastBonusPickedUp = bonusGoal.index;
		}
	}
}
