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

	public override NavigationGoal GetAnotherGoal() {
		// TODO - return different goal
		return GetGoal();
	}

	public override NavigationGoal GetGoal() {
		// TODO - take into consideration other goal types

		if (raceState.HasFinished) return new EmptyGoal(this.agent);

		NavigationGoal nextTrackGoal = SelectNextTrackGoal();
		NavigationGoal nextBonusGoal = SelectNextBonusGoal();

		if (nextBonusGoal == null) return nextTrackGoal;
		return ChooseBetterGoal(nextTrackGoal, nextBonusGoal);
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
	private NavigationGoal SelectNextTrackGoal() {
		int nextHoopIndex = raceState.trackPointToPassNext;
		if (nextHoopIndex >= raceState.hoopsPassedArray.Length) {
			return new FinishNavigationGoal(this.agent);
		} else if (RaceController.Instance.level.track[nextHoopIndex].isCheckpoint) {
			return new CheckpointGoal(this.agent, nextHoopIndex);
		} else {
			return new HoopGoal(this.agent, nextHoopIndex);
		}
	}

	// Creates goal for the 
	private NavigationGoal SelectNextBonusGoal() {
		float minDistance = float.MaxValue;
		int bonusIndex = -1;

		// Select the closest bonus from those which are in front of the agent
		for (int i = 0; i < RaceController.Instance.level.bonuses.Count; i++) {
			BonusSpot bonusSpot = RaceController.Instance.level.bonuses[i];
			if (!bonusSpot.IsBonusAvailable()) continue;
			// Ignore the bonus which was picked up as the last one (we don't want to choose it again immediately)
			if (i == lastBonusPickedUp) continue;
			// Check if the bonus is in front of the agent
			float angleYaw = Vector3.SignedAngle(this.agent.transform.forward, bonusSpot.position - this.agent.transform.position, Vector3.up);
			float anglePitch = Vector3.SignedAngle(this.agent.transform.forward, bonusSpot.position - this.agent.transform.position, this.agent.transform.right);
			if (Mathf.Abs(angleYaw) > yawAngleThreshold || Mathf.Abs(anglePitch) > pitchAngleThreshold)
				continue;
			// Select the closest bonus
			float distance = Vector3.Distance(this.agent.transform.position, bonusSpot.position);
			if (distance < minDistance) {
				minDistance = distance;
				bonusIndex = i;
			}
		}

		if (bonusIndex == -1) return null;
		else return new BonusGoal(this.agent, bonusIndex);
	}

	public override void OnGoalReached(NavigationGoal goal) {
		// Remember the last picked up bonus so that it is not chosen immediately again
		if (goal.Type == NavigationGoalType.Bonus) {
			BonusGoal bonusGoal = goal as BonusGoal;
			lastBonusPickedUp = bonusGoal.index;
		}
	}
}
