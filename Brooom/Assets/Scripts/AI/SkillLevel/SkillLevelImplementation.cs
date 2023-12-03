using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillLevelImplementation : MonoBehaviour {

	[Tooltip("Curve describing fraction of maximum speed which is added to the agent's current maximum speed based on distance from the player. Positive distance means the agent is in front of the player.")]
	[SerializeField] protected AnimationCurve maxSpeedAddedBasedOnDistance;

	protected CharacterRaceState agentRaceState;
	protected CharacterMovementController agentMovementController;

	protected float initialMaxSpeedFraction = -1;

	private float[] trackPointDistanceSum;
	protected float[] TrackPointDistanceSum { // sum of distances between track points up to the given index
		get {
			if (trackPointDistanceSum == null) { // precompute sums of track point distances
				trackPointDistanceSum = new float[RaceController.Instance.level.track.Count + 1];
				for (int i = 0; i < trackPointDistanceSum.Length; i++) {
					if (i == 0) // from start to the first hoop
						trackPointDistanceSum[i] = Vector3.Distance(RaceController.Instance.level.playerStartPosition, RaceController.Instance.level.track[i].position);
					else {
						if (i == RaceController.Instance.level.track.Count) // from the last hoop to finish
							trackPointDistanceSum[i] = Vector3.Distance(RaceController.Instance.level.finish.transform.position, RaceController.Instance.level.track[i - 1].position);
						else // from a hoop to another hoop
							trackPointDistanceSum[i] = Vector3.Distance(RaceController.Instance.level.track[i - 1].position, RaceController.Instance.level.track[i].position);
						trackPointDistanceSum[i] += trackPointDistanceSum[i - 1];
					}
				}
			}
			return trackPointDistanceSum;
		}
	}
	
	public void Initialize(CharacterRaceState agentRaceState, CharacterMovementController agentMovementController) {
		this.agentRaceState = agentRaceState;
		this.agentMovementController = agentMovementController;
	}

	public abstract PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType);
	public abstract PlayerStats GetCurrentStats();

	// Allows to set higher maximum speed if the AI agent is too far behind the player
	public virtual void AdjustCurrentMaximumSpeed() {
		if (initialMaxSpeedFraction < 0) initialMaxSpeedFraction = agentMovementController.GetMaxSpeed() / CharacterMovementController.MAX_SPEED;
		float distanceDifference = GetDistanceBetweenAgentAndPlayer();
		// Max speed increment is determined by a curve
		distanceDifference = Mathf.Clamp(distanceDifference, maxSpeedAddedBasedOnDistance.keys[0].time, maxSpeedAddedBasedOnDistance.keys[maxSpeedAddedBasedOnDistance.length - 1].time);
		float maxSpeedAdded = maxSpeedAddedBasedOnDistance.Evaluate(distanceDifference);
		agentMovementController.SetMaxSpeed(initialMaxSpeedFraction + maxSpeedAdded);
	}

	// For the given racer, returns approximate reached distance in the track
	protected float GetDistanceRaced(CharacterRaceState raceState) {
		float distanceRaced;
		// Sum up distances between all track points reached (+ the following one)
		distanceRaced = TrackPointDistanceSum[raceState.followingTrackPoint];
		// Subtract distance between the agent and the following track point
		if (raceState.followingTrackPoint == RaceController.Instance.level.track.Count) {
			distanceRaced -= Vector3.Distance(raceState.transform.position, RaceController.Instance.level.finish.transform.position);
		} else {
			distanceRaced -= Vector3.Distance(raceState.transform.position, RaceController.Instance.level.track[raceState.followingTrackPoint].position);
		}
		return distanceRaced;
	}

	// For the given racer, returns fraction of the track reached so far (normalized between 0 and 1)
	protected float GetNormalizedDistanceRaced(CharacterRaceState raceState) {
		// Get distance reached so far
		float distanceRaced = GetDistanceRaced(raceState);
		// Get total length of the track
		float totalDistance = TrackPointDistanceSum[TrackPointDistanceSum.Length - 1];
		float normalizedDistance = distanceRaced / totalDistance;
		// Clamp to (0, 1) - in case the racer is still before the first hoop but farther from it then the start (do the distance is negative)
		normalizedDistance = Mathf.Clamp(normalizedDistance, 0, 1);
		return normalizedDistance;
	}

	protected float GetDistanceBetweenAgentAndPlayer() {
		// Compute difference between agent's and player's distance raced
		float distanceDifference = GetDistanceRaced(this.agentRaceState) - GetDistanceRaced(RaceController.Instance.playerRacer.state);
		return distanceDifference;
	}

	protected int GetModifiedStatValue(int initialValue, float percentageChange) {
		if (percentageChange < 0) { // Decreasing the value - subtract percentage of the stat value
			return Mathf.RoundToInt(initialValue - initialValue * percentageChange);
		} else { // Increasing the value - add percentage of the mistakes (stat value complement)
			return Mathf.RoundToInt(initialValue + (100 - initialValue) * percentageChange);
		}
	}

}
