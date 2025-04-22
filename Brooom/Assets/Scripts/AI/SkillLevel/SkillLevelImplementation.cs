using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A base class for determining agents' stats values to affect their performance during race.
/// This allows them to adjust to different situations (e.g., improve when they are far behind the player).
/// Derived classes may implement different strategies for determining these values.
/// </summary>
public abstract class SkillLevelImplementation : MonoBehaviour {

	[Tooltip("Curve describing fraction of maximum speed which is added to the agent's current maximum speed based on distance from the player. Positive distance means the agent is in front of the player.")]
	[SerializeField] protected AnimationCurve maxSpeedAddedBasedOnDistance;

	/// <summary>Agent's race state.</summary>
	protected CharacterRaceState agentRaceState;
	/// <summary>Agent's movement controller to affect maximum speed.</summary>
	protected CharacterMovementController agentMovementController;

	/// <summary>Initial fraction of maximum speed which is used in agent's movement controller (based on broom upgrades).</summary>
	protected float initialMaxSpeedFraction = -1;

	private float[] trackPointDistanceSum;
	/// <summary>Sum of distances between track points up to the given index (to be able to quickly compute approximation of distance raced).</summary>
	protected float[] TrackPointDistanceSum {
		get {
			if (trackPointDistanceSum == null) {
				// Precompute sums of track point distances
				trackPointDistanceSum = new float[RaceControllerBase.Instance.Level.Track.Count + 1];
				for (int i = 0; i < trackPointDistanceSum.Length; i++) {
					if (i == 0) // from start to the first hoop
						trackPointDistanceSum[i] = Vector3.Distance(RaceControllerBase.Instance.Level.playerStartPosition, RaceControllerBase.Instance.Level.Track[i].position);
					else {
						if (i == RaceControllerBase.Instance.Level.Track.Count) // from the last hoop to finish
							trackPointDistanceSum[i] = Vector3.Distance(RaceControllerBase.Instance.Level.finish.transform.position, RaceControllerBase.Instance.Level.Track[i - 1].position);
						else // from a hoop to another hoop
							trackPointDistanceSum[i] = Vector3.Distance(RaceControllerBase.Instance.Level.Track[i - 1].position, RaceControllerBase.Instance.Level.Track[i].position);
						trackPointDistanceSum[i] += trackPointDistanceSum[i - 1];
					}
				}
			}
			return trackPointDistanceSum;
		}
	}
	
	/// <summary>
	/// Initialize everything necessary for computing agent's stats affecting their performance during race.
	/// </summary>
	/// <param name="agentRaceState">Agent's <c>RaceState</c> component for getting current state.</param>
	/// <param name="agentMovementController">Agent's <c>MovementController</c> component for affecting maximum speed.</param>
	public void Initialize(CharacterRaceState agentRaceState, CharacterMovementController agentMovementController) {
		this.agentRaceState = agentRaceState;
		this.agentMovementController = agentMovementController;
	}

	/// <summary>
	/// Determines initial stats values based on the given skill level type.
	/// </summary>
	/// <param name="skillLevelType">Skill level type for which to initialize stats.</param>
	/// <returns>Stats values based on skill level.</returns>
	public abstract PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType);
	/// <summary>
	/// Determines initial stats values based on the agent' skill level as well as current situation during race.
	/// </summary>
	/// <returns>CUrrent stats values based on current situation.</returns>
	public abstract PlayerStats GetCurrentStats();

	/// <summary>
	/// Adjust agent's maximum speed based on distance between the agent and player.
	/// This allows to set higher maximum speed if the agent is too far behind the player.
	/// </summary>
	public virtual void AdjustCurrentMaximumSpeed() {
		if (initialMaxSpeedFraction < 0) initialMaxSpeedFraction = agentMovementController.GetMaxSpeed() / CharacterMovementController.MAX_SPEED;
		float distanceDifference = GetDistanceBetweenAgentAndPlayer();
		// Max speed increment is determined by a curve
		distanceDifference = Mathf.Clamp(distanceDifference, maxSpeedAddedBasedOnDistance.keys[0].time, maxSpeedAddedBasedOnDistance.keys[maxSpeedAddedBasedOnDistance.length - 1].time);
		float maxSpeedAdded = maxSpeedAddedBasedOnDistance.Evaluate(distanceDifference);
		agentMovementController.SetMaxSpeed(initialMaxSpeedFraction + maxSpeedAdded);
	}

	/// <summary>
	/// Computes approximate distance raced in the track for a racer with the given race state.
	/// </summary>
	/// <param name="raceState">Race state of a racer for whom to compute distance raced.</param>
	/// <returns>Distance raced in the track so far.</returns>
	protected float GetDistanceRaced(CharacterRaceState raceState) {
		if (!RaceControllerBase.Instance.IsInitialized) return 0;
		float distanceRaced;
		// Sum up distances between all track points reached (+ the following one)
		distanceRaced = TrackPointDistanceSum[raceState.followingTrackPoint];
		// Subtract distance between the agent and the following track point
		if (raceState.followingTrackPoint == RaceControllerBase.Instance.Level.Track.Count) { // next is finish
			distanceRaced -= Vector3.Distance(raceState.transform.position, RaceControllerBase.Instance.Level.finish.transform.position);
		} else {
			distanceRaced -= Vector3.Distance(raceState.transform.position, RaceControllerBase.Instance.Level.Track[raceState.followingTrackPoint].position);
		}
		return distanceRaced;
	}

	/// <summary>
	/// Computes fraction of the track reached so far (normalized between 0 and 1) for a racer with the given race state.
	/// </summary>
	/// <param name="raceState">Race state of a racer for whom to compute distance raced.</param>
	/// <returns>Normalized distance raced in the track so far.</returns>
	protected float GetNormalizedDistanceRaced(CharacterRaceState raceState) {
		if (!RaceControllerBase.Instance.IsInitialized) return 0;
		// Get distance reached so far
		float distanceRaced = GetDistanceRaced(raceState);
		// Get total length of the track
		float totalDistance = TrackPointDistanceSum[TrackPointDistanceSum.Length - 1];
		float normalizedDistance = distanceRaced / totalDistance;
		// Clamp to (0, 1) - in case the racer is still before the first hoop but farther from it then the start (so the distance is negative)
		normalizedDistance = Mathf.Clamp(normalizedDistance, 0, 1);
		return normalizedDistance;
	}

	/// <summary>
	/// Computes difference between distance raced so far for this agent and for the player.
	/// </summary>
	/// <returns>Distance from the player in terms of distance raced in track.</returns>
	protected float GetDistanceBetweenAgentAndPlayer() {
		if (!RaceControllerBase.Instance.IsInitialized) return 0;
		// Compute difference between agent's and player's distance raced
		float distanceDifference = GetDistanceRaced(this.agentRaceState) - GetDistanceRaced(RaceControllerBase.Instance.playerRacer.state);
		return distanceDifference;
	}

	/// <summary>
	/// Computes a new stat value based on the initial value and a percentage change.
	/// </summary>
	/// <param name="initialValue">Initial stat value which is to be modified.</param>
	/// <param name="percentageChange">Percentage change. If negative, this percentage of stat value will be subtracted. If positive, this percentage of the stat value complement will be added.</param>
	/// <returns>Stat value after adjustment.</returns>
	protected int GetModifiedStatValue(int initialValue, float percentageChange) {
		if (percentageChange < 0) { // Decreasing the value - subtract percentage of the stat value
			return Mathf.RoundToInt(initialValue - initialValue * percentageChange);
		} else { // Increasing the value - add percentage of the mistakes (stat value complement)
			return Mathf.RoundToInt(initialValue + (100 - initialValue) * percentageChange);
		}
	}

}
