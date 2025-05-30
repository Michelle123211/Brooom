using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Opponents' navigation component responsible for selecting the next goal to which to be navigated.
/// It uses simple rules to determine, whether to pursue the following hoop/checkpoint or bonus (e.g. based on distance and direction).
/// </summary>
public class BasicNavigationGoalPicker : NavigationGoalPicker {

	[Tooltip("The agent will not pursue bonuses whose angle from the agent is not within this threshold.")]
	[SerializeField] float yawAngleThreshold = 60f;
	[Tooltip("The agent will not pursue bonuses whose angle from the agent is not within this threshold.")]
	[SerializeField] float pitchAngleThreshold = 30f;
	[Tooltip("If the agent is going in the wrong direction, it prefers bonus in that direction to next track point, if it is at most this far.")]
	[SerializeField] float bonusReturnDistanceThreshold = 8f;

	private int lastBonusPickedUp = -1; // index of the bonus spot from which the bonus was picked up

	// Remember which hoops/bonuses have been determined to be skipped so that they can be skipped in the next iteration too
	private HashSet<int> skippedHoops = new HashSet<int>();
	private HashSet<int> skippedBonuses = new HashSet<int>();

	/// <inheritdoc/>
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
				while (nextBonus < RaceControllerBase.Instance.Level.bonuses.Count 
					&& RaceControllerBase.Instance.Level.bonuses[nextBonus].previousHoopIndex < nextHoop) {
					if (IsBonusSuitableAsGoal(nextBonus)) {
						// Compare the bonus goal with the hoop goal (which is better or not null)
						nextGoal = ChooseBetterGoal(nextTrackGoal, new BonusGoal(this.agent, nextBonus));
						// If bonus goal is picked, store it and break the cycle
						if (nextGoal.Type == NavigationGoalType.Bonus) break;
					}
					nextBonus++;
				}
			}
			// Determine whether the goal should be skipped (based on agent's skill level)
			HashSet<int> skippedGoals = null;
			int goalIndex = -1;
			if (nextGoal.Type == NavigationGoalType.Hoop) {
				skippedGoals = skippedHoops;
				goalIndex = nextHoop;
			} else if (nextGoal.Type == NavigationGoalType.Bonus) {
				skippedGoals = skippedBonuses;
				goalIndex = nextBonus;
			}
			if (skippedGoals != null && goalIndex >= 0) { // hoop or bonus
				if (skippedGoals.Contains(goalIndex)) shouldBeSkipped = true;
				else {
					shouldBeSkipped = nextGoal.ShouldBeSkipped();
					// Remember skipped elements so that they can be skipped the next time too
					if (shouldBeSkipped) skippedGoals.Add(goalIndex);
				}
			} else { // other bonus type
				shouldBeSkipped = nextGoal.ShouldBeSkipped();
			}
			// Increase the corresponding index
			if (nextGoal.Type == NavigationGoalType.Bonus) nextBonus++;
			else nextHoop++;
		}
		// Return the new goal
		return nextGoal;
	}

	// Decides whether to go for the track point (hoop/checkpoint/finish) or bonus
	private NavigationGoal ChooseBetterGoal(NavigationGoal trackGoal, NavigationGoal bonusGoal) {
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
		} else if (RaceControllerBase.Instance.Level.Track[index].isCheckpoint) {
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
		for (int i = 0; i < RaceControllerBase.Instance.Level.bonuses.Count; i++) {
			if (!IsBonusSuitableAsGoal(i)) continue;
			// Select the closest bonus
			float distance = Vector3.Distance(this.agent.transform.position, RaceControllerBase.Instance.Level.bonuses[i].position);
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
		BonusSpot bonusSpot = RaceControllerBase.Instance.Level.bonuses[bonusIndex];
		// The bonus must be available (have at least one instance active)
		if (!bonusSpot.IsBonusAvailable()) return false;
		// Ignore the bonus if it was picked up as the last one (we don't want to choose it again immediately)
		if (bonusIndex == lastBonusPickedUp) return false;
		// Check if the bonus is in front of the agent
		float angleYaw = Vector3.SignedAngle(this.agent.transform.forward, bonusSpot.position - this.agent.transform.position, Vector3.up);
		float anglePitch = Vector3.SignedAngle(this.agent.transform.forward, bonusSpot.position - this.agent.transform.position, this.agent.transform.right);
		if (Mathf.Abs(angleYaw) > yawAngleThreshold || Mathf.Abs(anglePitch) > pitchAngleThreshold)
			return false; // if too far from the agent's direction, it is not suitable
		return true; // otherwise it is suitable
	}

	public override void OnGoalReached(NavigationGoal goal) {
		// Remember the last picked up bonus so that it is not chosen immediately again
		if (goal.Type == NavigationGoalType.Bonus) {
			BonusGoal bonusGoal = goal as BonusGoal;
			lastBonusPickedUp = bonusGoal.index;
		}
	}
}
