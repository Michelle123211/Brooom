using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Opponents' navigation component handling steering to a given target point.
/// It uses <c>RaycastCollisionDetection</c> to detect objects in front of the agent to try to avoid collisions with them.
/// </summary>
[RequireComponent(typeof(RaycastCollisionDetection))]
public class BasicNavigationSteering : NavigationSteering {

	[Header("Angle thresholds")]
	[Tooltip("The current yaw direction is changed only if it is at least this angle from the direction to the target.")]
	[SerializeField] float yawAngleThreshold = 2f;
	[Tooltip("The current pitch direction is changed only if it is at least this angle from the direction to the target.")]
	[SerializeField] float pitchAngleThreshold = 2f;

	[Tooltip("If the angle between current direction and the direction to the target is greater than this angle, it is considered better to brake.")]
	[SerializeField] float brakeAngleThreshold = 90f;

	[Header("Input curves")]
	[Tooltip("Yaw input value depending on the angle.")]
	[SerializeField] AnimationCurve yawInputCurve;
	[Tooltip("Pitch input value depending on the angle.")]
	[SerializeField] AnimationCurve pitchInputCurve;
	[Tooltip("Forward input value depending on the angle.")]
	[SerializeField] AnimationCurve forwardInputCurveAngle;
	[Tooltip("Forward input value depending on distance to the target.")]
	[SerializeField] AnimationCurve forwardInputCurveDistance;

	[Header("Collisions")]
	[Tooltip("If the agent is closer to the target position than this value, they will not try to avoid collisions at all.")]
	[SerializeField] float collisionsMinDistance = 1;
	[Tooltip("If the agent is farther to the target position than this value, they will give full weight to avoiding collisions. Then they will focus more on target position.")]
	[SerializeField] float collisionsMaxDistance = 30;
	[Tooltip("Directions with weight lower than this (between 0 and 1) will not be considered possible (too significant collisions would occur).")]
	[SerializeField] float directionWeightThreshold = 0.1f;

	[Header("Other")]
	[Tooltip("Whether debug messages should be logged.")]
	[SerializeField] protected bool debugLogs = false;

	// Unit for detecting objects in front of the agent
	private RaycastCollisionDetection collisionDetection;

	/// <inheritdoc/>
	protected override CharacterMovementValues GetMovementToTargetPosition() {
		CharacterMovementValues movement = ComputeMovementValues();
		return AdjustMovementToAvoidCollisions(movement);
	}

	// Computes movement values leading to the current target position
	private CharacterMovementValues ComputeMovementValues() {
		Vector3 startPosition = this.agent.transform.position;
		Vector3 targetDirection = targetPosition - startPosition;
		float distance = Vector3.Distance(startPosition, targetPosition);

		CharacterMovementValues movement = new CharacterMovementValues();

		// Yaw
		float yawAngle = Vector3.SignedAngle(targetDirection, this.agent.transform.forward, Vector3.up);
		float yawAngleAbs = Mathf.Abs(yawAngle);
		if (yawAngle < -yawAngleThreshold) movement.yawMotion = YawMotion.Right;
		else if (yawAngle > yawAngleThreshold) movement.yawMotion = YawMotion.Left;
		else movement.yawMotion = YawMotion.None;
		movement.yawValue = yawInputCurve.Evaluate(yawAngleAbs);

		// Pitch
		float pitchAngle = Vector3.SignedAngle(targetDirection, this.agent.transform.forward, this.agent.transform.right);
		float pitchAngleAbs = Mathf.Abs(pitchAngle);
		if (pitchAngle < -pitchAngleThreshold) movement.pitchMotion = PitchMotion.Down;
		else if (pitchAngle > pitchAngleThreshold) movement.pitchMotion = PitchMotion.Up;
		else movement.pitchMotion = PitchMotion.None;
		movement.pitchValue = pitchInputCurve.Evaluate(pitchAngleAbs);

		// Forward
		if (yawAngleAbs > brakeAngleThreshold) movement.forwardMotion = ForwardMotion.Brake; // if the current direction is entirely wrong, brake
		else {
			movement.forwardMotion = ForwardMotion.Forward;
			movement.forwardValue = Mathf.Max(forwardInputCurveAngle.Evaluate(yawAngleAbs), forwardInputCurveDistance.Evaluate(distance));
		}

		return movement;
	}

	/// <summary>
	/// Adjusts the given movement values to avoid eventual collisions.
	/// </summary>
	/// <param name="movement">Original movement values based on the current target position.</param>
	/// <returns>New movement values combining direction to the target position and direction to avoid collisions.</returns>
	protected virtual CharacterMovementValues AdjustMovementToAvoidCollisions(CharacterMovementValues movement) {
		// Get the best direction for avoiding all possible collisions
		CollisionAvoidanceDirection direction = GetCollisionAvoidanceDirection();
		if (direction.weight == 0) return movement; // not significant enough, simply continue in the original direction
		// Combine direction to target with the direction to avoid collisions (simple average)
		return CombineMovementWithCollisionAvoidance(movement, direction);
	}

	/// <summary>
	/// Gets the best direction for avoiding all possible collisions.
	/// It is chosen from 4 directions (left, right, up, down) based on their weights (computed from collision distance and target distance).
	/// </summary>
	/// <returns>Collision avoidance direction together with its weight.</returns>
	protected CollisionAvoidanceDirection GetCollisionAvoidanceDirection() {
		Vector3 startPosition = this.agent.transform.position;
		Vector3 targetDirection3 = (targetPosition - startPosition);
		Vector2 targetDirection = new Vector2(targetDirection3.x, targetDirection3.y).normalized; // only left/right, up/down
		float distance = Vector3.Distance(startPosition, targetPosition);

		List<CollisionInfo> collisions = collisionDetection.GetListOfCollisions(); // from 5 raycasts
		// If there are no collisions to avoid, simply continue in the original direction
		if (collisions == null || collisions.Count == 0) return new CollisionAvoidanceDirection(Vector3.zero, 0);
		if (debugLogs) Debug.Log("Collisions detected.");
		// Create list of all possible directions (only left/right, up/down)
		List<CollisionAvoidanceDirection> possibleDirections = new List<CollisionAvoidanceDirection>();
		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				if ((i == 0 || j == 0) && (i != 0 || j != 0)) // only 4 directions (actually try to change direction instead of going forward (most likely into the obstacle right ahead))
					possibleDirections.Add(new CollisionAvoidanceDirection(new Vector2(i, j)));
			}
		}
		// Lower weight of directions leading to collision (depending on the collision distance - smaller distance, smaller weight)
		foreach (var collision in collisions) {
			foreach (var direction in possibleDirections) {
				if (direction.direction == collision.direction) {
					direction.weight *= collision.normalizedDistance;
				}
			}
		}
		// Lower weight of directions based on the distance to target position (smaller distance, smaller weight) - so that the agent is more focused on the target the closer it is
		float targetDistanceWeight = (Mathf.Clamp(distance, collisionsMinDistance, collisionsMaxDistance) - collisionsMinDistance) / (collisionsMaxDistance - collisionsMinDistance);
		foreach (var direction in possibleDirections) {
			direction.weight *= targetDistanceWeight;
		}
		// Remove all directions with too small weight
		for (int i = possibleDirections.Count - 1; i >= 0; i--) {
			if (possibleDirections[i].weight < directionWeightThreshold)
				possibleDirections.RemoveAt(i);
		}
		// If not possible to avoid collisions, keep the direction to the target as is
		if (possibleDirections.Count == 0) return new CollisionAvoidanceDirection(Vector3.zero, 0);
		// From the remaining directions choose the one which is closest to the direction to target
		CollisionAvoidanceDirection closestDirection = null;
		float closestDistance = float.MaxValue;
		foreach (var direction in possibleDirections) {
			float directionDistance = Vector2.Distance(direction.direction.normalized, targetDirection);
			if (directionDistance < closestDistance) {
				closestDistance = directionDistance;
				closestDirection = direction;
			}
		}
		return closestDirection;
	}

	/// <summary>
	/// Combines direction to target position (given by movement values) with the direction to avoid collisions, using a weighted average.
	/// </summary>
	/// <param name="movement">Movement values leading to the target position.</param>
	/// <param name="direction">Direction to avoid collisions.</param>
	/// <param name="avoidanceWeight">How much weight the collision avoidance should have in the weighted average (original movement values have weight 1).</param>
	/// <returns></returns>
	protected CharacterMovementValues CombineMovementWithCollisionAvoidance(CharacterMovementValues movement, CollisionAvoidanceDirection direction, float avoidanceWeight = 1f) {
		// Combine direction to target with the direction to avoid collisions (weighted average)
		if (debugLogs) Debug.Log($"Movement before collision avoidance: {movement.yawMotion} is {movement.yawValue}, {movement.pitchMotion} is {movement.pitchValue} and {movement.forwardMotion} is {movement.forwardValue}.");
		float yaw = ((int)movement.yawMotion * movement.yawValue + direction.direction.x * direction.weight * avoidanceWeight) / (1f + avoidanceWeight);
		float pitch = ((int)movement.pitchMotion * movement.pitchValue + direction.direction.y * direction.weight * avoidanceWeight) / (1f + avoidanceWeight);
		if (debugLogs) Debug.Log($"---- yaw is {yaw}, pitch is {pitch}");
		if (Mathf.Abs(yaw) < Mathf.Epsilon) movement.yawMotion = YawMotion.None;
		else movement.yawMotion = (YawMotion)Mathf.RoundToInt(yaw / Mathf.Abs(yaw));
		movement.yawValue = Mathf.Abs(yaw);
		if (Mathf.Abs(pitch) < Mathf.Epsilon) movement.pitchMotion = PitchMotion.None;
		else movement.pitchMotion = (PitchMotion)Mathf.RoundToInt(pitch / Mathf.Abs(pitch));
		movement.pitchValue = Mathf.Abs(pitch);
		if (debugLogs) Debug.Log($"Movement after collision avoidance: {movement.yawMotion} is {movement.yawValue}, {movement.pitchMotion} is {movement.pitchValue} and {movement.forwardMotion} is {movement.forwardValue}.");
		// Return the new movement values
		return movement;
	}

	private void Start() {
		collisionDetection = GetComponent<RaycastCollisionDetection>();
	}

}

/// <summary>
/// A class associating direction to avoid collision with its weight (which is given e.g. by collision distance).
/// </summary>
public class CollisionAvoidanceDirection {
	/// <summary>Collision avoidance direction, omiting forward direction (only left, right, up or down).</summary>
	public Vector2 direction;
	/// <summary>Weight of this direction in collision avoidance, given e.g. by collision distance.</summary>
	public float weight;

	public CollisionAvoidanceDirection(Vector2 direction) : this(direction, 1) {
	}

	public CollisionAvoidanceDirection(Vector2 direction, float weight) {
		this.direction = direction;
		this.weight = weight;
	}
}