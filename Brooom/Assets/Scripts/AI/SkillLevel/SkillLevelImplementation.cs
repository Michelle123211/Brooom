using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillLevelImplementation : MonoBehaviour {

	protected CharacterRaceState agentRaceState;

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
	
	public void Initialize(CharacterRaceState agentRaceState) {
		this.agentRaceState = agentRaceState;
	}

	public abstract PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType);
	public abstract PlayerStats GetCurrentStats();

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
